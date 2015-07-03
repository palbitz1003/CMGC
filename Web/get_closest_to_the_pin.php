<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/results_functions.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$connection = new mysqli ('p:' . $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( $connection->connect_error );

if (isset ( $_POST ['TournamentKey'] )) {
	$tournamentKey = $_POST ['TournamentKey'];
} else {
	die('TournamentKey is not set');
}

$t = GetTournament($connection, $_POST ['TournamentKey']);

$ctpArray = GetClosestToThePin($connection, $tournamentKey, $t->StartDate);
SendClosestToThePin($ctpArray);

if (strcmp ( $t->StartDate, $t->EndDate ) != 0){
	$ctpArray = GetClosestToThePin($connection, $tournamentKey, $t->EndDate);
	SendClosestToThePin($ctpArray);
}

function SendClosestToThePin($ctpArray){
	for($i = 0; $i < count($ctpArray); ++$i){
		echo $ctpArray[$i]->Hole . ',';
		echo $ctpArray[$i]->Date . ',';
		echo $ctpArray[$i]->GHIN . ',';
		echo '"' . $ctpArray[$i]->Name . '"' . ',' ;  // name can have commas, so escape it
		echo $ctpArray[$i]->Distance . ',';
		echo $ctpArray[$i]->Prize . ',';
		echo $ctpArray[$i]->Business;
		echo PHP_EOL;
	}
}
?>