<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/dues_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$connection = new mysqli ( $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( $connection->connect_error );
	
	// var_dump($_POST);

login($_POST ['Login'], $_POST ['Password']);

if (! isset ( $_POST ['GHIN'] )) {
	die ( "No GHIN provided." );
} else if(!isset($_POST['Name'])){
	die ( "No name provided." );
} else if(!isset($_POST['Payment'])){
		die ( "No payment provided." );
} else {
	$now = new DateTime ( "now" );
	$year = $now->format('Y');
	$payment = $_POST['Payment'];
	
	$player = GetPlayerDues($connection, $_POST ['GHIN']);
	
	if(empty($player)){
		InsertPlayerForDues($connection, $year + 1, $_POST ['GHIN'], $_POST['Name']);
	}
	else {
		// Update name in case the name was uploaded wrong the first time because
		// auto-complete was off in WebAdmin and they typed in a name and GHIN.
		UpdateName($connection, $_POST ['GHIN'], $_POST['Name']);
		
		// If set from WebAdmin, make the starting point 0 by subtracting what is already
		// in the payment field.
		if($player->Payment > 0){
			$payment = $payment - $player->Payment;
		}
	}
	
	$logMessage = "GHIN = " . $_POST ['GHIN'] . ", name = " . $_POST['Name'] . ", payment = " . $_POST['Payment'];
	UpdateDuesDatabase($connection, $_POST ['GHIN'], $payment, "WebAdmin", "", "WebAdmin: " . $logMessage);

	$connection->close ();
	echo 'Success';
}

function UpdateName($connection, $ghin, $name)
{
	$player = GetPlayerDues($connection, $ghin);
	if(empty($player)){
		return;
	}
	
	$sqlCmd = "UPDATE `Dues` SET `Name`= ? WHERE `GHIN` = ?";
	$update = $connection->prepare ( $sqlCmd );
	
	if (! $update) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	if (! $update->bind_param ( 'si',  $name, $ghin)) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}
	
	if (! $update->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
	
	$update->close ();
}
?>