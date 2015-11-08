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

$t->LocalHandicap = IntToBool($t->LocalHandicap);
$t->ScgaTournament = IntToBool($t->ScgaTournament);
$t->SendEmail = IntToBool($t->SendEmail);
$t->RequirePayment = IntToBool($t->RequirePayment);
$t->Eclectic = IntToBool($t->Eclectic);
$t->Stableford = IntToBool($t->Stableford);
$t->SCGAQualifier = IntToBool($t->SCGAQualifier);
$t->SrClubChampionship = IntToBool($t->SrClubChampionship);

echo json_encode($t);

$connection->close ();
?>