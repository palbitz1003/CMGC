<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tee times functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/results_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$connection = new mysqli ( $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( $connection->connect_error );
	
	// var_dump($_POST);
	
login($_POST ['Login'], $_POST ['Password']);

if (! isset ( $_POST ['TeeTime'] )) {
	die ( "No list of tee times" );
} else if (! isset ( $_POST ['TeeTime'] [0] ['TournamentKey'] )) {
	die ( "Missing tournament key" );
} else {
	for($i = 0; $i < count ( $_POST ['TeeTime'] ); ++ $i) {
		$teeTime = new TeeTime ();
		$teeTime->StartTime = $_POST ['TeeTime'] [$i] ['StartTime'];
		$teeTime->StartHole = $_POST ['TeeTime'] [$i] ['StartHole'];
		$tournamentKey = $_POST ['TeeTime'] [$i] ['TournamentKey'];
		
		for($player = 0; $player < count ( $_POST ['TeeTime'] [$i] ['Player'] ); ++ $player) {
			$teeTime->Players [] = $_POST ['TeeTime'] [$i] ['Player'] [$player];
			$teeTime->GHIN [] = $_POST ['TeeTime'] [$i] ['GHIN'] [$player];
		}
		
		$teeTimes [] = $teeTime;
	}
	// echo "tournament key is: " . $tournamentKey;
	
	ClearTableWithTournamentKey ( $connection, 'TeeTimes', $tournamentKey );
	ClearTableWithTournamentKey ( $connection, 'TeeTimesPlayers', $tournamentKey );
	
	/*
	 * for($i = 0; $i < count($teeTimes); ++$i) { echo $i . ': '; echo $teeTimes[$i]->StartTime . ": "; echo "Hole " . $teeTimes[$i]->StartHole . ": "; if($teeTimes[$i]->Players) { for($player = 0; $player < count($teeTimes[$i]->Players); ++$player) { if($player > 0) echo ", "; echo $teeTimes[$i]->Players[$player]; } echo "\n"; } echo "\n"; }
	 */
	
	for($i = 0; $i < count ( $teeTimes ); ++ $i) {
		$teeTimeKey = InsertTeeTime ( $connection, $tournamentKey, $teeTimes [$i]->StartTime, $teeTimes [$i]->StartHole );
		if ($teeTimes [$i]->Players) {
			for($player = 0; $player < count ( $teeTimes [$i]->Players ); ++ $player) {
				InsertTeeTimePlayer ( $connection, $teeTimeKey, $tournamentKey, $teeTimes [$i]->GHIN [$player], $teeTimes [$i]->Players [$player], $player );
			}
		}
	}
	
	$date = date ( 'Y-m-d' );
	UpdateTournamentResultsField ( $connection, $tournamentKey, 'TeeTimesPostedDate', $date, 's' );
}

$connection->close ();
echo 'Success';
?>