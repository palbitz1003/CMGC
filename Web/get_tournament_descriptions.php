<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_descriptions_functions.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$connection = new mysqli ('p:' . $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( $connection->connect_error );

$t = GetTournamentDescriptions($connection);

for($i = 0; $i < count($t); ++$i){
	// Send comma separated values.  Add double quotes around name and description, since they may have commas
	echo $t[$i]->TournamentDescriptionKey . ',"' . $t[$i]->Name . '","' . $t[$i]->Description . '"' . PHP_EOL;
}

$connection->close ();
?>