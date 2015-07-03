<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_descriptions_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$connection = new mysqli ( $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( 'db connect error: ' . $connection->connect_error );

//var_dump ( $_POST );

login ( $_POST ['Login'], $_POST ['Password'] );

if ($_POST ['Action'] == 'Insert') {
	if (empty ( $_POST ['TournamentDetailsName'] )) {
		die ( "Missing name for tournament description" );
	}
	if (empty ( $_POST ['TournamentDetailsDescription'] )) {
		die ( "Missing description for tournament description" );
	}
	InsertTournamentDescription ( $connection, $_POST ['TournamentDetailsName'], $_POST ['TournamentDetailsDescription'] );
	echo "Success";
	
} else if ($_POST ['Action'] == 'Update') {
	if (empty ( $_POST ['TournamentDetailsName'] )) {
		die ( "Missing name for tournament description" );
	}
	if (empty ( $_POST ['TournamentDetailsDescription'] )) {
		die ( "Missing description for tournament description" );
	}
	if(empty($_POST['TournamentDetailsKey'])){
		die("Missing tournament details key");
	}
	
	UpdateTournamentDescription($connection, $_POST['TournamentDetailsKey'], 'Name', $_POST ['TournamentDetailsName']);
	UpdateTournamentDescription($connection, $_POST['TournamentDetailsKey'], 'Description', stripslashes($_POST ['TournamentDetailsDescription']));
	echo "Success";
	
} else if ($_POST['Action'] == "Delete"){
	if(empty($_POST['TournamentDetailsKey'])){
		die("Missing tournament details key");
	}
	DeleteTournamentDescription($connection, $_POST['TournamentDetailsKey']);
	echo "Success";
}

?>