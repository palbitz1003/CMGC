<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/results_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/signup functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

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
 
 class GhinIndex {
     public $GHIN;
     public $Index;
 }

$connection = new mysqli ('p:' . $db_hostname, $db_username, $db_password, $db_database );

$overrideTitle = "Winnings";

$end = new DateTime ( "now" );
$start = new DateTime ( "now" );

$year = intval(date("Y"));
$dateRangeYears = array();
//$dateRangeYears[] = $year + 1; // empty year for testing
for($i = 0; $i < 3; $i++){
    $dateRangeYears[] = $year - $i;
}

if (isset ( $_POST ['RequestedTimeRange'] )) {
    if($_POST ['RequestedTimeRange'] === "Last 1 Year"){
        $start->sub(new DateInterval ( 'P1Y' ));
        $startDate = $start->format("Y-m-d");
        $endDate = $end->format("Y-m-d");
        $dateRange = "Last 1 Year";
    }
    else if($_POST ['RequestedTimeRange'] === "Last 2 Years"){
        $start->sub(new DateInterval ( 'P2Y' ));
        $startDate = $start->format("Y-m-d");
        $endDate = $end->format("Y-m-d");
        $dateRange = "Last 2 Years";
    }
    else if(is_numeric($_POST ['RequestedTimeRange'])) {
        $startDate = $_POST ['RequestedTimeRange'] . "-1-1";
        $endDate = $_POST ['RequestedTimeRange'] . "-12-31";
        $dateRange = $_POST ['RequestedTimeRange'];
    } else {
        // unexpected choice
        get_header ();
        echo 'unexpected date range choice ' . $_POST ['RequestedTimeRange'] . '<br>';
        get_footer ();
        return;
    }
    
} else {
    // Default to last 2 years
    $start->sub(new DateInterval ( 'P2Y' ));
    $startDate = $start->format("Y-m-d");
    $endDate = $end->format("Y-m-d");
    $dateRange = "Last 2 Years";

    // allow parameters on command line
    /*
    if(!empty($_GET['start'])){
        $date = $_GET['start'];
        list($y, $m, $d) = array_pad(explode('-', $date, 3), 3, 0);
        if(!ctype_digit("$y") || !ctype_digit("$m") || !ctype_digit("$d") || !checkdate($m, $d, $y)){
            get_header ();
            echo "Invalid start date format: \"" . $date . "\" (expected YYYY-MM-DD)";
            get_footer ();
            return;
        }
        $startDate = $date;
        $dateRange = $startDate . " to " . $endDate;
    }
    //echo "start date " . $startDate . "<br>";
    
    if(!empty($_GET['end'])){
        $date = $_GET['end'];
        list($y, $m, $d) = array_pad(explode('-', $date, 3), 3, 0);
        if(!ctype_digit("$y") || !ctype_digit("$m") || !ctype_digit("$d") || !checkdate($m, $d, $y)){
            get_header ();
            echo "Invalid end date format: \"" . $date . "\" (expected YYYY-MM-DD)";
            get_footer ();
            return;
        }
        $endDate = $date;
        $dateRange = $startDate . " to " . $endDate;
    }
    */
}

$ghin = "all";
if (isset ( $_POST ['ghin'] )) {
    if(is_numeric($_POST ['ghin'])){
        $ghin = trim($_POST ['ghin']);
    }
}
else {
    if(!empty($_GET['ghin']) && is_numeric($_GET ['ghin'])){
        $ghin = trim($_GET ['ghin']);
    }
}

//echo "end date " . $endDate . "<br>";

get_header ();

echo ' <div id="content-container" class="entry-content">';
echo '    <div id="content" role="main">' . PHP_EOL;

// To measure timing
$start = microtime(true);

$players = GetRoster($connection);

//$duration = microtime(true) - $start;
//echo "elapsed time for roster " . $duration . " seconds<br>";

$namesToGhin = NamesToGhin($players);

$chitsArray = GetAllChits($connection, $namesToGhin);

//$duration = microtime(true) - $start;
//echo "elapsed time for chit results " . $duration . " seconds<br>";

$scoresArray = GetAllScoresResults($connection, $namesToGhin);

//$duration = microtime(true) - $start;
//echo "elapsed time for score results " . $duration . " seconds<br>";

echo '<form style="text-align: center" name="input" method="post">'  . PHP_EOL;

echo 'GHIN number or "all" for all players:&nbsp;' . PHP_EOL;
echo '<input type="text" name="ghin" maxlength="10" size="10" value="' . $ghin . '">' . PHP_EOL;

echo '&nbsp;&nbsp;&nbsp;Time Range:&nbsp;' . PHP_EOL;
echo '<select name="RequestedTimeRange">' . PHP_EOL;
if($dateRange === "Last 1 Year"){
    echo '<option selected="selected">Last 1 Year</option>' . PHP_EOL;
} else {
    echo '<option>Last 1 Year</option>' . PHP_EOL;
}
if($dateRange === "Last 2 Years"){
    echo '<option selected="selected">Last 2 Years</option>'  . PHP_EOL;
} else {
    echo '<option>Last 2 Years</option>'  . PHP_EOL;
}
for($i = 0; $i < count($dateRangeYears); $i++){
    if(intval($dateRange) === $dateRangeYears[$i]){
        echo '<option selected="selected">' . $dateRangeYears[$i] . '</option>'  . PHP_EOL;
    } else {
        echo '<option>' . $dateRangeYears[$i] . '</option>'  . PHP_EOL;
    }
}
echo '</select>' . PHP_EOL;

echo '&nbsp;&nbsp;&nbsp;';
echo '<input type="submit" value="Submit"> <br> <br>' . PHP_EOL;
echo '</form>' . PHP_EOL;

if($ghin === "all"){
    DisplayWinningsForAll($connection, $players, $chitsArray, $scoresArray, $startDate, $endDate, $dateRange);
} else {
    DisplayIndividualGhin($connection, $ghin, $chitsArray, $scoresArray, $startDate, $endDate, $dateRange);
}

function DisplayIndividualGhin($connection, $ghin, $chitsArray, $scoresArray, $startDate, $endDate, $dateRange){
    $rosterPlayer = GetRosterEntry($connection, $ghin);
    if(empty($rosterPlayer)){
        echo '<b style="color:red;">' .  $ghin . " is not a member</b>";
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

    $filteredScores = array();
    if(array_key_exists($player->GHIN, $scoresArray)){
        $filteredScores = FilterScoresArray($scoresArray[$player->GHIN], $startDate, $endDate);
        $player->TournamentsPlayed = count($filteredScores);
    }

    //echo "tournaments played " . $player->TournamentsPlayed . "<br>";

    $chitsResults = Array();
    if(array_key_exists($player->GHIN, $chitsArray)){
        $chitsResults = FilterChitsArray($chitsArray[$player->GHIN], $startDate, $endDate);
    }

    //echo "chits count " . count($chitsResults) . "<br>";

    // Arrange tournament array by key
    $t = GetTournaments($connection, "");
    $tournaments = Array();
    for($i = 0; $i < count($t); ++$i){
        $tournaments[$t[$i]->TournamentKey] = $t[$i];
    }

    $player->TournamentResults = array();
    for($i = 0; $i < count ( $chitsResults ); ++ $i) {
        $results = new TournamentResults();
        $results->Date = $chitsResults[$i]->Date;
        $results->PlayerName = $chitsResults[$i]->Name;
        $results->GHIN = $chitsResults[$i]->GHIN;
        $results->TournamentName = $tournaments[$chitsResults[$i]->TournamentKey]->Name;
        $results->TournamentWinnings = $chitsResults[$i]->Winnings;
        $player->TournamentResults[] = $results;

        // Sum up all the winnings
        $player->Winnings = $player->Winnings + $chitsResults[$i]->Winnings;
        $player->TournamentsWithWinnings++;
    }

    // Sort by tournament date
    if(count($player->TournamentResults) > 0){
        usort($player->TournamentResults, function($a, $b) {
            return strcasecmp($a->Date, $b->Date);
        });
    }

    echo '<h2 style="text-align:center">Winnings for ' . $player->Names[0]  . ' ' . $dateRange . '</h2>' . PHP_EOL;

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
        //echo '<td>' . $player->TournamentResults[$i]->TournamentName . ' (' . $player->TournamentResults[$i]->PlayerName . ' ' . $player->TournamentResults[$i]->GHIN . ')' . '</td>' . PHP_EOL;
        echo '<td>' . $player->TournamentResults[$i]->TournamentName  . '</td>' . PHP_EOL;
        echo '<td style="text-align:center">$' . $player->TournamentResults[$i]->TournamentWinnings . "</td>" . PHP_EOL;
        echo '</tr>' . PHP_EOL;
    }
    
    if((count ( $player->TournamentResults ) % 2) == 0){
        echo '<tr class="d1">' . PHP_EOL;
    } else {
        echo '<tr class="d0">' . PHP_EOL;
    }

    if($player->TournamentsWithWinnings === 0){
        echo '<td></td><td>No tournament winnings</td><td></td>' . PHP_EOL;
    } else {
        echo '<td></td><td>Total for ' . $player->TournamentsWithWinnings . ' tournaments</td><td style="text-align:center">$' . $player->Winnings . "</td>" . PHP_EOL;
    }
    echo '</tr>' . PHP_EOL;
    
    echo '</tbody></table>' . PHP_EOL;

    // Sort by tournament date
    usort($filteredScores, function($a, $b) {
        return strcasecmp($a->Date, $b->Date);
    });

    /*
    echo '<table style="margin-left:auto;margin-right:auto">' . PHP_EOL;
    echo '<thead><tr class="header"><th>Date</th><th>Tournament</th><th>Round 1</th><th>Round 2</th></tr></thead>' . PHP_EOL;
    echo '<tbody>' . PHP_EOL;

    for($i = 0; $i < count ( $filteredScores ); ++ $i) {
        $tournament = $tournaments[$filteredScores[$i]->TournamentKey];
    
        if (($i % 2) == 0) {
            echo '<tr class="d1">' . PHP_EOL;
        } else {
            echo '<tr class="d0">' . PHP_EOL;
        }
        echo '<td>' . $filteredScores[$i]->Date . '</td>' . PHP_EOL;
        echo '<td>' . $tournament->Name . '</td>' . PHP_EOL;
        echo '<td style="text-align:center">' . $filteredScores[$i]->ScoreRound1 . "</td>" . PHP_EOL;
        echo '<td style="text-align:center">' . $filteredScores[$i]->ScoreRound2 . "</td>" . PHP_EOL;
        echo '</tr>' . PHP_EOL;
    }

    if((count ( $filteredScores ) % 2) == 0){
        echo '<tr class="d1">' . PHP_EOL;
    } else {
        echo '<tr class="d0">' . PHP_EOL;
    }
    echo '<td></td><td>Played in ' . count ( $filteredScores ) . ' tournaments</td><td></td><td></td>' . PHP_EOL;
    echo '</tr>' . PHP_EOL;

    echo '</tbody></table>' . PHP_EOL;
    */
}

//$duration = microtime(true) - $start;
//echo "elapsed time " . $duration . " seconds<br>";

echo '    </div><!-- #content -->';
echo ' </div><!-- #content-container -->';

function DisplayWinningsForAll($connection, $players, $chitsArray, $scoresArray, $startDate, $endDate, $dateRange){
    
    for($i = 0; $i < count($players); ++$i){
        if(array_key_exists($players[$i]->GHIN, $scoresArray)){
            $filteredScores = FilterScoresArray($scoresArray[$players[$i]->GHIN], $startDate, $endDate);
            $players[$i]->TournamentsPlayed = count($filteredScores);
        }
        if(array_key_exists($players[$i]->GHIN, $chitsArray)){
            $filteredChits = FilterChitsArray($chitsArray[$players[$i]->GHIN], $startDate, $endDate);
            $players[$i]->TournamentsWithWinnings = count($filteredChits);

            for($j = 0; $j < count($filteredChits); ++$j){
                $players[$i]->Winnings = $players[$i]->Winnings + $filteredChits[$j]->Winnings;
            }
        }
    }

    // Sort by highest winnings first
    usort($players, function($a, $b) {
        if($a->Winnings === $b->Winnings) return $a->TournamentsPlayed < $b->TournamentsPlayed ? 1 : -1;
        return $a->Winnings < $b->Winnings ? 1 : -1;
    });

    echo '<h2 style="text-align:center">Winnings for All Players ' . $dateRange . '</h2>' . PHP_EOL;
    echo '<table style="margin-left:auto;margin-right:auto">' . PHP_EOL;
    echo '<thead><tr class="header"><th>Player</th><th>Winnings</th><th>Events Played</th><th>Events Cashed</th><th>% Cashed</th><th>$ Per Event</th></tr></thead>' . PHP_EOL;
    echo '<tbody>' . PHP_EOL;

    $displayLineNumber = 0;
    for($i = 0; $i < count($players); ++$i){
        if(($players[$i]->TournamentsPlayed > 0) && ($players[$i]->Winnings > 0)){

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
            echo '<td style="text-align:center">' . number_format($percentage, 1) . "%</td>" . PHP_EOL;
            $avgWinnings = $players[$i]->Winnings / $players[$i]->TournamentsPlayed;
            echo '<td style="text-align:center">$' . number_format($avgWinnings, 2) . "</td>" . PHP_EOL;
            echo '</tr>' . PHP_EOL;
        }
    }

    echo '</tbody></table>' . PHP_EOL;
}

function GetRoster($connection){
    $sqlCmd = "SELECT GHIN,LastName,FirstName FROM `Roster` Where `Active` = 1 ORDER BY `LastName` ASC";
    
    $query = $connection->prepare ( $sqlCmd );
		
    if (! $query) {
        die ( $sqlCmd . " prepare failed: " . $connection->error );
    }
    
    if (! $query->execute ()) {
        die ( $sqlCmd . " execute failed: " . $connection->error );
    }

    $query->bind_result ( $ghin, $lastName, $firstName);

    // Get list of active players
    $players = array();
    while ( $query->fetch () ) {
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

    return $players;
}

function NamesToGhin($players){

    $namesToGhin = array();
    for($i = 0; $i < count($players); ++$i){
        for($j = 0; $j < count($players[$i]->Names); ++$j){
            $namesToGhin[strtolower($players[$i]->Names[$j])] = $players[$i]->GHIN;
        }
    }

    return $namesToGhin;
}

function FilterScoresArray($scoresArray, $startDate, $endDate){
    
    // Convert to DateTime so 2020-01-01 is the same as 2020-1-1 for comparison
    $startDateTime = new DateTime($startDate);
    $endDateTime = new DateTime($endDate);

    $filteredArray = array();
    for($i = 0; $i < count($scoresArray); ++$i){
        $tournamentDateTime = new DateTime($scoresArray[$i]->Date);
        if(($tournamentDateTime >= $startDateTime) && ($tournamentDateTime <= $endDateTime)){
            $filteredArray[] = $scoresArray[$i];
        }
    }
    //echo "started with " . count($scoresArray) . " scores; after filtering " . count($filteredArray) . " scores<br>";
    return $filteredArray;
}

function FilterChitsArray($chitsArray, $startDate, $endDate){
    
    // Convert to DateTime so 2020-01-01 is the same as 2020-1-1 for comparison
    $startDateTime = new DateTime($startDate);
    $endDateTime = new DateTime($endDate);

    $filteredArray = array();
    for($i = 0; $i < count($chitsArray); ++$i){
        $tournamentDateTime = new DateTime($chitsArray[$i]->Date);
        if(($tournamentDateTime >= $startDateTime) && ($tournamentDateTime <= $endDateTime)){
            $filteredArray[] = $chitsArray[$i];
        }
    }
    //echo "started with " . count($chitsArray) . " tournaments with chits; after filtering " . count($filteredArray) . " tournaments with chits<br>";
    return $filteredArray;
}

// pass $namesToGhin by reference since new names are added to the array
function GetAllChits($connection, &$namesToGhin){

    $sqlCmd = "SELECT * FROM `Chits` Where `Date` >= '2015/01/01' ORDER BY `Date` ASC";
    //$sqlCmd = "SELECT * FROM `Chits` ORDER BY `Date` ASC";

    $query = $connection->prepare ( $sqlCmd );

    if (! $query) {
        die ( $sqlCmd . " prepare failed: " . $connection->error );
    }

    if (! $query->execute ()) {
        die ( $sqlCmd . " execute failed: " . $connection->error );
    }

    $query->bind_result ( $key, $name, $GHIN, $score, $winnings, $flight, $place, $teamNumber, $date, $flightName);

    $chitsArray = Array();
    while ( $query->fetch () ) {
        // Skip over tournaments with bad data
        if(($key != 16) && ($key != 17)){
            $chits = new Chits();
            $chits->TournamentKey = $key;
            $chits->Name = $name;
            $chits->GHIN = $GHIN;
            $chits->Score = $score;
            $chits->Winnings = $winnings;
            $chits->Flight = $flight;
            $chits->Place = $place;
            $chits->TeamNumber = $teamNumber;
            $chits->Date = $date;
            $chits->FlightName = $flightName;

            if($GHIN != 0){
                $chitsArray[$GHIN][] = $chits;

                // Add the name to the names-to-ghin array if it is not already there
                $lowerName = strtolower($chits->Name);
                if(!array_key_exists($lowerName, $namesToGhin)){
                    //echo "adding " . $lowerName . " for ghin " . $chits->GHIN . "<br>";
                    $namesToGhin[$lowerName] = $chits->GHIN;
                }
            }
            else {
                $zeroGhin[] = $chits;
            }
        }
    }

    // Add in the chits when GHIN is 0, if we can match the name
    for($i = 0; $i < count($zeroGhin); ++ $i){
        $lowerName = strtolower($zeroGhin[$i]->Name);
        if(array_key_exists($lowerName, $namesToGhin)){
            $chitsArray[$namesToGhin[$lowerName]][] = $zeroGhin[$i];
            //echo "Adding chits for " . $lowerName . " to " . $namesToGhin[$lowerName] . "<br>";
        }
    }

    $query->close();

    return $chitsArray;
}

function GetAllScoresResults($connection, $namesToGhin)
{
	$sqlCmd = "SELECT * FROM `Scores` Where `Date` >= '2015/01/01' ORDER BY `Date` ASC";
    //$sqlCmd = "SELECT * FROM `Scores` ORDER BY `Date` ASC";

	$query = $connection->prepare ( $sqlCmd );

	if (! $query) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}

	if (! $query->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}

	$query->bind_result ( $key, $name1, $GHIN1, $name2, $GHIN2 , $name3, $GHIN3, $name4, $GHIN4, $scoreRound1, $scoreRound2, $scoreTotal, $flight, $teamNumber, $date);

	$scoresArray = Array();
    $zeroGhinArray = Array();
	while ( $query->fetch () ) {
		if(($key != 16) && ($key != 17)){
			$scores = new Scores();
			$scores->TournamentKey = $key;
			$scores->Name1 = $name1;
			$scores->GHIN1 = $GHIN1;
			$scores->Name2 = $name2;
			$scores->GHIN2 = $GHIN2;
			$scores->Name3 = $name3;
			$scores->GHIN3 = $GHIN3;
			$scores->Name4 = $name4;
			$scores->GHIN4 = $GHIN4;
			$scores->ScoreRound1 = $scoreRound1;
			$scores->ScoreRound2 = $scoreRound2;
			$scores->ScoreTotal = $scoreTotal;
			$scores->Flight = $flight;
			$scores->TeamNumber = $teamNumber;
			$scores->Date = $date;

            $savedForLater = false;
            // Some of the data has ghin 0, but a name
            if($GHIN1 != 0){
                $scoresArray[$GHIN1][] = $scores;

                // For any valid ghin, map the name to the ghin if it is not already mapped
                $lowerName = strtolower($name1);
                if(!array_key_exists($lowerName, $namesToGhin)){
                    $namesToGhin[$lowerName] =  $GHIN1;
                    //echo "1 mapping " . $lowerName . " to " . $GHIN1 . "<br>";
                }
            } else if(!empty($name1) && !$savedForLater){
                $savedForLater = true;
                $zeroGhinArray[] = $scores;
            }

            if($GHIN2 != 0){
                $scoresArray[$GHIN2][] = $scores;

                // For any valid ghin, map the name to the ghin if it is not already mapped
                $lowerName = strtolower($name2);
                if(!array_key_exists($lowerName, $namesToGhin)){
                    $namesToGhin[$lowerName] =  $GHIN2;
                    //echo "2 mapping " . $lowerName . " to " . $GHIN2 . "<br>";
                }
            } else if(!empty($name2) && !$savedForLater){
                $savedForLater = true;
                $zeroGhinArray[] = $scores;
            }

            if($GHIN3 != 0){
                $scoresArray[$GHIN3][] = $scores;

                // For any valid ghin, map the name to the ghin if it is not already mapped
                $lowerName = strtolower($name3);
                if(!array_key_exists($lowerName, $namesToGhin)){
                    $namesToGhin[$lowerName] =  $GHIN3;
                    //echo "3 mapping " . $lowerName . " to " . $GHIN3 . "<br>";
                }
            } else if(!empty($name3) && !$savedForLater){
                $savedForLater = true;
                $zeroGhinArray[] = $scores;
            }

            if($GHIN4 != 0){
                $scoresArray[$GHIN4][] = $scores;

                // For any valid ghin, map the name to the ghin if it is not already mapped
                $lowerName = strtolower($name4);
                if(!array_key_exists($lowerName, $namesToGhin)){
                    $namesToGhin[$lowerName] =  $GHIN4;
                    //echo "4 mapping " . $lowerName . " to " . $GHIN4 . "<br>";
                }
            } else if(!empty($name4) && !$savedForLater){
                $savedForLater = true;
                $zeroGhinArray[] = $scores;
            }
			
		}
	}

	$query->close();

    //echo "zero ghin scores " . count($zeroGhinArray) . "<br>";
    for($i = 0; $i < count($zeroGhinArray); ++$i){
        if(($zeroGhinArray[$i]->GHIN1 == 0) && !empty($zeroGhinArray[$i]->Name1))
        {
            $lowerName = strtolower($zeroGhinArray[$i]->Name1);
            if(array_key_exists($lowerName, $namesToGhin)){
                $scoresArray[$namesToGhin[$lowerName]][] = $zeroGhinArray[$i];
                //echo "1 added score for " . $lowerName . " ghin " . $namesToGhin[$lowerName] . "<br>";
            }
            else {
                //echo "failed to find " . $lowerName . "<br>";
            }
        }

        if(($zeroGhinArray[$i]->GHIN2 == 0) && !empty($zeroGhinArray[$i]->Name2))
        {
            $lowerName = strtolower($zeroGhinArray[$i]->Name2);
            if(array_key_exists($lowerName, $namesToGhin)){
                $scoresArray[$namesToGhin[$lowerName]][] = $zeroGhinArray[$i];
                //echo "2 added score for " . $lowerName . " ghin " . $namesToGhin[$lowerName] . "<br>";
            }
        }

        if(($zeroGhinArray[$i]->GHIN3 == 0) && !empty($zeroGhinArray[$i]->Name3))
        {
            $lowerName = strtolower($zeroGhinArray[$i]->Name3);
            if(array_key_exists($lowerName, $namesToGhin)){
                $scoresArray[$namesToGhin[$lowerName]][] = $zeroGhinArray[$i];
                //echo "3 added score for " . $lowerName . " ghin " . $namesToGhin[$lowerName] . "<br>";
            }
        }

        if(($zeroGhinArray[$i]->GHIN4 == 0) && !empty($zeroGhinArray[$i]->Name4))
        {
            $lowerName = strtolower($zeroGhinArray[$i]->Name4);
            if(array_key_exists($lowerName, $namesToGhin)){
                $scoresArray[$namesToGhin[$lowerName]][] = $zeroGhinArray[$i];
                //echo "4 added score for " . $lowerName . " ghin " . $namesToGhin[$lowerName] . "<br>";
            }
        }
    }

	return $scoresArray;
}

get_footer ();
?>