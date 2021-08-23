<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/signup functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_descriptions_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$tournamentKey = $_GET ['tournament'];
if (! $tournamentKey || !is_numeric($tournamentKey)) {
	get_header ();
	
	get_sidebar ();
	die ( "Which tournament?" );
}

if (!file_exists($default_log_folder)) {
	mkdir($default_log_folder, 0755, true);
}
$logFile = $default_log_folder . "/signup." . $tournamentKey . ".log";

$testMode = false;
if($_GET ['mode'] == "test"){
	$testMode = true;
}

$connection = new mysqli ( $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( $connection->connect_error );

$t = GetTournament($connection, $tournamentKey);

if(empty($t)){
	get_header ();
	
	get_sidebar ();
	die("Tournament " . $tournamentKey . " does not exist.");
}

$now = new DateTime ( "now" );
$startSignUp = new DateTime ( $t->SignupStartDate);
$startSignUp->add(new DateInterval ( $signup_start_time ));

// Only check for someone trying to sign up before the signup period starts.
// If they want to sign up after the signup period ends, that is fine.
if($now < $startSignUp){
	$overrideTitle = "Sign Up";
	get_header ();

	get_sidebar ();

	echo "<p>Sign up doesn't start until " . GetUnbreakableHtmlDateString(date ( 'M d (ga)', date_timestamp_get($startSignUp) )) . ".</p>";
	if (isset ( $connection )) {
		$connection->close ();
	}
	
	get_footer ();
	return;
}

/*
$count = GetPlayerCountForTournament($connection, $tournamentKey);
if(($t->MaxSignups != 0) && ($count >= $t->MaxSignups))
{
	$overrideTitle = "Sign Up";
	get_header ();

	get_sidebar ();

	echo "<p>This tournament is full.</p>";
	if (isset ( $connection )) {
		$connection->close ();
	}
	
	get_footer ();
	return;
}
*/

$descr = '';
if($t->TournamentDescriptionKey > 0){
	$td = GetTournamentDescription($connection, $t->TournamentDescriptionKey);
	$descr = $td->Description;
}

$errorList = array ();
$flightErrorList = array();
$GHIN = array ();
$LastName = array ();
$FullName = array();
$Extra = array();
$RequestedTime = "None";
$memberSignedUp = array_fill(0, 3, false);

if (isset ( $_POST ['Player'] )) {
	
	for($i = 0; $i < 4; ++ $i) {
		$GHIN [$i] = trim ( $_POST ['Player'] [$i] ['GHIN'] );
		$LastName [$i] = trim ( $_POST ['Player'] [$i] ['LastName'] );
		$Extra [$i] = trim ( $_POST ['Player'] [$i] ['Extra'] );
		
		$LastName[$i] = stripslashes ( $LastName[$i] ); // remove any slashes before quotes
		$LastName[$i] = str_replace("'", "", $LastName[$i]); // remove single quotes
		$LastName[$i] = str_replace('"', "", $LastName[$i]); // remove double quotes
		$RequestedTime = $_POST ['RequestedTime'];
		
		if(($t->TeamSize == 2) && ($t->SCGAQualifier))
		{
			if(($i == 0) || ($i == 1)){
				$teamFlightIndex = GetTeamFlightIndex(1);
				$Extra[$i] = $_POST[$teamFlightIndex];
			}
			else {
				$teamFlightIndex = GetTeamFlightIndex(2);
				$Extra[$i] = $_POST[$teamFlightIndex];
			}
			if(! empty ( $GHIN [$i] ) && empty ($_POST[$teamFlightIndex]))
			{
				$flightErrorList[$i] = "Select Flight";
			}
		}
		else if($t->SrClubChampionship || $t->ClubChampionship)
		{
			$playerFlightIndex = GetPlayerFlightIndex($i + 1);
			$Extra[$i] = $_POST[$playerFlightIndex];
			if(! empty ( $GHIN [$i] ) && empty ($_POST[$playerFlightIndex]))
			{
				$flightErrorList[$i] = "Select Flight";
			}
		} 
		
		// If you put in a GHIN of 0, empty("0") returns true, so change
		// the GHIN number to "0000000"
		if (isset($GHIN [$i]) && $GHIN [$i] === '0'){
			$GHIN [$i] = "0000000";
		}
		
		$guest = ($i % 2) == 1;
		
		if($t->MemberGuest && $guest){
			$guestIsMember = false;
			if(!$memberSignedUp[$i - 1]){
				// Only check guest if member is signed up
			} else if(empty ( $LastName [$i]) && empty ( $GHIN [$i] )){
				// Must sign up with a guest
				$errorList [$i] = 'No guest signed up with ' . $LastName[$i - 1];
			} else {
				if(!empty ( $GHIN [$i] ) && ($GHIN [$i] !== "0000000")){
					$rosterEntry = GetRosterEntry ( $connection, $GHIN [$i] );
					if (!empty ( $rosterEntry )) {
						if($rosterEntry->Active){
							$guestIsMember = true;
							// Comment out check below and allow member-member signup
							//$errorList [$i] = 'GHIN ' . $GHIN [$i] . " is a member of the Coronado Men's Golf Club<br>The guest cannot be a member.";

							// If they only filled in the last name for the member in the guest field,
							// change it to be the roster name. Otherwise, the check below for "last name, first name" will fail.
							// TODO: check here for last name matching GHIN or that player is already signed up.
							if (strpos($LastName [$i], ',') === FALSE){
								$LastName [$i] =  $rosterEntry->LastName . ', ' . $rosterEntry->FirstName;
							}
						}
					}
					// Since the GHIN is non-zero, check to see if they are already signed up.
					if (IsPlayerSignedUp ( $connection, $tournamentKey, $GHIN [$i] )) {
						$errorList [$i] = 'Player ' . $GHIN [$i] . ' is already signed up';
					}
				}
					
				if(empty($errorList [$i])){
					if(empty ( $GHIN [$i] ))
					{
						$errorList [$i] = 'Fill in GHIN for guest';
					} else if (strpos($LastName [$i], ',') === FALSE){
						$errorList [$i] = 'Please fill in "last name, first name" for guests';
					} else {
						// Save the the full name.
						$FullName[$i] = FixNameCasing($LastName [$i]);
						if($guestIsMember){
							$Extra[$i] = "M"; // "Member" flight
						} else {
							$Extra[$i] = "G"; // "Guest" flight
						}
					} 
				}
			}
		} else if ($t->AllowNonMemberSignup && ! empty ( $GHIN [$i] ) && !empty ( $LastName [$i]) && ($GHIN [$i] === "0000000")) {
			// Check that both GHIN and Last Name were filled in
			if (strpos($LastName [$i], ',') !== FALSE){
				// No checks for name matching GHIN or if player is already signed up
				// Just save the the full name.
				$FullName[$i] = $LastName [$i];
			} else {
				$errorList [$i] = 'Please fill in "last name, first name" when using GHIN 0';
			}
		} else if (! empty ( $GHIN [$i] ) && empty ( $LastName [$i] )) {
			$errorList [$i] = 'Player ' . ($i + 1) . ' Last Name must be filled in';
		} else if (empty ( $GHIN [$i] ) && ! empty ( $LastName [$i] )) {
			$errorList [$i] = 'Player ' . ($i + 1) . ' GHIN must be filled in';
		} else if (! empty ( $GHIN [$i] ) && ! empty ( $LastName [$i] )) {
			// Check for player already signed up
			if (IsPlayerSignedUp ( $connection, $tournamentKey, $GHIN [$i] )) {
				$errorList [$i] = 'Player ' . $GHIN [$i] . ' is already signed up';
			} else {
				// Check that last name matches GHIN database
				$rosterEntry = GetRosterEntry ( $connection, $GHIN [$i] );
				
				// $errorList[$i] = 'Last name is ' . $lastName;
				if (empty ( $rosterEntry )) {
					$errorList [$i] = 'GHIN ' . $GHIN [$i] . " is not a member of the Coronado Men's Golf Club";
				} else if(!$rosterEntry->Active) {
					$errorList [$i] = 'GHIN ' . $GHIN [$i] . " is not an active member of the Coronado Men's Golf Club";
				} else {
					if (strpos($rosterEntry->LastName, ' ') !== FALSE){
						// Only compare the part before the space
						$nameArray1 = explode(' ', $rosterEntry->LastName);
						$nameArray2 = explode(' ', $LastName [$i]);
						$lastNamesMatch = strcasecmp ( $nameArray1[0], $nameArray2[0] ) == 0;
					} else {
						$lastNamesMatch = strcasecmp ( $LastName [$i], $rosterEntry->LastName ) == 0;
					}
					
					if (!$lastNamesMatch) {
						$errorList [$i] = 'Last name for GHIN ' . $GHIN [$i] . ' is not ' . $LastName [$i];
					} else {
						// Use the database casing for the last name
						$LastName [$i] = $rosterEntry->LastName;
						$FullName[$i] = $rosterEntry->LastName . ', ' . $rosterEntry->FirstName;
						if($t->MemberGuest){
							$memberSignedUp[$i] = true;
							$Extra[$i] = "M"; // "Member" flight
						}
						else if($t->SrClubChampionship && ($Extra[$i] == "AGE")){
							$birthday = new DateTime ( $rosterEntry->BirthDate);
							$tournamentStart = new DateTime($t->StartDate);
							$interval = $tournamentStart->diff($birthday);
							if($interval){
								//echo "years = " . $interval->y . "<br>";
								if($interval->y < 60){
									$Extra[$i] = "F1";
								} else if($interval->y < 70){
									$Extra[$i] = "F2";
								} else {
									$Extra[$i] = "F3";
								}
							} else {
								echo "failed to determine age<br>";
							}
						}
					}
				}
			}
		}
	}
	
	// The first player must be filled in
	if (empty ( $GHIN [0] ) && empty ( $LastName [0] )) {
		$errorList [0] = 'Player 1 GHIN and Last Name must be filled in';
	}
	else if($t->TeamSize == 2)
	{
		if(empty($GHIN [1]) && (!empty($GHIN [2]) || !empty($GHIN [3])))
		{
			$errorList [1] = "Team 1 must be full before starting team 2";
		}
	}
}

$hasError = false;
for($i = 0; $i < 4; ++ $i) {
	if (! empty ( $errorList [$i] ) || ! empty ($flightErrorList [$i])) {
		$hasError = true;
	}
}

$overrideTitle = "Sign Up";
get_header ();

get_sidebar ();

//var_dump($_POST);

// If this page has not been filled in or there is an error, show the form
if ($hasError || !isset ( $_POST ['Player'] )) {

	echo '<div id="content-container" class="entry-content">' . PHP_EOL;
	echo '<div id="content" role="main">' . PHP_EOL;
	echo '<h2 class="entry-title">Sign Up</h2>' . PHP_EOL;
	echo '<h3>' . $t->Name . '</h3>' . PHP_EOL;

	if(!$hasError) {
		echo '<h4>Tournament Description</h4>' . PHP_EOL;
		echo $descr; 
		DisplayTournamentDetails($t); 
	}
	echo '<p>' . PHP_EOL;
	if($t->MemberGuest){
		echo 'Fill in the GHIN and last name for members and GHIN and both the last name and first name for guests.' . PHP_EOL;
	} else {
		echo 'Fill in the GHIN and last name for 1-4 players.  Player 1 must be filled in.' . PHP_EOL;
	}
	
	echo '</p>' . PHP_EOL;
	if($t->RequirePayment) { 
		echo '<p>This is only step 1.  After entering the list of players, you will be asked to pay the tournament fee.</p>' . PHP_EOL;
	}
	echo '<form name="input" method="post">' . PHP_EOL;
	
	if($t->TeamSize == 2)
	{
		echo '<table>' . PHP_EOL;
		echo '	<colgroup>' . PHP_EOL;
		echo '		<col style="width: 100px">' . PHP_EOL;
		echo '		<col>' . PHP_EOL;
		echo '	</colgroup>' . PHP_EOL;
		echo '<tr>' . PHP_EOL;
		TeamNumber($t, 1, $flightErrorList, $Extra);
		
		echo '<td>' . PHP_EOL;
		AddPlayerTable($t);  // adds unfinished <table> which is closed below
		
		AddPlayer($t, 1, $GHIN[0], $LastName[0], $Extra[0], $flightErrorList[0]);
		insert_error_line($errorList[0], 2);
		
		AddPlayer($t, 2, $GHIN[1], $LastName[1], $Extra[1], $flightErrorList[1]);
		insert_error_line($errorList[1], 2);
		
		echo '</table>' . PHP_EOL;
		echo '</td>' . PHP_EOL;
		echo '</tr>' . PHP_EOL;
		
		echo '<tr>' . PHP_EOL;
		TeamNumber($t, 2, $flightErrorList, $Extra);
		
		echo '<td>' . PHP_EOL;
		AddPlayerTable($t);  // adds unfinished <table> which is closed below
		
		AddPlayer($t, 3, $GHIN[2], $LastName[2], $Extra[2], $flightErrorList[2]);
		insert_error_line($errorList[2], 2);
		
		AddPlayer($t, 4, $GHIN[3], $LastName[3], $Extra[3], $flightErrorList[3]);
		insert_error_line($errorList[3], 2);
		
		echo '</table>' . PHP_EOL;
		echo '</td>' . PHP_EOL;
		echo '</tr>' . PHP_EOL;
		echo '<tr>' . PHP_EOL;
		echo '<td></td>' . PHP_EOL;  // empty team number
		
		echo '<td>' . PHP_EOL;
		AddPlayerTable($t);  // adds unfinished <table> which is closed below
		
		RequestedTime($RequestedTime);
		
		echo '</table>' . PHP_EOL;
		echo '</td>' . PHP_EOL;
		echo '</tr>' . PHP_EOL;
		echo '</table>' . PHP_EOL;
		
	}
	else 
	{
		AddPlayerTable($t);  // adds unfinished <table> which is closed below
		
		AddPlayer($t, 1, $GHIN[0], $LastName[0], $Extra[0], $flightErrorList[0]);
		insert_error_line($errorList[0], 2);
		
		AddPlayer($t, 2, $GHIN[1], $LastName[1], $Extra[1], $flightErrorList[1]);
		insert_error_line($errorList[1], 2);
		
		AddPlayer($t, 3, $GHIN[2], $LastName[2], $Extra[2], $flightErrorList[2]);
		insert_error_line($errorList[2], 2);
		
		AddPlayer($t, 4, $GHIN[3], $LastName[3], $Extra[3], $flightErrorList[3]);
		insert_error_line($errorList[3], 2);
	
		RequestedTime($RequestedTime);
		echo '</table>' . PHP_EOL;
	}

	echo '<input type="hidden" name="tournament" value="' . $tournamentKey . '">' . PHP_EOL;
	echo '<input type="submit" value="Sign Up"> <br> <br>' . PHP_EOL;
	echo '<a href="signups.php?tournament=' . $tournamentKey . '">Current Signups</a>' . PHP_EOL;
	echo '</form>' . PHP_EOL;
	echo '</div><!-- #content -->' . PHP_EOL;
	echo '</div><!-- #content-container -->' . PHP_EOL;

} else {

	$playerCount = 0;
	$playersNameOnly = "";
	for($i = 0; $i < count ( $GHIN ); ++ $i) {
		if (! empty ( $GHIN [$i] )) {
			++$playerCount;

			// Save names for log message
			if(!empty($playersNameOnly)){
				$playersNameOnly = $playersNameOnly . " --- ";
			}
			$playersNameOnly = $playersNameOnly . $FullName[$i];
		}
	}
	
	if($playerCount == 0){
		die("No players specified");
	}
	
	$entryFees = 0;
	$accessCode = rand(1000, 9999);
	
	if($t->RequirePayment){
		$cost = $t->Cost;
		if($testMode){
			$cost = 3;
		}
		
		$paypalDetails = GetPayPalDetails($connection, $cost);
		
		if(!isset($paypalDetails->PayPayButton)){
			die("No PayPal button for tournament fee " . $cost);
		}
		
		$entryFees = $playerCount * ($paypalDetails->TournamentFee + $paypalDetails->ProcessingFee);
	}

	// Need the previous 2 day tournament key to get the waitlist
	$previousTournamentKey = GetPrevious2DayTournamentKey($connection, $tournamentKey);
	
	// Enable payment for some groups immediately
	$paymentEnabled = DecideIfPaymentEnabled($connection, $previousTournamentKey, $GHIN, $logFile);

	// The signup has no errors. Proceed to sign up the group. First create the signup entry.
	$insertId = InsertSignUp ( $connection, $tournamentKey, $RequestedTime, $entryFees, $accessCode, $paymentEnabled);
	// echo 'insert id is: ' . $insertId . '<br>';

	// Now add the players to the signup entry
	InsertSignUpPlayers ( $connection, $tournamentKey, $insertId, $GHIN, $FullName, $Extra );

	if(!empty($logFile)){
		error_log(date ( '[Y-m-d H:i e] ' ) . "Players signed up: ". $playersNameOnly . ". Payment enabled: " . ($paymentEnabled ? "true" : "false") . PHP_EOL, 3, $logFile);
	}

	if($t->SendEmail ){
		$tournamentDates = GetFriendlyNonHtmlTournamentDates($t);
		$errorMsg = SendSignupEmail($connection, $t, $tournamentDates, $insertId, $doNotReplyEmailAddress, $doNotReplyEmailPassword);
		if(!empty($errorMsg)){
			echo '<p>' . $errorMsg . '</p>' . PHP_EOL;
		}
	}

	if($t->RequirePayment && $paymentEnabled)
	{
		echo '<div id="content-container" class="entry-content">' . PHP_EOL;
		echo '<div id="content" role="main">' . PHP_EOL;
		
		ShowPayment($web_site, $ipn_file, $script_folder_href, $connection, $t, $insertId, $accessCode, $testMode);

		echo '</div><!-- #content -->' . PHP_EOL;
		echo '</div><!-- #content-container -->' . PHP_EOL;

	} // end of require payment 
	else {
		echo '<div id="content-container" class="entry-content">' . PHP_EOL;
		echo '<div id="content" role="main">' . PHP_EOL;

		echo '<h2 class="entry-title" style="text-align:center">' . $tournament->Name . ' Signup Complete</h2>' . PHP_EOL;
		if(!empty($accessCode)){
			echo '<p>Your signup data has been saved.  Here is your access code to make changes to your signup. Save this code for later!</p>';
			echo '<p style="text-align: center;"><b>' . $accessCode . '</b> </p>' . PHP_EOL;
		}
		if($t->RequirePayment && !$paymentEnabled){
			echo '<p>You will pay later if you are selected to play in this tournament.</p>';
		}
		echo '<p><a href="' . 'signups.php?tournament=' . $tournamentKey . '">View Signups</a></p>' . PHP_EOL;
		
		echo '</div><!-- #content -->' . PHP_EOL;
		echo '</div><!-- #content-container -->' . PHP_EOL;
	}
} // end of else clause


function AddPlayer($t, $playerNumber, $GHIN, $lastName, $extraForPlayer, $errorForPlayer)
{
	$index = $playerNumber - 1;
	echo '<tr>' . PHP_EOL;
	echo '<td style="border: none;">' . GetPlayerGHINLabel($t, $playerNumber) . '</td>' . PHP_EOL;
	echo '<td style="border: none;"><input type="text" name="Player[' . $index . '][GHIN]"';
	echo '    value="' . $GHIN . '"></td>' . PHP_EOL;
	AddFlights($t, $playerNumber, $extraForPlayer, $errorForPlayer, 2);
	
	echo '</tr>' . PHP_EOL;
	echo '<tr>' . PHP_EOL;
	echo '<td style="border: none;">' . GetPlayerNameLabel($t, $playerNumber) . '</td>' . PHP_EOL;
	echo '<td style="border: none;"><input type="text"';
	echo '    name="Player[' . $index . '][LastName]" value="' . $lastName . '"></td>' . PHP_EOL;
	
	echo '</tr>';
}

function GetPlayerGHINLabel($t, $playerNumber)
{
	if($t->MemberGuest)
	{
		if($playerNumber % 2 != 0)
		{
			return 'Member GHIN:';
		}
		else 
		{
			return 'Guest GHIN:';
		}
	}
	else 
	{
		return 'Player ' . $playerNumber . ' GHIN:';
	}
}

function GetPlayerNameLabel($t, $playerNumber)
{
	if($t->MemberGuest)
	{
		if($playerNumber % 2 != 0)
		{
			return 'Member Last Name:';
		}
		else
		{
			return 'Guest Name:<br>(Last Name, First Name)';
		}
	}
	else
	{
		return 'Player ' . $playerNumber . ' Last Name:';
	}
}



if (isset ( $connection )) {
	$connection->close ();
}

get_footer ();
?>