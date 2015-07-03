<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
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

echo $t->TournamentKey . ',';
echo $t->Name . ',';
echo $t->Year . ',';
echo $t->StartDate . ',';
echo $t->EndDate . ',';
echo $t->SignupStartDate . ',';
echo $t->SignupEndDate . ',';
echo $t->CancelEndDate . ',';
echo $t->LocalHandicap . ',';
echo $t->ScgaTournament . ',';
echo $t->TeamSize . ',';
echo $t->TournamentDescriptionKey . ',';
echo $t->Cost . ',';
echo $t->Pool . ',';
echo $t->ChairmanName . ',';
echo $t->ChairmanEmail . ',';
echo $t->ChairmanPhone . ',';
echo $t->Stableford . ',';
echo $t->Eclectic . ',';
echo $t->SendEmail . ',';
echo $t->RequirePayment . ',';
echo $t->SCGAQualifier . ',';
echo $t->SrClubChampionship;

$connection->close ();
?>