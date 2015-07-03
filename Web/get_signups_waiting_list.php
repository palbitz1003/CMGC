<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/signup functions.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$tournamentKey = $_POST ['tournament'];
if (! $tournamentKey) {
	die ( "Which tournament?" );
}

$connection = new mysqli ('p:' . $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( $connection->connect_error );

$entries = GetSignUpWaitingList($connection, $tournamentKey);

for($i = 0; $i < count($entries); ++$i){
	echo $entries[$i]->GHIN1 . ',"' . $entries[$i]->Name1 . '",';
	echo $entries[$i]->GHIN2 . ',"' . $entries[$i]->Name2 . '",';
	echo $entries[$i]->GHIN3 . ',"' . $entries[$i]->Name3 . '",';
	echo $entries[$i]->GHIN4 . ',"' . $entries[$i]->Name4 . '",' . "\n";
}

$connection->close ();
?>