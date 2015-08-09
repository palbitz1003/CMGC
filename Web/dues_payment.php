<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/signup functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/dues_functions.php';
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
$playerDues = null;

if (isset ( $_POST ['Player'] )) {
	
		$GHIN = trim ( $_POST ['Player'] ['GHIN'] );
		$LastName = trim ( $_POST ['Player'] ['LastName'] );
		
		$LastName = stripslashes ( $LastName ); // remove any slashes before quotes
		$LastName = str_replace("'", "", $LastName); // remove single quotes
	
		// Check that both GHIN and Last Name were filled in
		if(empty($GHIN) && empty($LastName)){
			$error = 'GHIN and Last Name must be filled in';
		} else if (! empty ( $GHIN ) && empty ( $LastName )) {
			$error = 'Last Name must be filled in';
		} else if (empty ( $GHIN ) && ! empty ( $LastName )) {
			$error = 'GHIN must be filled in';
		} else if (! empty ( $GHIN ) && ! empty ( $LastName )) {
			// TODO: Check for player already paid (may be in table but not yet paid)
			$playerDues = GetPlayerDues($connection, $GHIN);
			if (!empty($playerDues) && ($playerDues->Payment > 0)) {
				$error = 'Player ' . $LastName . ' (' . $GHIN . ') has already payed dues';
			} else {
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
			}
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
	echo '<h2 class="entry-title" style="text-align:center">Pay Yearly Dues</h2>' . PHP_EOL;
	
	echo '<p>The dues for regular members is $150 before Oct 1. From Oct 1 through Oct 31, the dues are $175. After Oct 31, you will be dropped from membership automatically.</p>' . PHP_EOL;
	echo '<p>Life members (80 and older) do not use this form. Click <a href="http://' . $web_site . '/' . $script_folder_href . 'dues_life_members.php">here</a> for instructions.</p>' . PHP_EOL;
	echo '<p>Regular members: Fill in your GHIN and last name below.</p>' . PHP_EOL;

	echo '<p>This is only step 1.  After entering your GHIN number and last name and passing the verification step, you will be shown a page with a PayPal button to pay the yearly dues.</p>' . PHP_EOL;
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
	echo '    name="Player[LastName]" value="' . $LastName . '"></td>' . PHP_EOL;
	echo '</tr>'  . PHP_EOL;
	//insert_error_line($error, 2);
	
	echo '</table>' . PHP_EOL;
	
	if(isset($error)){
		echo '<p style="color:red;">' . PHP_EOL;
		echo $error . PHP_EOL;
		echo '</p>' . PHP_EOL;
	}

	echo '<input type="submit" value="Step 1: Verify GHIN and Last Name"> <br> <br>' . PHP_EOL;
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
		die("Error: Last day for payments was " . $endExtendedDues->format('Y-m-d'));
	}
	if($testMode){
		$cost = 3;
	}
		
	$paypalDetails = GetPayPalDuesDetails($connection, $cost);
		
	if(!isset($paypalDetails->PayPayButton)){
		die("Error: No PayPal button for yearly dues $" . $cost);
	}

	if(empty($playerDues)){
		InsertPlayerForDues($connection, $year + 1, $GHIN, $FullName);
		
		// for testing
		//UpdateDatabase($connection, $GHIN, 150, "Player 1", "Player Email", "original string from paypal");
		//SendDuesEmail($connection, $GHIN, 150, $web_site);
	}
	
	echo '<div id="content-container" class="entry-content">' . PHP_EOL;
	echo '<div id="content" role="main">' . PHP_EOL;

	echo '<h2 class="entry-title" style="text-align:center">Pay Yearly Dues</h2>' . PHP_EOL;
	
	echo '<p>You must pay the dues now.  You have not payed your dues until the PayPal step is complete!</p>' . PHP_EOL;
	echo "<p>The link below takes you to PayPal to make your payment.  You can pay with credit card even if you do not have a PayPal account. No credit card or account information is kept on the Coronado Men's Golf web site.</p>" . PHP_EOL;
	echo '<p style="text-align: center;"><b>Dues: $' . number_format( $cost, 2) . '</b></p>' . PHP_EOL;
	
	echo '<form style="text-align:center" action="https://www.paypal.com/cgi-bin/webscr" method="post" target="_top">' . PHP_EOL;
	echo '<input type="hidden" name="cmd" value="_s-xclick">' . PHP_EOL;
	echo '<input type="hidden" name="hosted_button_id" value="' . $paypalDetails->PayPayButton . '">' . PHP_EOL;
	echo '<input type="hidden" name="item_name" value="Yearly Dues">' . PHP_EOL;
	echo '<input type="hidden" name="custom" value="Yearly Dues;' . $GHIN . ';' . $LastName . '">' . PHP_EOL;
	echo '<input type="hidden" name="on0" value="Yearly Dues">' . PHP_EOL;
	echo '<input type="hidden" name="currency_code" value="USD">' . PHP_EOL;
	echo '<input type="hidden" name="notify_url" value="http://' . $web_site . '/' . $ipn_dues_file . '">' . PHP_EOL;
	echo '<input type="hidden" name="return" value="http://' . $web_site . '/' . $script_folder_href . 'dues_payment_completed.php">' . PHP_EOL;
	echo '<input type="hidden" name="rm" value="1">' . PHP_EOL;
	echo '<input type="image" src="https://www.paypalobjects.com/en_US/i/btn/btn_paynowCC_LG.gif" border="0" name="submit" alt="PayPal - The safer, easier way to pay online!">' . PHP_EOL;
	echo '<img alt="" border="0" src="https://www.paypalobjects.com/en_US/i/scr/pixel.gif" width="1" height="1">' . PHP_EOL;
	echo '</form>' . PHP_EOL;
	
	echo '</div><!-- #content -->' . PHP_EOL;
	echo '</div><!-- #content-container -->' . PHP_EOL;
} // end of else clause

function InsertPlayerForDues($connection, $year, $ghin, $name) {
	$sqlCmd = "INSERT INTO `Dues` VALUES (?, ?, ?, ?, NULL, ?, ?, ?)";
	$insert = $connection->prepare ( $sqlCmd );

	if (! $insert) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	$payment = 0.0;
	$payerName = "";
	$payerEmail = "";
	$rigs = false;

	if (! $insert->bind_param ( 'iisdssi',  $year, $ghin, $name, $payment, $payerName, $payerEmail, $rigs )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}

	if (! $insert->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
	
	$insert->close();
}

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

function UpdateDatabase($connection, $ghin, $payment, $payerName, $payerEmail, $logMessage){

	if (!file_exists('./logs')) {
		mkdir('./logs', 0755, true);
	}
	
	$now = new DateTime ( "now" );
	$year = $now->format('Y') + 1;
	
	$logFile = "./logs/dues." . $year . ".log";
	error_log(date ( '[Y-m-d H:i e] ' ) . $logMessage . PHP_EOL, 3, $logFile);

	if ($connection->connect_error){
		error_log(date ( '[Y-m-d H:i e] ' ) . $connection->connect_error . PHP_EOL, 3, $logFile);
		return;
	}

	$player = GetPlayerDues($connection, $ghin);
	if(empty($player)){
		error_log(date ( '[Y-m-d H:i e] ' ) . "Failed to find ghin " . $ghin . " in the dues table." . PHP_EOL, 3, $logFile);
		return;
	}

	// Duplicate the UpdateSignup code here so the die messages can be replace with log messages
	$sqlCmd = "UPDATE `Dues` SET `Payment`= ?, `PaymentDateTime`= ?, `PayerName`= ?, `PayerEmail`= ?, `RIGS` = 0 WHERE `GHIN` = ?";
	$update = $connection->prepare ( $sqlCmd );

	if (! $update) {
		error_log(date ( '[Y-m-d H:i e] ' ) . $sqlCmd . " prepare failed: " . $connection->error . PHP_EOL, 3, $logFile);
		return;
	}

	$date = date ( 'Y-m-d H:i:s' );
	if (! $update->bind_param ( 'dsssi',  $payment, $date, $payerName, $payerEmail, $ghin)) {
		error_log(date ( '[Y-m-d H:i e] ' ) . $sqlCmd . " bind_param failed: " . $connection->error . PHP_EOL, 3, $logFile);
		return;
	}

	if (! $update->execute ()) {
		error_log(date ( '[Y-m-d H:i e] ' ) . $sqlCmd . " execute failed: " . $connection->error . PHP_EOL, 3, $logFile);
		return;
	}
	$update->close ();

	error_log(date ( '[Y-m-d H:i e] ' ) . "Updated player " . $player->Name . " payment to " . $payment . PHP_EOL, 3, $logFile);
}

function SendDuesEmail($connection, $ghin, $payment, $web_site){

	$rosterEntry = GetRosterEntry ( $connection, $ghin );
	if(empty($rosterEntry)){
		return "Did not find a player for ghin: " . $ghin;
	}
	
	$now = new DateTime ( "now" );
	$year = $now->format('Y') + 1;

	// compose message
	$message = "You have paid your dues ($" . $payment . ") for the Coronado Men's Golf Club for " . $year;
	
	if(!empty($rosterEntry) && !empty($rosterEntry->Email)){
		mail($rosterEntry->Email, "Coronado Men's Golf Club yearly dues", $message, "From: DoNotReply@" . $web_site);
	}
	
	return null;
}

if (isset ( $connection )) {
	$connection->close ();
}

get_footer ();
?>