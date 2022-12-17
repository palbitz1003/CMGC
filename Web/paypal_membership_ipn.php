<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/dues_functions.php';

// CONFIG: Enable debug mode. This means we'll log requests into 'ipn.log' in the same directory.
// Especially useful if you encounter network errors or other intermittent problems with IPN (validation).
// Set this to 0 once you go live or don't require logging.
define ( "DEBUG", 0 );
// Set to 0 once you're ready to go live
define ( "USE_SANDBOX", 1 );

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
	//if (DEBUG == true) {
		error_log ( date ( '[Y-m-d H:i e] ' ) . "Can't connect to PayPal to validate IPN message: " . curl_error ( $ch ) . PHP_EOL, 3, LOG_FILE );
		error_log ( date ( '[Y-m-d H:i e] ' ) . "Request was: $req " . PHP_EOL, 3, LOG_FILE );
	//}
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
	 
     $membershipAction = '';
     $recordKey = 0;
     $name = '';
     $dateAdded = '';
	 $ghin = '';

	 if(!empty($custom)){
	 	// format is:
		// FinalPayment;RecordKey;Name;DateAdded or
		// InitialPayment;RecordKey;Name;GHIN
	 	$customArray = explode(";", $custom);
	 	if((count($customArray) > 0) && !empty($customArray[0])){
	 		$membershipAction = trim($customArray[0]);
	 	}
	 	if((count($customArray) > 1) && !empty($customArray[1])){
	 		$recordKey = trim($customArray[1]);
	 	}
	 	if((count($customArray) > 2) && !empty($customArray[2])){
	 		$name = trim($customArray[2]);
	 	}
         if((count($customArray) > 3) && !empty($customArray[3])){
			if($membershipAction === "FinalPayment"){
            	$dateAdded = trim($customArray[3]);
			}
			else {
				$ghin = trim($customArray[3]);
			}
        }
	 }
	if (DEBUG == true) {
		error_log ( date ( '[Y-m-d H:i e] ' ) . "Verified IPN: $req " . PHP_EOL, 3, LOG_FILE );
	}
	
	if($membershipAction === "FinalPayment"){
		if($payment_amount < 0){
			$label = "Final payment refund: ";
		}
		else {
			$label = "Final payment: ";
		}
		$logMessage = $label . "Waiting list RecordKey = " . $recordKey . ", waiting list name = " . $name . ", waiting list date added = " . $dateAdded . ", payment = " . $payment_amount;
		
		$connection = new mysqli ( $db_hostname, $db_username, $db_password, $db_database );

        cmgc_paid_final_membership($connection, $payment_amount, $payerName, $recordKey, $logMessage);
		
		$connection->close();
	}
	else if($membershipAction === "InitialPayment"){
		if($payment_amount < 0){
			$label = "Application initial payment refund: ";
		}
		else {
			$label = "Application initial payment: ";
		}
		$logMessage = $label . "Application RecordKey = " . $recordKey . ", application name = " . $name . ", application GHIN = " . $ghin . ", payment = " . $payment_amount;
		
		$connection = new mysqli ( $db_hostname, $db_username, $db_password, $db_database );

        cmgc_paid_application_fee($connection, $payment_amount, $payerName, $recordKey, $logMessage);
		
		$connection->close();
	}
	else {
		error_log ( date ( '[Y-m-d H:i e] ' ) . "Unexpected custom field (first field was not FinalPayment): " . $custom . PHP_EOL, 3, LOG_FILE );
	}
	
} else if (strcmp ( $res, "INVALID" ) == 0) {
	// log for manual investigation
	// Add business logic here which deals with invalid IPN messages
	//if (DEBUG == true) {
		error_log ( date ( '[Y-m-d H:i e] ' ) . "Invalid IPN: $req" . PHP_EOL, 3, LOG_FILE );
	//}
}

function cmgc_waitlist_get_current_payment($connection, $recordKey, $logFile){

	$sqlCmd = "SELECT Payment FROM `WaitingList` WHERE `RecordKey` = ?";
	$query = $connection->prepare ( $sqlCmd );

	if (! $query->bind_param ( 'i', $recordKey )) {
		error_log(date ( '[Y-m-d H:i e] ' ) . $sqlCmd . " bind_param failed: " . $connection->error  . PHP_EOL, 3, $logFile);
		return 0;
	}

	if (! $query) {
		error_log(date ( '[Y-m-d H:i e] ' ) . $sqlCmd . " prepare failed: " . $connection->error  . PHP_EOL, 3, $logFile);
		return 0;
	}

	if (! $query->execute ()) {
		error_log(date ( '[Y-m-d H:i e] ' ) . $sqlCmd . " execute failed: " . $connection->error  . PHP_EOL, 3, $logFile);
		return 0;
	}

	$payment = 0;

	$query->bind_result ($payment);

	$query->fetch ();

	//error_log(date ( '[Y-m-d H:i e] ' ) . "previous payment value was " . $payment  . PHP_EOL, 3, $logFile);

	$query->close();

	return $payment;
}

function cmgc_paid_final_membership($connection, $paymentAmount, $payerName, $recordKey, $logMessage){

    if (!file_exists('./logs')) {
		mkdir('./logs', 0755, true);
	}

	$now = new DateTime ( "now" );
	$year = $now->format('Y') + 1;

	$logFile = "./logs/membership." . $year . ".log";
	error_log(date ( '[Y-m-d H:i e] ' ) . $logMessage . PHP_EOL, 3, $logFile);

    if ($connection->connect_error){
		error_log(date ( '[Y-m-d H:i e] ' ) . "connection error: " . $connection->connect_error . PHP_EOL, 3, $logFile);
		return;
	}

	// Get the current payment amount, so we can adjust relative to that value, which
	// will handle the case of refunds.
	$updatedPaymentAmount = cmgc_waitlist_get_current_payment($connection, $recordKey, $logFile) + $paymentAmount;

    $sqlCmd = "UPDATE `WaitingList` SET `Payment`= ?, `PaymentDateTime` = ?, `PayerName` = ? WHERE `RecordKey` = ?";
    $update = $connection->prepare ( $sqlCmd );

    if (! $update) {
        error_log(date ( '[Y-m-d H:i e] ' ) . $sqlCmd . " prepare failed: " . $connection->error . PHP_EOL, 3, $logFile);
		return;
    }

    $date = date ( 'Y-m-d H:i:s' );
    if (! $update->bind_param ( 'issi', $updatedPaymentAmount, $date, $payerName, $recordKey)) {
        error_log(date ( '[Y-m-d H:i e] ' ) . $sqlCmd . " bind_param failed: " . $connection->error . PHP_EOL, 3, $logFile);
		return;
    }
    
    if (! $update->execute ()) {
        error_log(date ( '[Y-m-d H:i e] ' ) . $sqlCmd . " execute failed: " . $connection->error . PHP_EOL, 3, $logFile);
		return;
    }
    $update->close ();

    error_log(date ( '[Y-m-d H:i e] ' ) . "Record key " . $recordKey . " updated payment to " . $updatedPaymentAmount . PHP_EOL, 3, $logFile);
}

function cmgc_application_get_current_payment($connection, $recordKey, $logFile){

	$sqlCmd = "SELECT Payment FROM `MembershipApplication` WHERE `RecordKey` = ?";
	$query = $connection->prepare ( $sqlCmd );

	if (! $query->bind_param ( 'i', $recordKey )) {
		error_log(date ( '[Y-m-d H:i e] ' ) . $sqlCmd . " bind_param failed: " . $connection->error  . PHP_EOL, 3, $logFile);
		return 0;
	}

	if (! $query) {
		error_log(date ( '[Y-m-d H:i e] ' ) . $sqlCmd . " prepare failed: " . $connection->error  . PHP_EOL, 3, $logFile);
		return 0;
	}

	if (! $query->execute ()) {
		error_log(date ( '[Y-m-d H:i e] ' ) . $sqlCmd . " execute failed: " . $connection->error  . PHP_EOL, 3, $logFile);
		return 0;
	}

	$payment = 0;

	$query->bind_result ($payment);

	$query->fetch ();

	error_log(date ( '[Y-m-d H:i e] ' ) . "previous payment value was " . $payment  . PHP_EOL, 3, $logFile);

	$query->close();

	return $payment;
}

function cmgc_paid_application_fee($connection, $paymentAmount, $payerName, $recordKey, $logMessage){

    if (!file_exists('./logs')) {
		mkdir('./logs', 0755, true);
	}

	$now = new DateTime ( "now" );
	$year = $now->format('Y') + 1;

	$logFile = "./logs/MembershipApplication.log";
	error_log(date ( '[Y-m-d H:i e] ' ) . $logMessage . PHP_EOL, 3, $logFile);

    if ($connection->connect_error){
		error_log(date ( '[Y-m-d H:i e] ' ) . "connection error: " . $connection->connect_error . PHP_EOL, 3, $logFile);
		return;
	}

	// Get the current payment amount, so we can adjust relative to that value, which
	// will handle the case of refunds.
	$updatedPaymentAmount = cmgc_application_get_current_payment($connection, $recordKey, $logFile) + $paymentAmount;

    $sqlCmd = "UPDATE `MembershipApplication` SET `Payment`= ?, `PaymentDateTime` = ?, `PayerName` = ? WHERE `RecordKey` = ?";
    $update = $connection->prepare ( $sqlCmd );

    if (! $update) {
        error_log(date ( '[Y-m-d H:i e] ' ) . $sqlCmd . " prepare failed: " . $connection->error . PHP_EOL, 3, $logFile);
		return;
    }

    $date = date ( 'Y-m-d H:i:s' );
    if (! $update->bind_param ( 'issi', $updatedPaymentAmount, $date, $payerName, $recordKey)) {
        error_log(date ( '[Y-m-d H:i e] ' ) . $sqlCmd . " bind_param failed: " . $connection->error . PHP_EOL, 3, $logFile);
		return;
    }
    
    if (! $update->execute ()) {
        error_log(date ( '[Y-m-d H:i e] ' ) . $sqlCmd . " execute failed: " . $connection->error . PHP_EOL, 3, $logFile);
		return;
    }
    $update->close ();

    error_log(date ( '[Y-m-d H:i e] ' ) . "Record key " . $recordKey . " updated payment to " . $updatedPaymentAmount . PHP_EOL, 3, $logFile);
}
?>