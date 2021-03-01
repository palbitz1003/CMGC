<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/signup functions.php';
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
	$signups = array();
	for($i = 0; $i < count ( $_POST ['TeeTime'] ); ++ $i) {
		$teeTime = new DatabaseTeeTime ();
		$teeTime->StartTime = $_POST ['TeeTime'] [$i] ['StartTime'];
		$teeTime->StartHole = $_POST ['TeeTime'] [$i] ['StartHole'];
		$tournamentKey = $_POST ['TeeTime'] [$i] ['TournamentKey'];
		
		for($player = 0; $player < count ( $_POST ['TeeTime'] [$i] ['Player'] ); ++ $player) {
			$playerName = FixNameCasing($_POST ['TeeTime'] [$i] ['Player'] [$player]);
			$teeTime->Players [] = $playerName;
			$teeTime->GHIN [] = $_POST ['TeeTime'] [$i] ['GHIN'] [$player];
			$teeTime->Extra [] = $_POST ['TeeTime'] [$i] ['Extra'] [$player];

			// We could look up the signup key, instead of having it passed in here, but that 
			// requires that the GHIN number is always non-zero.  If GHIN number 0 is allowed, then
			// searching for the GHIN among the players signed up for this tournament would result in multiple matches
			// and we would not be able to pair up the player to the signup to see if they have paid.
			$teeTime->SignupKey [] = $_POST ['TeeTime'] [$i] ['SignupKey'] [$player];

			// Save the player in a 2 dimensional array indexed by the signup key for matching up to
			// the signups below
			$signup = new PlayerSignUpClass();
			$signup->SignUpKey = $_POST ['TeeTime'] [$i] ['SignupKey'] [$player];
			$signup->LastName = $playerName;
			$signup->GHIN = intval($_POST ['TeeTime'] [$i] ['GHIN'] [$player]);  // convert to int for comparison later
			$signups[$signup->SignUpKey][] = $signup;
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
				InsertTeeTimePlayer ( $connection, $teeTimeKey, $tournamentKey, 
					$teeTimes [$i]->GHIN [$player], $teeTimes [$i]->Players [$player], $teeTimes [$i]->Extra [$player], $player, $teeTimes [$i]->SignupKey [$player] );
			}
		}
	}

	//var_dump($signups);

	// Test that the tee times and signup data match up
	foreach($signups as $signupKey => $signupPlayers) {
		//echo $signupKey . " => ";
		$dbSignups = GetPlayersForSignUp($connection, $signupKey);
		if(empty($dbSignups)){
			echo "error: signup key " . $signupKey . " not found<br>";
		}
		else {
			for($i = 0; $i < count ($signupPlayers); ++$i){
				//echo $signupPlayers[$i]->LastName . "    ";
				$playerFound = false;
				for($dbi = 0; ($dbi < count($dbSignups) && !$playerFound); ++ $dbi){
					//echo "comparing " . $signupPlayers[$i]->GHIN . " (" . gettype($signupPlayers[$i]->GHIN) . ") to " . $dbSignups[$dbi]->GHIN . " (" . gettype($dbSignups[$dbi]->GHIN) . ")<br>";
					$playerFound = $signupPlayers[$i]->GHIN === $dbSignups[$dbi]->GHIN;
				}
				if(!$playerFound){
					echo "Failed to find " . $signupPlayers[$i]->GHIN . " in player list for signup " . $signupKey . "<br>";
				}
			}

			if(count($signupPlayers) != count($dbSignups))
			{
				echo "Some players have been removed from signup " . $signupKey . "<br>";
			}
		}
		//echo "<br>";
	}
	
	$date = date ( 'Y-m-d' );
	UpdateTournamentResultsField ( $connection, $tournamentKey, 'TeeTimesPostedDate', $date, 's' );
}

$connection->close ();
echo 'Success';
?>