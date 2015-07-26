<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/signup functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_descriptions_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$testMode = false;
if($_GET ['mode'] == "test"){
	$testMode = true;
}

$connection = new mysqli ( $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( $connection->connect_error );

$error = "";
$GHIN = "";
$LastName = "";
$FullName = "";

if (isset ( $_POST ['Player'] )) {
	
		$GHIN = trim ( $_POST ['Player'] ['GHIN'] );
		$LastName = trim ( $_POST ['Player'] ['LastName'] );
		
		$LastName = stripslashes ( $LastName ); // remove any slashes before quotes
		$LastName = str_replace("'", "", $LastName); // remove single quotes
	
		// Check that both GHIN and Last Name were filled in
		if (! empty ( $GHIN ) && empty ( $LastName )) {
			$error = 'Last Name must be filled in';
		} else if (empty ( $GHIN ) && ! empty ( $LastName )) {
			$error = 'GHIN must be filled in';
		} else if (! empty ( $GHIN ) && ! empty ( $LastName )) {
			// TODO: Check for player already paid (may be in table but not yet paid)
			//if (IsPlayerSignedUp ( $connection, $tournamentKey, $GHIN [$i] )) {
			//	$errorList [$i] = 'Player ' . $GHIN [$i] . ' is already signed up';
			//} else {
				// Check that last name matches GHIN database
				$rosterEntry = GetRosterEntry ( $connection, $GHIN );
				if (empty ( $rosterEntry )) {
					$error = 'GHIN ' . $GHIN . " is not a member of the Coronado Men's Golf Club";
				} else if (strcasecmp ( $LastName, $rosterEntry->LastName ) != 0) {
					$error = 'Last name for GHIN ' . $GHIN . ' is not ' . $LastName;
				} else if(!$rosterEntry->Active) {
					$error = 'GHIN ' . $GHIN . " is not an active member of the Coronado Men's Golf Club";
				} else {
					// Use the database casing for the last name
					$LastName = $rosterEntry->LastName;
					$FullName = $rosterEntry->LastName . ', ' . $rosterEntry->FirstName;
				}
			//}
	}
}

$overrideTitle = "Pay Dues";
get_header ();

get_sidebar ();

//var_dump($_POST);

// If this page has not been filled in or there is an error, show the form
if (!empty($error) || !isset ( $_POST ['Player'] )) {

	echo '<div id="content-container" class="entry-content">' . PHP_EOL;
	echo '<div id="content" role="main">' . PHP_EOL;
	echo '<h2 class="entry-title">Pay Yearly Dues</h2>' . PHP_EOL;
	echo '<p>' . PHP_EOL;
	echo 'Fill in your GHIN number and last name.' . PHP_EOL;
	echo '</p>' . PHP_EOL;

	echo '<p>This is only step 1.  After entering your GHIN number and last name, you will be asked to pay the yearly dues.</p>' . PHP_EOL;
	echo '<form name="input" method="post">' . PHP_EOL;
	
	echo '<table style="border: none;">' . PHP_EOL;
	
	echo '<tr>' . PHP_EOL;
	echo '<td style="border: none;">GHIN:</td>' . PHP_EOL;
	echo '<td style="border: none;"><input type="text" name="Player[GHIN]"';
	echo '    value="' . $GHIN . '"></td>' . PHP_EOL;
	echo '</tr>' . PHP_EOL;
	
	echo '<tr>' . PHP_EOL;
	echo '<td style="border: none;">Last Name:</td>' . PHP_EOL;
	echo '<td style="border: none;"><input type="text"';
	echo '    name="Player[LastName]" value="' . $lastName . '"></td>' . PHP_EOL;
	echo '</tr>'  . PHP_EOL;
	insert_error_line($error, 1);
	
	echo '</table>' . PHP_EOL;

	echo '<input type="submit" value="Pay Yearly Dues"> <br> <br>' . PHP_EOL;
	echo '<a href="dues_paid.php">View list of players that have paid</a>' . PHP_EOL;
	echo '</form>' . PHP_EOL;
	echo '</div><!-- #content -->' . PHP_EOL;
	echo '</div><!-- #content-container -->' . PHP_EOL;

} else {
	$now = new DateTime ( "now" );
	$year = $now->format('Y');
	
	$endBasicDues = new DateTime($year . '-10-01');
	$endExtendedDues = new DateTime($year . '-11-01');
	
	$cost = 150;
	if(($now > $endBasicDues) && ($now < $endExtendedDues))
	{
		$cost = 175;
	}
	if($now > $endExtendedDues)
	{
		die("Did not expect to be making payments after " . $endExtendedDues->format('Y-m-d'));
	}
	if($testMode){
		$cost = 3;
	}
		
	$paypalDetails = GetPayPalDuesDetails($connection, $cost);
		
	if(!isset($paypalDetails->PayPayButton)){
		die("No PayPal button for yearly dues $" . $cost);
	}

	// TODO:
	// The signup has no errors. Proceed to sign up the group. First create the signup entry.
	$insertId = InsertSignUp ( $connection, $tournamentKey, $RequestedTime, $entryFees, $accessCode);
	// echo 'insert id is: ' . $insertId . '<br>';
	
	// Now add the players to the signup entry
	InsertSignUpPlayers ( $connection, $tournamentKey, $insertId, $GHIN, $FullName, $Extra );
	
	echo '<div id="content-container" class="entry-content">' . PHP_EOL;
	echo '<div id="content" role="main">' . PHP_EOL;

	echo '<h2 class="entry-title" style="text-align:center">' . $tournament->Name . ' Signup Complete</h2>' . PHP_EOL;
	echo '<p><a href="' . 'signups.php?tournament=' . $tournamentKey . '">View Signups</a></p>' . PHP_EOL;
	
	echo '</div><!-- #content -->' . PHP_EOL;
	echo '</div><!-- #content-container -->' . PHP_EOL;
} // end of else clause

function GetPayPalDuesDetails($connection, $dues){
	$sqlCmd = "SELECT * FROM `PayPalDues` WHERE `Dues` = ?";
	$payPal = $connection->prepare ( $sqlCmd );

	if (! $payPal) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}

	if (! $payPal->bind_param ( 'i', $dues )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}

	if (! $payPal->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}

	$payPal->bind_result ( $payPalButton, $fee );

	$details = new PayPalDetails();
	if($payPal->fetch ()){
		$details->PayPayButton = $payPalButton;
		$details->TournamentFee = $fee;
	}

	$payPal->close ();

	return $details;
}

if (isset ( $connection )) {
	$connection->close ();
}

get_footer ();
?>