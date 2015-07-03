<?php
require_once realpath ( $_SERVER ["DOCUMENT_ROOT"] ) . '/login.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$connection = new mysqli ( 'p:' . $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( $connection->connect_error );

$sqlCmd = "SELECT TournamentKey,Name,StartDate,EndDate,SignUpStartDate,Eclectic FROM `Tournaments` ORDER BY `StartDate`";
$tournament = $connection->prepare ( $sqlCmd );

if (! $tournament) {
	die ( $sqlCmd . " prepare failed: " . $connection->error );
}

if (! $tournament->execute ()) {
	die ( $sqlCmd . " execute failed: " . $connection->error );
}

$tournament->bind_result ( $tournamentKey, $Name, $StartDate, $EndDate, $SignupStartDate, $IsEclectic );

while ( $tournament->fetch () ) {
	echo $tournamentKey . ',' . $Name . ',' . $StartDate . ',' . $EndDate . ',' . $SignupStartDate . ',' . $IsEclectic . "\n";
}

$tournament->close ();

$connection->close ();
?>