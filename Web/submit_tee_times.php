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

	$tournamentKey = $_POST ['TeeTime'] [0] ['TournamentKey'];
	if (! $tournamentKey || !is_numeric($tournamentKey)) {
		die ( "Missing tournament key" );
	}

	if (!file_exists($default_log_folder)) {
		mkdir($default_log_folder, 0755, true);
	}
	$logFile = $default_log_folder . "/TeeTimes." . $tournamentKey . ".log";

	$signups = array();
	$errors = false;
	for($i = 0; $i < count ( $_POST ['TeeTime'] ); ++ $i) {
		$teeTime = new DatabaseTeeTime ();
		$teeTime->StartTime = $_POST ['TeeTime'] [$i] ['StartTime'];
		$teeTime->StartHole = $_POST ['TeeTime'] [$i] ['StartHole'];
		
		// All the tee times are submitted. Some may be empty
		if(!empty($_POST ['TeeTime'] [$i] ['Player'])){
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
				
				// Get the signup data for the player. This just gets the data for the individual player, not all
				// the players in the signup group.
				$signup = GetPlayerSignUp($connection, $tournamentKey, $_POST ['TeeTime'] [$i] ['GHIN'] [$player]);

				if(!empty($signup)){

					$teeTime->SignupKey [] = $signup->SignUpKey;

					// Save the player in a 2 dimensional array indexed by the signup key for matching up to
					// the signups below
					$signups[$signup->SignUpKey][] = $signup;
				}
				else{
					// This case can only happen if a player is added without first signing up. One
					// could argue this shouldn't happen, but handle it if it does.
					echo "Added player to signup list: " . $playerName . " (" . $_POST ['TeeTime'] [$i] ['GHIN'] [$player] . ")<br>";

					if(!empty($logFile)){
						error_log(date ( '[Y-m-d H:i e] ' ) . "Player added to signups: " . $playerName . " (" . $_POST ['TeeTime'] [$i] ['GHIN'] [$player] . ")" . PHP_EOL, 3, $logFile);
					}

					$accessCode = rand(1000, 9999);
					$t = GetTournament($connection, $tournamentKey);

					$paymentRequired = $t->RequirePayment;
					$cost = $t->Cost;
					if(!$paymentRequired){
						$cost = 0;
					}

					// Create an individual signup so this player can pay
					$insertId = InsertSignUp ( $connection, $tournamentKey, "None", $cost, $accessCode, $paymentRequired);
					$ghin = array($_POST ['TeeTime'] [$i] ['GHIN'] [$player]);
					$fullName = array($playerName);
					$extra = array($_POST ['TeeTime'] [$i] ['Extra'] [$player]);
					InsertSignUpPlayers ( $connection, $tournamentKey, $insertId, $ghin, $fullName, $extra);

					// Fill in signup key after creating the signup record 
					$teeTime->SignupKey [] = $insertId;

					// Save the player in a 2 dimensional array indexed by the signup key for matching up to
					// the signups below
					$signups[$insertId][] = GetPlayerSignUp($connection, $tournamentKey, $_POST ['TeeTime'] [$i] ['GHIN'] [$player]);
				}
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
			echo "error: while checking that tee time players match signup list groups, signup key (" . $signupKey . ") was not found<br>";
		}
		else {
			// If the counts do not match at this point, that means that the signup has more players than have a tee time.
			// I.e.: 4 players signed up, but only 3 of them have been given tee times.
			// Instead of just reducing the signup count to match the tee time assignments, split the signup group into
			// singles. But, the only reason to split the group up is for payments. So, if they've already paid, just ignore
			// the dropped players.
			if(count($signupPlayers) < count($dbSignups)){
				//echo "Some players have been removed from signup " . $signupKey . "<br>";
				$dbSignup = GetSignup($connection, $signupKey);

				// If they have already paid, there is no need to break up the signup.
				if($dbSignup->Payment == 0){

					if(!empty($logFile)){
						error_log(date ( '[Y-m-d H:i e] ' ) . "Splitting up group signup (signup key " . $signupKey . ") into individual signups" . PHP_EOL, 3, $logFile);
					}

					$singleEntryFees = $dbSignup->PaymentDue / count($dbSignups);

					for($dbi = 0; $dbi < count($dbSignups); ++ $dbi){
						// Create an individual signup per player using the other data from the original signup
						$insertId = InsertSignUp ( $connection, $dbSignup->TournamentKey, $dbSignup->RequestedTime, $singleEntryFees, $dbSignup->AccessCode, $dbSignup->PaymentEnabled);
						$ghin = array($dbSignups[$dbi]->GHIN);
						$fullName = array($dbSignups[$dbi]->LastName);
						$extra = array($dbSignups[$dbi]->Extra);

						// Remove the player from the original signup before adding them for the new signup. Otherwise
						// the player is removed from both signups, since this method is not keying on the signup key
						RemoveSignedUpPlayer($connection, $dbSignup->TournamentKey, $dbSignups[$dbi]->GHIN, $dbSignups[$dbi]->LastName);

						InsertSignUpPlayers ( $connection, $dbSignup->TournamentKey, $insertId, $ghin, $fullName, $extra );
						//echo "created individual signup for " . $dbSignups[$dbi]->LastName . "<br>";

						if(!empty($logFile)){
							error_log(date ( '[Y-m-d H:i e] ' ) . "Created individual signup for " . $dbSignups[$dbi]->LastName . "(" . $dbSignups[$dbi]->GHIN . ")" . PHP_EOL, 3, $logFile);
						}

						// Fix up the saved signup key for this player in the tee time list. It's tedious, but you have to
						// go through the complete tee time list to find the player.  
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

	// Handle the waitlist
	ClearTableWithTournamentKey ( $connection, 'SignUpsWaitingList', $tournamentKey);
	if(!empty($_POST ['SignUpsWaitingList'])){
		for($i = 0; $i < count ( $_POST ['SignUpsWaitingList'] ); ++ $i) {
			$signUpWaitingList = new SignUpWaitingListClass();
			$signUpWaitingList->TournamentKey = $tournamentKey;
			$signUpWaitingList->Position = $_POST ['SignUpsWaitingList'] [$i] ['Position'];
			$signUpWaitingList->GHIN1 = $_POST ['SignUpsWaitingList'] [$i] ['GHIN1'];
			$signUpWaitingList->Name1 = $_POST ['SignUpsWaitingList'] [$i] ['Name1'];

			$signUpWaitingList->GHIN2 = "";
			$signUpWaitingList->Name2 = "";
			$signUpWaitingList->GHIN3 = "";
			$signUpWaitingList->Name3 = "";
			$signUpWaitingList->GHIN4 = "";
			$signUpWaitingList->Name4 = "";

			InsertSignUpWaitingListEntry($connection, $signUpWaitingList);

			// Turn off payment enabled for anyone on the waitlist
			$signup = GetPlayerSignUp($connection, $tournamentKey, $_POST ['SignUpsWaitingList'] [$i] ['GHIN1']);
			if(!empty($signup)){
				UpdateSignup($connection, $signup->SignUpKey, 'PaymentEnabled', 0, 'i');
			}
		}
	}

	// Handle the cancellations
	ClearTableWithTournamentKey ( $connection, 'TeeTimesCancelled', $tournamentKey );
	if(!empty($_POST ['CancelledPlayer'])){
		for($i = 0; $i < count ( $_POST ['CancelledPlayer'] ); ++ $i) {
			$teeTimeCancelledPlayer = new TeeTimeCancelledPlayer();
			$teeTimeCancelledPlayer->TournamentKey = $tournamentKey;
			$teeTimeCancelledPlayer->Position = $_POST ['CancelledPlayer'] [$i] ['Position'];
			$teeTimeCancelledPlayer->GHIN = $_POST ['CancelledPlayer'] [$i] ['GHIN'];
			$teeTimeCancelledPlayer->Name = $_POST ['CancelledPlayer'] [$i] ['Name'];

			InsertTeeTimeCancelledPlayer($connection, $teeTimeCancelledPlayer);
		}
	}

	
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