<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/signup functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$connection = new mysqli ( $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( 'db connect error: ' . $connection->connect_error );
	
	// var_dump($_POST);

login ( $_POST ['Login'], $_POST ['Password'] );

if ($_POST ['Action'] == 'Delete') {
	
} else if ($_POST ['Action'] == 'UpdatePaymentMade') {
	
	$tournamentKey = $_POST ['TournamentKey'];
	if (! $tournamentKey || !is_numeric($tournamentKey)) {
		die ( "Missing tournament key" );
	}
	
	for($i = 0; $i < count ( $_POST ['Signup'] ); ++ $i) {
		UpdateSignup($connection,$_POST ['Signup'][$i]['SignupKey'], 'Payment', $_POST ['Signup'][$i]['PaymentMade'], 'd');
	}
}
echo "Success";
?>