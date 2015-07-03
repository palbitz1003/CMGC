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

if (! isset ( $_POST ['LocalHandicap'] )) {
	die ( "No list of local handicaps" );
} else {
	if(isset ( $_POST ['ClearTable'])){
	clear_table ( $connection, 'LocalHandicap');
	}
	
	if(isset( $_POST['LocalHandicapDate'])){
		$sqlCmd = "UPDATE `Misc` SET `LocalHandicapDate` = ?";
		
		$update = $connection->prepare ( $sqlCmd );
		
		if (! $update) {
			die ( $sqlCmd . " prepare failed: " . $connection->error );
		}
		
		if (! $update->bind_param ( 's',  $_POST['LocalHandicapDate'])) {
			die ( $sqlCmd . " bind_param failed: " . $connection->error );
		}
		
		if (! $update->execute ()) {
			die ( $sqlCmd . " execute failed: " . $connection->error );
		}
	}

	for($i = 0; $i < count ( $_POST ['LocalHandicap'] ); ++ $i) {
		
		$sqlCmd = "INSERT INTO `LocalHandicap` VALUES (?, ?, ?, ?)";
		$insert = $connection->prepare ( $sqlCmd );
		
		if (! $insert) {
			die ( $sqlCmd . " prepare failed: " . $connection->error );
		}
		
		$SCGAHandicapNeg = str_replace("+", "-", $_POST ['LocalHandicap'] [$i] ['SCGAHandicap']);
		$LocalHandicapNeg = str_replace("+", "-", $_POST ['LocalHandicap'] [$i] ['LocalHandicap']);
		if($SCGAHandicapNeg < $LocalHandicapNeg){
			$lowerHandicap = (float)$SCGAHandicapNeg;
		}
		else {
			$lowerHandicap = (float)$LocalHandicapNeg;
		}
		$calculatedHandicap = round($lowerHandicap * 117.0 / 113.0);
		$calculatedHandicap = str_replace("-", "+", (string)$calculatedHandicap);
		
		if (! $insert->bind_param ( 'isss', $_POST ['LocalHandicap'] [$i] ['GHIN'], $_POST ['LocalHandicap'] [$i] ['SCGAHandicap'], $_POST ['LocalHandicap'] [$i] ['LocalHandicap'], $calculatedHandicap )) {
			die ( $sqlCmd . " bind_param failed: " . $connection->error );
		}
		
		if (! $insert->execute ()) {
			die ( $sqlCmd . " execute failed: " . $connection->error );
		}

		$insert->close();
		
	}
}

$connection->close ();
echo 'Success';
?>