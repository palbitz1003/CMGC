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
	$errors = false;
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
			//$teeTime->SignupKey [] = $_POST ['TeeTime'] [$i] ['SignupKey'] [$player];

			if(intval($_POST ['TeeTime'] [$i] ['GHIN'] [$player]) === 0 ){
				echo "GHIN number for " . $playerName . " is 0. GHIN value 0 is not supported because it cannot be looked up in the signup list.<br>";
				$errors = true;
			}
			
			$signup = GetPlayerSignUp($connection, $tournamentKey, $_POST ['TeeTime'] [$i] ['GHIN'] [$player]);

			if(!empty($signup)){
				//if($signup->SignUpKey !== intval($_POST ['TeeTime'] [$i] ['SignupKey'] [$player])){
				//	echo "GHIN mismatch: signup key (from GHIN) " . $signup->SignUpKey . " does not match uploaded signup key " . $_POST ['TeeTime'] [$i] ['SignupKey'] [$player] . "<br>";
				//}

				$teeTime->SignupKey [] = $signup->SignUpKey;

				// Save the player in a 2 dimensional array indexed by the signup key for matching up to
				// the signups below
				$signups[$signup->SignUpKey][] = $signup;
			}
			else{
				echo "Failed to find signup for " . $playerName . " (" . $_POST ['TeeTime'] [$i] ['GHIN'] [$player] . ")<br>";
				$errors = true; // for now ...

				// Fill in signup key after creating a signup record 
				$teeTime->SignupKey [] = 0;
				$signups[$signup->SignUpKey][] = null;
			}
		}
		
		$teeTimes [] = $teeTime;
	}

	if($errors){
		echo "Failed to load tee times";
		return;
	}
	// echo "tournament key is: " . $tournamentKey;

	// Test that the tee times and signup counts match up
	foreach($signups as $signupKey => $signupPlayers) {
		//echo $signupKey . " => ";
		$dbSignups = GetPlayersForSignUp($connection, $signupKey);
		if(empty($dbSignups)){
			echo "error: signup key " . $signupKey . " not found<br>";
		}
		else {
			// If the counts do not match at this point, that means that the signup has more players than have a tee time.
			// I.e.: 4 players signed up, but only 3 of them have been given tee times.
			if(count($signupPlayers) < count($dbSignups)){
				//echo "Some players have been removed from signup " . $signupKey . "<br>";
				$dbSignup = GetSignup($connection, $signupKey);

				// If they have already paid, there is no need to break up the signup.
				if($dbSignups->Payment == 0){
					$singleEntryFees = $dbSignup->PaymentDue / count($dbSignups);

					for($dbi = 0; $dbi < count($dbSignups); ++ $dbi){
						// Create an individual signup per player
						$insertId = InsertSignUp ( $connection, $dbSignup->TournamentKey, $dbSignup->RequestedTime, $singleEntryFees, $dbSignup->AccessCode, $dbSignup->PaymentEnabled);
						$ghin = array($dbSignups[$dbi]->GHIN);
						$fullName = array($dbSignups[$dbi]->LastName);
						$extra = array($dbSignups[$dbi]->Extra);

						// Remove the player from the original signup before adding them for the new signup. Otherwise
						// the player is removed from both signups, since this method is not keying on the signup key
						RemoveSignedUpPlayer($connection, $dbSignup->TournamentKey, $dbSignups[$dbi]->GHIN, $dbSignups[$dbi]->LastName);

						InsertSignUpPlayers ( $connection, $dbSignup->TournamentKey, $insertId, $ghin, $fullName, $extra );
						//echo "created individual signup for " . $dbSignups[$dbi]->LastName . "<br>";

						// Fix up the saved signup key for this player in the tee time list
						$fixedKey = false;
						for($i = 0; ($i < count ( $teeTimes )) && !$fixedKey; ++ $i) {
							if ($teeTimes [$i]->Players) {
								for($player = 0; ($player < count ( $teeTimes [$i]->Players )) && !$fixedKey; ++ $player) {
									if(intval($teeTimes [$i]->GHIN [$player]) === intval($dbSignups[$dbi]->GHIN)){
										//echo "changed signup key from " . $teeTimes [$i]->SignupKey [$player] . " to " . $insertId . " for " . $teeTimes [$i]->Players [$player];
										$teeTimes [$i]->SignupKey [$player] = $insertId;
										$fixedKey = true;
									}
								}
							}
						}

						
					}
					DeleteSignup($connection, $signupKey);
				}
			}
		}
	}
	
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

	
	$date = date ( 'Y-m-d' );
	UpdateTournamentResultsField ( $connection, $tournamentKey, 'TeeTimesPostedDate', $date, 's' );
}

$connection->close ();
if($errors){
	echo "Loaded tee times, but there were errors";
}
else {
	echo 'Success';
}
?>