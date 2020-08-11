<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/results_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/signup functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$connection = new mysqli ('p:' . $db_hostname, $db_username, $db_password, $db_database );

$overrideTitle = "Winnings";

$ghin = $_GET ['ghin'];
if (! $ghin) {
    get_header ();
    echo 'Please provide ghin number';
    get_footer ();
    return;
}

get_header ();

echo ' <div id="content-container" class="entry-content">';
echo '    <div id="content" role="main">' . PHP_EOL;

if($ghin === "all"){
    DisplayWinningsForAll($connection);
} else {
    DisplayIndividualGhin($connection, $ghin);
}

echo '    </div><!-- #content -->';
echo ' </div><!-- #content-container -->';

class TournamentResults {
   public $Date;
   public $PlayerName;
   public $GHIN;
   public $TournamentName;
   public $TournamentWinnings; 
}

class Player {
    public $LastName;
    public $FirstName;
    public $Names;
    public $GHIN;
    public $Winnings;
    public $TournamentsPlayed;
    public $TournamentsWithWinnings;
    public $TournamentResults;
}



function DisplayWinningsForAll($connection){
    $sqlCmd = "SELECT GHIN,LastName,FirstName,Active FROM `Roster` ORDER BY `LastName` ASC";
    
    $query = $connection->prepare ( $sqlCmd );
		
    if (! $query) {
        die ( $sqlCmd . " prepare failed: " . $connection->error );
    }
    
    if (! $query->execute ()) {
        die ( $sqlCmd . " execute failed: " . $connection->error );
    }

    $query->bind_result ( $ghin, $lastName, $firstName, $active);

    // Get list of active players
    $players = array();
    while ( $query->fetch () ) {
        if($active){
            $player = new Player();
            $player->LastName = $lastName;
            $player->FirstName = $firstName;
            $player->Names[] = $lastName . ', ' . $firstName;
            // Before 2016, names had 2 spaces after the comma
            $player->Names[] = $lastName . ',  ' . $firstName;
            $player->GHIN = $ghin;
            $player->Winnings = 0;
            $player->TournamentsPlayed = 0;
            $player->TournamentsWithWinnings = 0;
            $players[] = $player;
        }
    }

    // Capture each players winnings
    for($i = 0; ($i < count($players)); ++$i){
        GetPlayerData($connection, $players[$i]);
    }
    
    // Capture how many tournaments they played in that have scores
    for($i = 0; $i < count($players); ++$i){

        $players[$i]->TournamentsPlayed = 0;
        for($j = 0; $j < count($players[$i]->Names); ++ $j){
            $newScores = GetScoresResultsByName($connection, $players[$i]->Names[$j], 16, 17);
            //echo $player->Names[$i] . " " . count($newScores) . "<br>";
            $players[$i]->TournamentsPlayed = $players[$i]->TournamentsPlayed + count($newScores);
        }
    }
    
    // Sort by highest winnings first
    usort($players, function($a, $b) {
        if($a->Winnings === $b->Winnings) return $a->TournamentsPlayed < $b->TournamentsPlayed ? 1 : -1;
        return $a->Winnings < $b->Winnings ? 1 : -1;
    });

    echo '<h2 style="text-align:center">Winnings for All Players</h2>' . PHP_EOL;
    echo '<table style="margin-left:auto;margin-right:auto">' . PHP_EOL;
    echo '<thead><tr class="header"><th>Player</th><th>Winnings</th><th>Tournaments Played</th><th>Tournaments With Winnings</th><th>Winning Percentage</th><th>Winnings Per Tournament Entered</th></tr></thead>' . PHP_EOL;
    echo '<tbody>' . PHP_EOL;

    $displayLineNumber = 0;
    for($i = 0; $i < count($players); ++$i){
        if($players[$i]->TournamentsPlayed > 0){

            if (($displayLineNumber % 2) == 0) {
                echo '<tr class="d1">' . PHP_EOL;
            } else {
                echo '<tr class="d0">' . PHP_EOL;
            }
            $displayLineNumber++;

            echo '<td>' . $players[$i]->Names[0] . "</td>" . PHP_EOL;
            echo '<td style="text-align:center">$' . $players[$i]->Winnings . "</td>" . PHP_EOL;
            echo '<td style="text-align:center">' . $players[$i]->TournamentsPlayed . "</td>" . PHP_EOL;
            echo '<td style="text-align:center">' . $players[$i]->TournamentsWithWinnings . "</td>" . PHP_EOL;
            $percentage = ($players[$i]->TournamentsWithWinnings / $players[$i]->TournamentsPlayed) * 100;
            echo '<td style="text-align:center">' . number_format($percentage, 1) . "</td>" . PHP_EOL;
            $avgWinnings = $players[$i]->Winnings / $players[$i]->TournamentsPlayed;
            echo '<td style="text-align:center">$' . number_format($avgWinnings, 2) . "</td>" . PHP_EOL;
            echo '</tr>' . PHP_EOL;
        }
    }

    echo '</tbody></table>' . PHP_EOL;
}

function GetPlayerData($connection, $player){

    $player->TournamentResults = Array();

    // First look up all entries with the provided GHIN number.
    // Tournaments 16 and 17 have bad data, so skip them
    $chitsResults = GetChitsResultsByNameOrGhin($connection, "", $player->GHIN, 16, 17);

    AddPlayerData($connection, $player, $chitsResults);

    // Since we looked up by GHIN, check to see if the roster name
    // has been different in the past
    for($i = 0; $i < count ( $chitsResults ); ++ $i) {
        $found = false;
        for($j = 0; ($j < count($player->Names)) && !$found; ++$j){
            if(strcasecmp($player->Names[$j], $chitsResults[$i]->Name) == 0){
                $found = true;
            }
        }
        if(!$found){
            $elements = explode(",", $chitsResults[$i]->Name, 2);
            if(count($elements) > 1){
                // echo "added " . $chitsResults[$i]->Name . " to " . $player->Names[0] . "<br>";
                //if(strcasecmp($player->LastName, trim($elements[0])) !== 0){
                //    echo "Last name mismatch! adding " . $chitsResults[$i]->Name . " to " . $player->Names[0] . " for " . $chitsResults[$i]->Date . "<br>";
                //}
                $player->Names[] = trim($elements[0]) . ", " . trim($elements[1]);
                $player->Names[] = trim($elements[0]) . ",  " . trim($elements[1]);  // 2 spaces
            }
            else {
                // no comma (shouldn't happen). just add the name itself.
                $player->Names[] = $chitsResults[$i]->Name;
            }
        }
    }

    // Look up all the names with GHIN == 0
    for($i = 0; $i < count($player->Names); ++ $i){
        $chitsResults = GetChitsResultsByNameOrGhin($connection, $player->Names[$i], 0, 16, 17);

        AddPlayerData($connection, $player, $chitsResults); 
    }
}

function AddPlayerData($connection, $player, $chitsResults){

    for($i = 0; $i < count ( $chitsResults ); ++ $i) {
        $tournament = GetTournament($connection, $chitsResults[$i]->TournamentKey);
        $results = new TournamentResults();
        $results->Date = $chitsResults[$i]->Date;
        $results->PlayerName = $chitsResults[$i]->Name;
        $results->GHIN = $chitsResults[$i]->GHIN;
        $results->TournamentName = $tournament->Name;
        $results->TournamentWinnings = $chitsResults[$i]->Winnings;
        $player->TournamentResults[] = $results;

        // Sum up all the winnings
        $player->Winnings = $player->Winnings + $chitsResults[$i]->Winnings;
        $player->TournamentsWithWinnings++;
    }
}

function DisplayIndividualGhin($connection, $ghin){

    $rosterPlayer = GetRosterEntry($connection, $ghin);
    if(empty($rosterPlayer)){
        echo $ghin . " is not a member";
        return;
    }

    $player = new Player();
    $player->LastName = $rosterPlayer->LastName;
    $player->FirstName = $rosterPlayer->FirstName;
    $player->GHIN = $ghin;
    $player->Winnings = 0;
    $player->TournamentsPlayed = 0;
    $player->TournamentsWithWinnings = 0;

    // Default name list ...
    $player->Names = Array();
    $player->Names[] = $rosterPlayer->LastName . ', ' . $rosterPlayer->FirstName;
    // Before 2016, names had 2 spaces after the comma
    $player->Names[] = $rosterPlayer->LastName . ',  ' . $rosterPlayer->FirstName;

    GetPlayerData($connection, $player);

    // Sort by tournament date
    usort($player->TournamentResults, function($a, $b) {
        return strcasecmp($a->Date, $b->Date);
    });
 
    echo '<h2 style="text-align:center">Winnings for ' . $player->Names[0] . '</h2>' . PHP_EOL;

    echo '<table style="margin-left:auto;margin-right:auto">' . PHP_EOL;
    echo '<thead><tr class="header"><th>Date</th><th>Tournament</th><th>Winnings</th></tr></thead>' . PHP_EOL;
    echo '<tbody>' . PHP_EOL;
    
    for($i = 0; $i < count ( $player->TournamentResults ); ++ $i) {
    
        if (($i % 2) == 0) {
            echo '<tr class="d1">' . PHP_EOL;
        } else {
            echo '<tr class="d0">' . PHP_EOL;
        }
        echo '<td>' . $player->TournamentResults[$i]->Date . '</td>' . PHP_EOL;
        echo '<td>' . $player->TournamentResults[$i]->TournamentName . ' (' . $player->TournamentResults[$i]->PlayerName . ' ' . $player->TournamentResults[$i]->GHIN . ')' . '</td>' . PHP_EOL;
        echo '<td style="text-align:center">$' . $player->TournamentResults[$i]->TournamentWinnings . "</td>" . PHP_EOL;
        echo '</tr>' . PHP_EOL;
    }
    
    if((count ( $chitsResults ) % 2) == 0){
        echo '<tr class="d1">' . PHP_EOL;
    } else {
        echo '<tr class="d0">' . PHP_EOL;
    }
    echo '<td></td><td>Total for ' . $player->TournamentsWithWinnings . ' tournaments</td><td style="text-align:center">$' . $player->Winnings . "</td>" . PHP_EOL;
    echo '</tr>' . PHP_EOL;
    
    echo '</tbody></table>' . PHP_EOL;

    // ----------------------------------------------------------------------------------------------------------------
    // show tournaments played
    $scores = Array();
    for($i = 0; $i < count($player->Names); ++ $i){
        $newScores = GetScoresResultsByName($connection, $player->Names[$i], 16, 17);
        //echo $player->Names[$i] . " " . count($newScores) . "<br>";
        if(count($newScores) > 0){
            $scores = array_merge($scores, $newScores);
        }
    }

    // Sort by tournament date
    usort($scores, function($a, $b) {
        return strcasecmp($a->Date, $b->Date);
    });

    echo '<table style="margin-left:auto;margin-right:auto">' . PHP_EOL;
    echo '<thead><tr class="header"><th>Date</th><th>Tournament</th><th>Round 1</th><th>Round 2</th></tr></thead>' . PHP_EOL;
    echo '<tbody>' . PHP_EOL;

    for($i = 0; $i < count ( $scores ); ++ $i) {
        $tournament = GetTournament($connection, $scores[$i]->TournamentKey);
    
        if (($i % 2) == 0) {
            echo '<tr class="d1">' . PHP_EOL;
        } else {
            echo '<tr class="d0">' . PHP_EOL;
        }
        echo '<td>' . $scores[$i]->Date . '</td>' . PHP_EOL;
        echo '<td>' . $tournament->Name . '</td>' . PHP_EOL;
        echo '<td style="text-align:center">' . $scores[$i]->ScoreRound1 . "</td>" . PHP_EOL;
        echo '<td style="text-align:center">' . $scores[$i]->ScoreRound2 . "</td>" . PHP_EOL;
        echo '</tr>' . PHP_EOL;
    }

    if((count ( $scores ) % 2) == 0){
        echo '<tr class="d1">' . PHP_EOL;
    } else {
        echo '<tr class="d0">' . PHP_EOL;
    }
    echo '<td></td><td>Played in ' . count ( $scores ) . ' tournaments</td><td></td><td></td>' . PHP_EOL;
    echo '</tr>' . PHP_EOL;

    echo '</tbody></table>' . PHP_EOL;
}

get_footer ();
?>