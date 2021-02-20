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

if (! isset ( $_POST ['GHIN'] )) {
	die ( "No GHIN entries provided." );
} else {
	
	if (isset ( $_POST ['SetAllInactive'] )) {
		// Mark all the players as inactive
		$sqlCmd = "UPDATE `Roster` SET `Active`=0";
		$setAllInactive = $connection->prepare ( $sqlCmd );
		if (! $setAllInactive) {
			die ( $sqlCmd . " prepare failed: " . $connection->error );
		}
		if (! $setAllInactive->execute ()) {
			die ( $sqlCmd . " execute failed: " . $connection->error );
		}
		$setAllInactive->close ();
	}
	
	for($i = 0; $i < count ( $_POST ['GHIN'] ); ++ $i) {
		
		$sqlCmd = "SELECT * FROM `Roster` WHERE `GHIN` = ?";
		$query = $connection->prepare ( $sqlCmd );
		
		if (! $query) {
			die ( $sqlCmd . " prepare failed: " . $connection->error );
		}
		
		if (! $query->bind_param ( 'i', $_POST ['GHIN'] [$i] ['GHIN'] )) {
			die ( $sqlCmd . " bind_param failed: " . $connection->error );
		}
		
		if (! $query->execute ()) {
			die ( $sqlCmd . " execute failed: " . $connection->error );
		}
		
		if ($query->fetch ()) {
			// record exists, update all the fields
			$query->close ();
			
			$sqlCmd = "UPDATE `Roster` SET `LastName`= ?, `FirstName` = ?,`Email`= ?, `BirthDate` = ?, `MembershipType` = ?, `SignupPriority` = ?,`Active` = 1 WHERE `GHIN` = ?";
			$update = $connection->prepare ( $sqlCmd );
			
			if (! $update) {
				die ( $sqlCmd . " prepare failed: " . $connection->error );
			}
			
			$email = $_POST ['GHIN'] [$i] ['Email'];
			$birthdate = $_POST ['GHIN'] [$i] ['Birthdate'];
			$membershipType = $_POST ['GHIN'] [$i] ['MembershipType'];
			$signupPriority = $_POST ['GHIN'] [$i] ['SignupPriority'];
			// default to G if empty
			if(strlen($signupPriority) == 0){
				$signupPriority = "G";
			}
			if (! $update->bind_param ( 'ssssssi', $_POST ['GHIN'] [$i] ['LastName'], $_POST ['GHIN'] [$i] ['FirstName'], $email, 
													$birthdate, $membershipType, $signupPriority, $_POST ['GHIN'] [$i] ['GHIN'] )) {
				die ( $sqlCmd . " bind_param failed: " . $connection->error );
			}
			
			if (! $update->execute ()) {
				die ( $sqlCmd . " execute failed: " . $connection->error );
			}
			$update->close ();
		} else {
			// record does not exist, just insert
			$query->close ();
			
			$sqlCmd = "INSERT INTO `Roster` VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)";
			$insert = $connection->prepare ( $sqlCmd );
			
			if (! $insert) {
				die ( $sqlCmd . " prepare failed: " . $connection->error );
			}
			
			$active = 1;
			$dateAdded = date ( "Y-n-j" );
			$email = $_POST ['GHIN'] [$i] ['Email'];
			$birthdate = $_POST ['GHIN'] [$i] ['Birthdate'];
			$membershipType = $_POST ['GHIN'] [$i] ['MembershipType'];
			$signupPriority = $_POST ['GHIN'] [$i] ['SignupPriority'];
			// default to G if empty
			if(strlen($signupPriority) == 0){
				$signupPriority = "G";
			}
			if (! $insert->bind_param ( 'ississsss', $_POST ['GHIN'] [$i] ['GHIN'], $_POST ['GHIN'] [$i] ['LastName'], $_POST ['GHIN'] [$i] ['FirstName'], 
													$active, $email, $birthdate, $dateAdded, $membershipType, $signupPriority )) {
				die ( $sqlCmd . " bind_param failed: " . $connection->error );
			}
			
			if (! $insert->execute ()) {
				die ( $sqlCmd . " execute failed: " . $connection->error );
			}
			$insert->close ();
		}
	}
	
	$connection->close ();
	echo 'Success';
}
?>