<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_functions.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$connection = new mysqli ('p:' . $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( $connection->connect_error );

if (isset ( $_POST ['TournamentKey'] )) {
	$tournamentKey = $_POST ['TournamentKey'];
} else {
	die('TournamentKey is not set');
}
$t = GetTournament($connection, $tournamentKey);
$t->ConvertToBool();

echo json_encode($t);

$connection->close ();
?>