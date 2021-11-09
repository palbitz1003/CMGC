<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/signup functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tee times functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

class TournamentAndTeeTimes
{
    public $ElapsedTime;
    public $Tournament;
    public $TeeTimes;
}

// Make local copies of these structures to minimize the json information transferred
class HistoricalTeeTimePlayer {
    public $TeeTimeKey;
	public $GHIN;
	public $LastName;
}

class HistoricalTournament {
	public $Name;
	public $StartDate;
	public $EndDate;
}

// Copy a full tournament structure to the abbreviated structure
function CopyTournament($tournament){
    $historicalTournament = new HistoricalTournament();
    $historicalTournament->Name = $tournament->Name;
    $historicalTournament->StartDate = $tournament->StartDate;
    $historicalTournament->EndDate = $tournament->EndDate;
    return $historicalTournament;
}

// login() requires headers functions.php and wp-blog-header.php
login($_POST ['Login'], $_POST ['Password']);

$connection = new mysqli ('p:' . $db_hostname, $db_username, $db_password, $db_database );

// For testing 
// $tournamentKey = 139;
// $monthsOfData = 12;

$tournamentKey = $_POST ['tournament'];
if (! $tournamentKey || !is_numeric($tournamentKey)) {
	die ( "Which tournament?" );
}

$monthsOfData = $_POST ['months'];
if (! $monthsOfData || !is_numeric($monthsOfData)) {
	die ( "How many months of data?" );
}

if ($connection->connect_error){
	die ( $connection->connect_error );
}

// To measure timing
$start = microtime(true);

// Get all tournaments, sorted by start date
$tournaments = GetTournaments($connection, '');

// Find the tournament starting point
$currentIndex = -1;
for($i = 0; ($i < count($tournaments)) && ($currentIndex == -1); ++$i){
    if($tournamentKey == $tournaments[$i]->TournamentKey){
        $currentIndex = $i;
        $beginningDate = new DateTime ( $tournaments[$i]->StartDate);
        $beginningDate->sub(new DateInterval ( 'P' . $monthsOfData . 'M' ));
    }
}

$tournamentAndTeeTimesArray = array();

if($currentIndex >= 0){
    for($i = $currentIndex - 1; $i >= 0; --$i){
        $startDate = new DateTime ( $tournaments[$i]->StartDate);
        if(!$tournaments[$i]->AnnouncementOnly && ($startDate >= $beginningDate)){
            $tournamentAndTeeTimes = new TournamentAndTeeTimes();
            $tournamentAndTeeTimes->Tournament = CopyTournament($tournaments[$i]); // Copy to a smaller object so less json to encode/decode
            $tournamentAndTeeTimes->TeeTimes = GetTeeTimesLocal($connection, $tournaments[$i]->TournamentKey, $tournamentAndTeeTimes);

            $tournamentAndTeeTimes->ElapsedTime[] = microtime(true) - $start; // save the elapsed timestamp
            $tournamentAndTeeTimesArray[] = $tournamentAndTeeTimes;
        }
    }
}

echo json_encode($tournamentAndTeeTimesArray);

$connection->close ();





// Use a local copy of GetTeeTimes() to reduce the number of sql calls which improves performance
function GetTeeTimesLocal($connection, $tournamentKey, $tournamentAndTeeTimes) {
    //$start = microtime(true);

	$sqlCmd = "SELECT * FROM `TeeTimes` WHERE `TournamentKey` = ? ORDER BY `StartTime` ASC";
	$query = $connection->prepare ( $sqlCmd );
	
	if (! $query) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	if (! $query->bind_param ( 'i', $tournamentKey )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}
	
	if (! $query->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
	
	$query->bind_result ( $key, $tournament, $startTime, $startHole );
	
	$teeTimeArray = array();
	while ( $query->fetch () ) {
		$teeTime = new DatabaseTeeTime ();
		$teeTime->Key = $key;
		$teeTime->StartTime = $startTime;
		$teeTime->StartHole = $startHole;
		$teeTimeArray [] = $teeTime;
	}
	
	$query->close ();
	
	if (! $teeTimeArray || (count ( $teeTimeArray ) == 0)) {
		return;
	}
	
    //$tournamentAndTeeTimes->ElapsedTime[] = microtime(true) - $start;

    $allPlayers = GetAllPlayersForTournament($connection, $tournamentKey);

    //$tournamentAndTeeTimes->ElapsedTime[] = microtime(true) - $start;
    //$tournamentAndTeeTimes->ElapsedTime[] = count($allPlayers);

	for($i = 0; $i < count ( $teeTimeArray ); ++ $i) {
		$teeTimeArray [$i]->Players = GetPlayersForTeeTimeLocal ( $connection, $teeTimeArray [$i]->Key, $allPlayers );
	}
    //$tournamentAndTeeTimes->ElapsedTime[] = microtime(true) - $start;
	
	return $teeTimeArray;
}

// Grab all the players for a tournament in a single call, rather than
// one call per tee time.
function GetAllPlayersForTournament($connection, $tournamentKey) {
    // If you sort by position, when they are added to the player array later, the players
    // will be added in the correct order
    $sqlCmd = "SELECT * FROM `TeeTimesPlayers` WHERE `TournamentKey` = ? ORDER BY `Position` ASC";
	$query = $connection->prepare ( $sqlCmd );
	
	if (! $query) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	if (! $query->bind_param ( 'i', $tournamentKey )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}
	
	if (! $query->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
	
	$query->bind_result ( $key, $tournament, $GHIN, $Name, $Position, $extra, $signupKey );
	
	$playerCount = 0;
	$playerArray = array ();
	while ( $query->fetch () ) {
		$player = new HistoricalTeeTimePlayer ();
        $player->TeeTimeKey = $key;
		$player->GHIN = $GHIN;
		$player->LastName = $Name;
		$playerArray [] = $player;
	}
	
	$query->close ();
	return $playerArray;
}

// Filter out the players for a specific tee time from the full player list.
// The SQL query ordered by position, so the players will be added in the correct order here.
function GetPlayersForTeeTimeLocal($connection, $teeTimeKey, $allPlayers) {

	$playerArray = array ();
    for($i = 0; $i < count($allPlayers); ++$i){
        if($allPlayers[$i]->TeeTimeKey == $teeTimeKey){
            $playerArray[] = $allPlayers[$i];
        }
    }

	return $playerArray;
}

?>