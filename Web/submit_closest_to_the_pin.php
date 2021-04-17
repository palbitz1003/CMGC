<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/results_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$connection = new mysqli ( $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( $connection->connect_error );
	
	// var_dump($_POST);

login ( $_POST ['Login'], $_POST ['Password'] );

if (! isset ( $_POST ['TournamentKey'] ) || !is_numeric($_POST ['TournamentKey'])) {
	die ( "No tournament key provided." );
} else {
	if (isset ( $_POST ['ClearClosestToPin'] )) {
		// delete the previous values
		ClearTableWithTournamentKey ( $connection, 'ClosestToThePin', $_POST ['TournamentKey'] );
		
		$emptyDate = TournamentDetails::EMPTYDATE;
		UpdateTournamentDetails ( $connection, $_POST ['TournamentKey'], 'ClosestToThePinPostedDate', $emptyDate );
	}
	
	if (isset ( $_POST ['CTP'] )) {
		for($i = 0; $i < count ( $_POST ['CTP'] ); ++ $i) {
			
			$sqlCmd = "INSERT INTO `ClosestToThePin` VALUES (?, ?, ?, ?, ?, ?, ?, ?)";
			$insert = $connection->prepare ( $sqlCmd );
			
			if (! $insert) {
				die ( $sqlCmd . " prepare failed: " . $connection->error );
			}
			
			// Remove the slashes that PHP adds to single and double quotes
			$date = stripslashes ( $_POST ['CTP'] [$i] ['Date'] );
			$ghin = stripslashes ( $_POST ['CTP'] [$i] ['GHIN'] );
			$name = FixNameCasing ( $_POST ['CTP'] [$i] ['Name'] );
			$hole = stripslashes ( $_POST ['CTP'] [$i] ['Hole'] );
			$distance = stripslashes ( $_POST ['CTP'] [$i] ['Distance'] );
			$prize = stripslashes ( $_POST ['CTP'] [$i] ['Prize'] );
			$business = stripslashes ( $_POST ['CTP'] [$i] ['Business'] );
			
			if (! $insert->bind_param ( 'isisisss', $_POST ['TournamentKey'], $date, $ghin, $name, $hole, $distance, $prize, $business )) {
				die ( $sqlCmd . " bind_param failed: " . $connection->error );
			}
			
			if (! $insert->execute ()) {
				die ( $sqlCmd . " execute failed: " . $connection->error );
			}
			$insert->close ();
		}
		
		$date = date ( 'Y-m-d' );
		UpdateTournamentResultsField ( $connection, $_POST ['TournamentKey'], 'ClosestToThePinPostedDate', $date, 's' );
	}
	
	$connection->close ();
	echo 'Success';
}
?>