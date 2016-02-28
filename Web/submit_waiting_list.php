<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$connection = new mysqli ( $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( $connection->connect_error );

//var_dump($_POST);

login($_POST ['Login'], $_POST ['Password']);
	

if (! isset ( $_POST ['WaitingList'] )) {
	die ( "No waiting list provided." );
} else {
	clear_table($connection, 'WaitingList');
	
	for($i = 0; $i < count ( $_POST ['WaitingList'] ); ++ $i) {
		
		$sqlCmd = "INSERT INTO `WaitingList` VALUES (?, ?, ?)";
		$insert = $connection->prepare ( $sqlCmd );
		
		if (! $insert) {
			die ( $sqlCmd . " prepare failed: " . $connection->error );
		}
		
		// Change upper case name to normal case
		$name = FixNameCasing($_POST ['WaitingList'][$i]['Name']);
		
		if (! $insert->bind_param ( 'iss', $_POST ['WaitingList'][$i]['Position'], $name, $_POST ['WaitingList'][$i]['DateAdded'] )) {
			die ( $sqlCmd . " bind_param failed: " . $connection->error );
		}
		
		if (! $insert->execute ()) {
			die ( $sqlCmd . " execute failed: " . $connection->error );
		}
	}
	
$connection->close ();
echo 'Success';
}
?>