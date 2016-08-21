<?php
require_once realpath ( $_SERVER ["DOCUMENT_ROOT"] ) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/dues_functions.php';
date_default_timezone_set ( 'America/Los_Angeles' );


$connection = new mysqli ( 'p:' . $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( $connection->connect_error );

$sqlCmd = "SELECT * FROM `PayPalDues`";
$payPal = $connection->prepare ( $sqlCmd );

if (! $payPal) {
	die ( $sqlCmd . " prepare failed: " . $connection->error );
}

if (! $payPal->execute ()) {
	die ( $sqlCmd . " execute failed: " . $connection->error );
}

$payPal->bind_result ( $payPalButton, $fee, $membership );

$membershipTypes = array();
while($payPal->fetch ()){
	$membership = str_replace("_Late", "", $membership);
	if(!in_array($membership, $membershipTypes)){
		$membershipTypes[] = $membership;
	}
}

$payPal->close ();

echo json_encode($membershipTypes);

$connection->close ();
?>