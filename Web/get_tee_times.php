<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tee times functions.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$connection = new mysqli ('p:' . $db_hostname, $db_username, $db_password, $db_database );

$tournamentKey = $_POST ['tournament'];
if (! $tournamentKey) {
	die ( "Which tournament?" );
}

if ($connection->connect_error)
	die ( $connection->connect_error );

$teeTimes = GetTeeTimes($connection, $tournamentKey);

for($i = 0; $i < count($teeTimes); ++$i){
	echo date ( 'g:i', strtotime ( $teeTimes[$i]->StartTime ) );
	for($j = 0; $j < count($teeTimes[$i]->Players); ++$j){
		echo ',"' . $teeTimes[$i]->Players[$j]->LastName . '",' . $teeTimes[$i]->Players[$j]->GHIN . ',' . $teeTimes[$i]->Players[$j]->Handicap;
	}
	echo PHP_EOL;
}

$connection->close ();

?>