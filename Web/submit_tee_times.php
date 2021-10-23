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

	// Remove players first, so the count of players for each signup are correct for calculations below
	if(!empty($_POST ['Remove'])){
		for($i = 0; $i < count ( $_POST ['Remove'] ); ++ $i) {
			RemovePlayer($connection, $logFile, $tournamentKey, $_POST ['Remove'] [$i] ['GHIN'], $_POST ['Remove'] [$i] ['Name']);
		}
	}

	$signups = array();
	$errors = false;
	$errorMessages = "";
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

				if(intval($_POST ['TeeTime'] [$i] ['GHIN'] [$player]) === 0 ){
					//$errorMessages .= "GHIN number for " . $playerName . " is 0. GHIN value 0 is not supported because it cannot be looked up in the signup list.<br>";
					//$errors = true;

					$signup = GetPlayerSignUpByName($connection, $tournamentKey, $_POST ['TeeTime'] [$i] ['Player'] [$player]);

					if(!empty($signup)){
						//die("Found player " . $signup->LastName);
					}
					else {
						//die("Failed to find player " . $_POST ['TeeTime'] [$i] ['Player'] [$player]);
					}
				}
				else {
					// Get the signup data for the player. This just gets the data for the individual player, not all
					// the players in the signup group.
					$signup = GetPlayerSignUp($connection, $tournamentKey, $_POST ['TeeTime'] [$i] ['GHIN'] [$player]);
				}
				
				if(!empty($signup)){

					$teeTime->SignupKey [] = $signup->SignUpKey;

					// Save the player in a 2 dimensional array indexed by the signup key for matching up to
					// the signups below
					$signups[$signup->SignUpKey][] = $signup;
				}
				else{
					// This case can only happen if a player is added without first signing up. One
					// could argue this shouldn't happen, but handle it if it does.
					// echo "Added player to signup list: " . $playerName . " (" . $_POST ['TeeTime'] [$i] ['GHIN'] [$player] . ")<br>";

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
					if(intval($_POST ['TeeTime'] [$i] ['GHIN'] [$player]) === 0){
						$signups[$insertId][] = GetPlayerSignUpByName($connection, $tournamentKey, $_POST ['TeeTime'] [$i] ['Player'] [$player]);
					} 
					else {
						$signups[$insertId][] = GetPlayerSignUp($connection, $tournamentKey, $_POST ['TeeTime'] [$i] ['GHIN'] [$player]);
					}
				}
			}
		}
		
		$teeTimes [] = $teeTime;
	}

	if($errors){
		echo $errorMessages;
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

										// There may be more than 1 with GHIN 0, so check name
										if(intval($teeTimes [$i]->GHIN [$player]) === 0){
											//echo "checking for " . $dbSignups[$dbi]->LastName . " matching " . $teeTimes [$i]->Players [$player] . "<br>";
											if(strcasecmp($dbSignups[$dbi]->LastName, $teeTimes [$i]->Players [$player]) == 0){
												//echo "Matched by name<br>"; 
												//echo "changed signup key from " . $teeTimes [$i]->SignupKey [$player] . " to " . $insertId . " for " . $teeTimes [$i]->Players [$player];
												$teeTimes [$i]->SignupKey [$player] = $insertId;
												$fixedKey = true;
											}
										}
										else {
											//echo "changed signup key from " . $teeTimes [$i]->SignupKey [$player] . " to " . $insertId . " for " . $teeTimes [$i]->Players [$player];
											$teeTimes [$i]->SignupKey [$player] = $insertId;
											$fixedKey = true;
										}
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
			if(intval($_POST ['SignUpsWaitingList'] [$i] ['GHIN1']) === 0){
				$signup = GetPlayerSignUpByName($connection, $tournamentKey, $_POST ['SignUpsWaitingList'] [$i] ['Name1']);
			}
			else {
				$signup = GetPlayerSignUp($connection, $tournamentKey, $_POST ['SignUpsWaitingList'] [$i] ['GHIN1']);
			}
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
	echo "Loaded tee times, but there were errors: " . $errorMessages;
}
else {
	echo 'Success';
}

function RemovePlayer($connection, $logFile, $tournamentKey, $ghin, $name)
{
	// Get the signup data for the player. This just gets the data for the individual player, not all
	// the players in the signup group.  Guests in member guest may have GHIN 0.
	if(intval($ghin) === 0){
		$removeSignupPlayer = GetPlayerSignUpByName($connection, $tournamentKey, $name);
	}
	else {
		$removeSignupPlayer = GetPlayerSignUp($connection, $tournamentKey, $ghin);
	}
	if(!empty($removeSignupPlayer)){
		
		RemoveSignedUpPlayer ( $connection, $tournamentKey, $removeSignupPlayer->GHIN, $removeSignupPlayer->LastName );

		if(!empty($logFile)){
			error_log(date ( '[Y-m-d H:i e] ' ) . "Removed player: " . $name . " (" . $ghin . ")" . PHP_EOL, 3, $logFile);
		}

		$t = GetTournament($connection, $tournamentKey);

		if(!empty($t) && $t->RequirePayment){
			$removeSignup = GetSignup($connection, $removeSignupPlayer->SignUpKey);
			if(!empty($removeSignup)){
				$remainingFees = $removeSignup->PaymentDue - $t->Cost;
				if($remainingFees < 0){
					// Fee should never go below 0
					$remainingFees = 0;
				}

				UpdateSignup($connection, $removeSignupPlayer->SignUpKey, 'PaymentDue', $remainingFees, 'd');

				if(!empty($logFile)){
					error_log(date ( '[Y-m-d H:i e] ' ) . "Updated payment due to: " . $remainingFees . PHP_EOL, 3, $logFile);
				}
			}
		}

		$remainingPlayers = GetPlayersForSignUp($connection, $removeSignupPlayer->SignUpKey);
		if(count($remainingPlayers) == 0){
			DeleteSignup($connection, $removeSignupPlayer->SignUpKey);

			if(!empty($logFile)){
				error_log(date ( '[Y-m-d H:i e] ' ) . "Deleted signup: " . $removeSignupPlayer->SignUpKey . PHP_EOL, 3, $logFile);
			}
		}
	}
	else {
		if(!empty($logFile)){
			error_log(date ( '[Y-m-d H:i e] ' ) . "Failed to find player to remove: " . $name . " (" . $ghin . ")" . PHP_EOL, 3, $logFile);
		}
	}
}
?>