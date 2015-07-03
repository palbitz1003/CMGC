<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/signup functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$connection = new mysqli ( $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( $connection->connect_error );

$tournamentKey = $_POST ['Tournament'];
if (! $tournamentKey) {
	die ( "Missing tournament key" );
}
// var_dump($_POST);
	
login($_POST ['Login'], $_POST ['Password']);

ClearTableWithTournamentKey ( $connection, 'SignUpsWaitingList', $tournamentKey );

for($i = 0; $i < count ( $_POST ['SignUpsWaitingList'] ); ++ $i) {
	$signUpWaitingList = new SignUpWaitingListClass();
	$signUpWaitingList->TournamentKey = $tournamentKey;
	$signUpWaitingList->Position = $_POST ['SignUpsWaitingList'] [$i] ['Position'];
	$signUpWaitingList->GHIN1 = $_POST ['SignUpsWaitingList'] [$i] ['GHIN1'];
	$signUpWaitingList->Name1 = $_POST ['SignUpsWaitingList'] [$i] ['Name1'];
	$signUpWaitingList->GHIN2 = $_POST ['SignUpsWaitingList'] [$i] ['GHIN2'];
	$signUpWaitingList->Name2 = $_POST ['SignUpsWaitingList'] [$i] ['Name2'];
	$signUpWaitingList->GHIN3 = $_POST ['SignUpsWaitingList'] [$i] ['GHIN3'];
	$signUpWaitingList->Name3 = $_POST ['SignUpsWaitingList'] [$i] ['Name3'];
	$signUpWaitingList->GHIN4 = $_POST ['SignUpsWaitingList'] [$i] ['GHIN4'];
	$signUpWaitingList->Name4 = $_POST ['SignUpsWaitingList'] [$i] ['Name4'];
	
	InsertSignUpWaitingListEntry($connection, $signUpWaitingList);
}

$connection->close ();
echo 'Success';
?>