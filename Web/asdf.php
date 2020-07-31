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

class Player {
    public $LastName;
    public $FirstName;
    public $Name1;
    public $Name2;
    public $GHIN;
    public $Winnings;
    public $TournamentsPlayed;
    public $TournamentsWithWinnings;
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
            $player->Name1 = $lastName . ', ' . $firstName;
            // Before 2016, names had 2 spaces after the comma
            $player->Name2 = $lastName . ',  ' . $firstName;
            $player->GHIN = $ghin;
            $player->Winnings = 0;
            $player->TournamentsPlayed = 0;
            $player->TournamentsWithWinnings = 0;
            $players[] = $player;
        }
    }

    
    // Capture each players winnings
    for($i = 0; $i < count($players); ++$i){
        // Tournaments 16 and 17 have bad data, so skip them
        $chitsResults = GetChitsResultsByName($connection, $players[$i]->Name1, $players[$i]->Name2, 16, 17);

        $winnings = 0;
        for($j = 0; $j < count ( $chitsResults ); ++ $j) {
            $winnings = $winnings + $chitsResults[$j]->Winnings;
        }
        $players[$i]->Winnings = $winnings;
        $players[$i]->TournamentsWithWinnings = count($chitsResults);
    }

    
    // Capture how many tournaments they played in that have scores, skipping
    // those players with no winnings
    for($i = 0; $i < count($players); ++$i){
        //if($players[$i]->Winnings > 0){
            // Tournaments 16 and 17 have bad data, so skip them
            $scores = GetScoresResultsByName($connection, $players[$i]->Name1, $players[$i]->Name2, 16, 17);
            $players[$i]->TournamentsPlayed = count($scores);
        //}
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

            echo '<td>' . $players[$i]->Name1 . "</td>" . PHP_EOL;
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

function DisplayIndividualGhin($connection, $ghin){

    $player = GetRosterEntry($connection, $ghin);
    if(empty($player)){
        echo $ghin . " is not a member";
        return;
    }
    $name = $player->LastName . ", " . $player->FirstName;
    // prior to 2016, the names had 2 spaces after the comma
    $name2 = $player->LastName . ",  " . $player->FirstName; 
    
    echo '<h2 style="text-align:center">Winnings for ' . $name . '</h2>' . PHP_EOL;

    // Tournaments 16 and 17 have bad data, so skip them
    $chitsResults = GetChitsResultsByName($connection, $name, $name2, 16, 17);

    echo '<table style="margin-left:auto;margin-right:auto">' . PHP_EOL;
    echo '<thead><tr class="header"><th>Date</th><th>Tournament</th><th>Winnings</th></tr></thead>' . PHP_EOL;
    echo '<tbody>' . PHP_EOL;
    
    $winnings = 0;
    $totalWinnings = 0;
    $currentYear = "";
    $yearArray = array();
    $winningsArray = array();
    for($i = 0; $i < count ( $chitsResults ); ++ $i) {
        $tournament = GetTournament($connection, $chitsResults[$i]->TournamentKey);
    
        if (($i % 2) == 0) {
            echo '<tr class="d1">' . PHP_EOL;
        } else {
            echo '<tr class="d0">' . PHP_EOL;
        }
        echo '<td>' . $chitsResults[$i]->Date . '</td>' . PHP_EOL;
        echo '<td>' . $tournament->Name . '</td>' . PHP_EOL;
        echo '<td style="text-align:center">$' . $chitsResults[$i]->Winnings . "</td>" . PHP_EOL;
        echo '</tr>' . PHP_EOL;
    
        $winnings = $winnings + $chitsResults[$i]->Winnings;
    
        list($year, $month, $day) = explode("-", $chitsResults[$i]->Date);
        if(empty($currentYear)){
            $currentYear = $year;
            
        } else if($currentYear != $year){
            $yearArray[] = $currentYear;
            $winningsArray[] = $winnings;
            $winnings = 0;
            $currentYear = $year;
        }
    
        $totalWinnings = $totalWinnings + $chitsResults[$i]->Winnings;
    }
    
    if(!empty($currentYear)){
        $yearArray[] = $currentYear;
        $winningsArray[] = $winnings;
    }
    
    /*
    for($i = 0; $i < count($yearArray); ++$i){
        echo '<tr><td>Total for ' . $yearArray[$i] . '</td><td></td><td style="text-align:center">$' . $winningsArray[$i]. "</td></tr>" . PHP_EOL;
    }
    */
    
    if((count ( $chitsResults ) % 2) == 0){
        echo '<tr class="d1">' . PHP_EOL;
    } else {
        echo '<tr class="d0">' . PHP_EOL;
    }
    echo '<td></td><td>Total for ' . count ( $chitsResults ) . ' tournaments</td><td style="text-align:center">$' . $totalWinnings . "</td>" . PHP_EOL;
    echo '</tr>' . PHP_EOL;
    
    echo '</tbody></table>' . PHP_EOL;

    // show tournaments played
    $scores = GetScoresResultsByName($connection, $name, $name2, 16, 17);

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