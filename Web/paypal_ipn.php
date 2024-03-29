<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/signup functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_functions.php';

// CONFIG: Enable debug mode. This means we'll log requests into 'ipn.log' in the same directory.
// Especially useful if you encounter network errors or other intermittent problems with IPN (validation).
// Set this to 0 once you go live or don't require logging.
define ( "DEBUG", 1 );
// Set to 0 once you're ready to go live
define ( "USE_SANDBOX", 0 );

if (!file_exists('./logs')) {
	mkdir('./logs', 0755, true);
}

define ( "LOG_FILE", "./logs/ipn.log" );

// Read POST data
// reading posted data directly from $_POST causes serialization
// issues with array data in POST. Reading raw POST data from input stream instead.
$raw_post_data = file_get_contents ( 'php://input' );
$raw_post_array = explode ( '&', $raw_post_data );
$myPost = array ();
foreach ( $raw_post_array as $keyval ) {
	$keyval = explode ( '=', $keyval );
	if (count ( $keyval ) == 2)
		$myPost [$keyval [0]] = urldecode ( $keyval [1] );
}
// read the post from PayPal system and add 'cmd'
$req = 'cmd=_notify-validate';
$get_magic_quotes_exists = false;
if (function_exists ( 'get_magic_quotes_gpc' )) {
	$get_magic_quotes_exists = true;
}
foreach ( $myPost as $key => $value ) {
	if ($get_magic_quotes_exists == true && get_magic_quotes_gpc () == 1) {
		$value = urlencode ( stripslashes ( $value ) );
	} else {
		$value = urlencode ( $value );
	}
	$req .= "&$key=$value";
}
// Post IPN data back to PayPal to validate the IPN data is genuine
// Without this step anyone can fake IPN data
if (USE_SANDBOX == true) {
	$paypal_url = "https://www.sandbox.paypal.com/cgi-bin/webscr";
} else {
	$paypal_url = "https://www.paypal.com/cgi-bin/webscr";
}
$ch = curl_init ( $paypal_url );
if ($ch == FALSE) {
	return FALSE;
}
curl_setopt ( $ch, CURLOPT_HTTP_VERSION, CURL_HTTP_VERSION_1_1 );
curl_setopt ( $ch, CURLOPT_POST, 1 );
curl_setopt ( $ch, CURLOPT_RETURNTRANSFER, 1 );
curl_setopt ( $ch, CURLOPT_POSTFIELDS, $req );
curl_setopt ( $ch, CURLOPT_SSL_VERIFYPEER, 1 );
curl_setopt ( $ch, CURLOPT_SSL_VERIFYHOST, 2 );
curl_setopt ( $ch, CURLOPT_FORBID_REUSE, 1 );
if (DEBUG == true) {
	curl_setopt ( $ch, CURLOPT_HEADER, 1 );
	curl_setopt ( $ch, CURLINFO_HEADER_OUT, 1 );
}
// CONFIG: Optional proxy configuration
// curl_setopt($ch, CURLOPT_PROXY, $proxy);
// curl_setopt($ch, CURLOPT_HTTPPROXYTUNNEL, 1);
// Set TCP timeout to 30 seconds
curl_setopt ( $ch, CURLOPT_CONNECTTIMEOUT, 30 );
curl_setopt ( $ch, CURLOPT_HTTPHEADER, array (
		'Connection: Close' 
) );
// CONFIG: Please download 'cacert.pem' from "http://curl.haxx.se/docs/caextract.html" and set the directory path
// of the certificate as shown below. Ensure the file is readable by the webserver.
// This is mandatory for some environments.
// $cert = __DIR__ . "./cacert.pem";
// curl_setopt($ch, CURLOPT_CAINFO, $cert);
$res = curl_exec ( $ch );
if (curl_errno ( $ch ) != 0) // cURL error
{
	if (DEBUG == true) {
		error_log ( date ( '[Y-m-d H:i e] ' ) . "Can't connect to PayPal to validate IPN message: " . curl_error ( $ch ) . PHP_EOL, 3, LOG_FILE );
	}
	curl_close ( $ch );
	exit ();
} else {
	// Log the entire HTTP response if debug is switched on.
	if (DEBUG == true) {
		error_log ( date ( '[Y-m-d H:i e] ' ) . "HTTP request of validation request:" . curl_getinfo ( $ch, CURLINFO_HEADER_OUT ) . " for IPN payload: $req" . PHP_EOL, 3, LOG_FILE );
		error_log ( date ( '[Y-m-d H:i e] ' ) . "HTTP response of validation request: $res" . PHP_EOL, 3, LOG_FILE );
	}
	curl_close ( $ch );
}
// Inspect IPN validation result and act accordingly
// Split response headers and payload, a better way for strcmp
$tokens = explode ( "\r\n\r\n", trim ( $res ) );
$res = trim ( end ( $tokens ) );
if (strcmp ( $res, "VERIFIED" ) == 0) {
	// check whether the payment_status is Completed
	// check that txn_id has not been previously processed
	// check that receiver_email is your PayPal email
	// check that payment_amount/payment_currency are correct
	// process payment and mark item as paid.
	// assign posted variables to local variables
	 $item_name = $_POST['item_name'];
	 //$item_number = $_POST['item_number'];
	 $payment_status = $_POST['payment_status'];
	 $payment_amount = $_POST['mc_gross'];
	// $payment_currency = $_POST['mc_currency'];
	// $txn_id = $_POST['txn_id'];
	 $receiver_email = $_POST['receiver_email'];
	 $payer_email = $_POST['payer_email'];
	 $custom = $_POST['custom'];
	 $payerName = $_POST['address_name'];
	 $payerEmail = $_POST['payer_email'];
	 
	 if(!empty($custom)){
	 	$customArray = explode(";", $custom);
	 	if((count($customArray) > 0) && !empty($customArray[0])){
	 		$tournamentKey = trim($customArray[0]);
	 	}
	 	if((count($customArray) > 1) && !empty($customArray[1])){
	 		$submitKey = trim($customArray[1]);
	 	}
	 	if((count($customArray) > 2) && !empty($customArray[2])){
	 		$players = trim($customArray[2]);
	 	}
	 }
	if (DEBUG == true) {
		error_log ( date ( '[Y-m-d H:i e] ' ) . "Verified IPN: $req " . PHP_EOL, 3, LOG_FILE );
	}
	
	if(!empty($tournamentKey)){
		$logMessage = "submitKey = " . $submitKey . ", payment = " . $payment_amount . ", players = " . $players;
		
		$connection = new mysqli ( $db_hostname, $db_username, $db_password, $db_database );
		UpdateDatabase($connection, $tournamentKey, $submitKey, $payment_amount, $payerName, $payerEmail, $logMessage);
		
		// email is always sent on signup now, so no need to send the email on payment
		//$tournament = GetTournament($connection, $tournamentKey);
		// Send email if enabled and the payment amount is positive (not a refund)
		//if(!empty($tournament) && $tournament->SendEmail && ($payment_amount > 0)){
		//	$tournamentDates = GetFriendlyNonHtmlTournamentDates($tournament);
		//	SendSignupEmail($connection, $tournament, $tournamentDates, $submitKey, $doNotReplyEmailAddress, $doNotReplyEmailPassword);
		//}
		
		$connection->close();
	}
	
} else if (strcmp ( $res, "INVALID" ) == 0) {
	// log for manual investigation
	// Add business logic here which deals with invalid IPN messages
	if (DEBUG == true) {
		error_log ( date ( '[Y-m-d H:i e] ' ) . "Invalid IPN: $req" . PHP_EOL, 3, LOG_FILE );
	}
}

function UpdateDatabase($connection, $tournamentKey, $submitKey, $payment, $payerName, $payerEmail, $logMessage){
	
	$logFile = "./logs/ipn." . $tournamentKey . ".log";
	error_log(date ( '[Y-m-d H:i e] ' ) . $logMessage . PHP_EOL, 3, $logFile);
	
	if ($connection->connect_error){
		error_log(date ( '[Y-m-d H:i e] ' ) . $connection->connect_error . PHP_EOL, 3, $logFile);
		return;
	}
	
	$signup = GetSignup($connection, $submitKey);
	if(empty($signup)){
		error_log(date ( '[Y-m-d H:i e] ' ) . "Failed to find submit key " . $submitKey . " in the signup table.  Were all the players removed from the group?" . PHP_EOL, 3, $logFile);
		return;
	}
	
	$updatedPayment = $signup->Payment + $payment;

	// check that PayPal has not re-sent the transaction by
	// capping the payment at the payment due.
	if($updatedPayment > $signup->PaymentDue){
		$updatedPayment = $signup->PaymentDue;
	}
	
	// Duplicate the UpdateSignup code here so the die messages can be replace with log messages
	//UpdateSignup($connection, $submitKey, 'Payment', $updatedPayment, 'd');
	$sqlCmd = "UPDATE `SignUps` SET `Payment`= ? WHERE `SubmitKey` = ?";
	$update = $connection->prepare ( $sqlCmd );
	
	if (! $update) {
		error_log(date ( '[Y-m-d H:i e] ' ) . $sqlCmd . " prepare failed: " . $connection->error . PHP_EOL, 3, $logFile);
		return;
	}
	
	if (! $update->bind_param ( 'di',  $updatedPayment, $submitKey)) {
		error_log(date ( '[Y-m-d H:i e] ' ) . $sqlCmd . " bind_param failed: " . $connection->error . PHP_EOL, 3, $logFile);
		return;
	}
	
	if (! $update->execute ()) {
		error_log(date ( '[Y-m-d H:i e] ' ) . $sqlCmd . " execute failed: " . $connection->error . PHP_EOL, 3, $logFile);
		return;
	}
	$update->close ();
	
	// Update payment date time
	$sqlCmd = "UPDATE `SignUps` SET `PaymentDateTime`= ? WHERE `SubmitKey` = ?";
	$update = $connection->prepare ( $sqlCmd );
	
	if (! $update) {
		error_log(date ( '[Y-m-d H:i e] ' ) . $sqlCmd . " prepare failed: " . $connection->error . PHP_EOL, 3, $logFile);
		return;
	}
	
	$date = date ( 'Y-m-d H:i:s' );
	if (! $update->bind_param ( 'si',  $date, $submitKey)) {
		error_log(date ( '[Y-m-d H:i e] ' ) . $sqlCmd . " bind_param failed: " . $connection->error . PHP_EOL, 3, $logFile);
		return;
	}
	
	if (! $update->execute ()) {
		error_log(date ( '[Y-m-d H:i e] ' ) . $sqlCmd . " execute failed: " . $connection->error . PHP_EOL, 3, $logFile);
		return;
	}
	$update->close ();
	
	// Update payer name
	$sqlCmd = "UPDATE `SignUps` SET `PayerName`= ? WHERE `SubmitKey` = ?";
	$update = $connection->prepare ( $sqlCmd );
	
	if (! $update) {
		error_log(date ( '[Y-m-d H:i e] ' ) . $sqlCmd . " prepare failed: " . $connection->error . PHP_EOL, 3, $logFile);
		return;
	}
	
	if (! $update->bind_param ( 'si',  $payerName, $submitKey)) {
		error_log(date ( '[Y-m-d H:i e] ' ) . $sqlCmd . " bind_param failed: " . $connection->error . PHP_EOL, 3, $logFile);
		return;
	}
	
	if (! $update->execute ()) {
		error_log(date ( '[Y-m-d H:i e] ' ) . $sqlCmd . " execute failed: " . $connection->error . PHP_EOL, 3, $logFile);
		return;
	}
	$update->close ();
	
	// Update payer email
	$sqlCmd = "UPDATE `SignUps` SET `PayerEmail`= ? WHERE `SubmitKey` = ?";
	$update = $connection->prepare ( $sqlCmd );
	
	if (! $update) {
		error_log(date ( '[Y-m-d H:i e] ' ) . $sqlCmd . " prepare failed: " . $connection->error . PHP_EOL, 3, $logFile);
		return;
	}
	
	if (! $update->bind_param ( 'si',  $payerEmail, $submitKey)) {
		error_log(date ( '[Y-m-d H:i e] ' ) . $sqlCmd . " bind_param failed: " . $connection->error . PHP_EOL, 3, $logFile);
		return;
	}
	
	if (! $update->execute ()) {
		error_log(date ( '[Y-m-d H:i e] ' ) . $sqlCmd . " execute failed: " . $connection->error . PHP_EOL, 3, $logFile);
		return;
	}
	$update->close ();
	
	error_log(date ( '[Y-m-d H:i e] ' ) . "Updated submit key " . $submitKey . " payment to " . $updatedPayment . PHP_EOL, 3, $logFile);
}
?>