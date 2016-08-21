<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$connection = new mysqli ( $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( $connection->connect_error );
	
	// var_dump($_POST);

login($_POST ['Login'], $_POST ['Password']);

$errors = null;
if (! isset ( $_POST ['Roster'] )) {
	die ( "No roster provided." );
} else {
	
	for($i = 0; $i < count ( $_POST ['Roster'] ); ++ $i) {
		
		$sqlCmd = "SELECT * FROM `Roster` WHERE `GHIN` = ?";
		$query = $connection->prepare ( $sqlCmd );
		
		if (! $query) {
			die ( $sqlCmd . " prepare failed: " . $connection->error );
		}
		
		if (! $query->bind_param ( 'i', $_POST ['Roster'] [$i] ['GHIN'] )) {
			die ( $sqlCmd . " bind_param failed: " . $connection->error );
		}
		
		if (! $query->execute ()) {
			die ( $sqlCmd . " execute failed: " . $connection->error );
		}
		
		if ($query->fetch ()) {
			// record exists, update it
			$query->close ();
			
			$sqlCmd = "UPDATE `Roster` SET `Email`= ?, `BirthDate` = ?, `MembershipType` = ? WHERE `GHIN` = ?";
			$update = $connection->prepare ( $sqlCmd );
			
			if (! $update) {
				die ( $sqlCmd . " prepare failed: " . $connection->error );
			}
			
			if(empty($_POST ['Roster'] [$i] ['MembershipType'])){
				die ("Missing membership type for GHIN " . $_POST ['Roster'] [$i] ['GHIN']);
			}
			
			if (! $update->bind_param ( 'sssi', $_POST ['Roster'] [$i] ['Email'], $_POST ['Roster'] [$i] ['Birthdate'], $_POST ['Roster'] [$i] ['MembershipType'], $_POST ['Roster'] [$i] ['GHIN'] )) {
				die ( $sqlCmd . " bind_param failed: " . $connection->error );
			}
			
			if (! $update->execute ()) {
				die ( $sqlCmd . " execute failed: " . $connection->error );
			}
			$update->close ();
		} else {
			// record does not exist, skip it
			$query->close ();
			
			$errors = $errors . "<br>Failed to find player for GHIN " . $_POST ['Roster'] [$i] ['GHIN'];
		}
	}
	
	$connection->close ();
	if(!empty($errors)){
		echo $errors;
	}
	else {
		echo 'Success';
	}
}
?>