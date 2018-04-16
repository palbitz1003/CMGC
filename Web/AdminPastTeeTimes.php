<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';

$connection = new mysqli ('p:' . $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( $connection->connect_error );

$time = new DateTime('now');
$oneYearAgo = $time->modify('-1 year')->format('Ymd');

$sqlCmd = 'SELECT ' .
	'TeeTimesPlayers.Name, ' .
	'SignUps.RequestedTime, ' .
	'TeeTimes.StartTime, ' .
	'Tournaments.Name, ' .
	'Tournaments.StartDate, ' . 
	'Tournaments.TournamentKey ' .
'FROM Tournaments ' .
	'INNER JOIN TeeTimes ' .
		'ON Tournaments.TournamentKey = TeeTimes.TournamentKey ' .
	'INNER JOIN TeeTimesPlayers '.
		'ON TeeTimes.TeeTimeKey = TeeTimesPlayers.TeeTimeKey ' .
	'INNER JOIN SignUpsPlayers ' .
		'ON Tournaments.TournamentKey = SignUpsPlayers.TournamentKey AND TeeTimesPlayers.Name = SignUpsPlayers.LastName ' .
	'INNER JOIN SignUps ' .
		'ON SignUps.SubmitKey = SignUpsPlayers.SignUpKey ' .
'WHERE Tournaments.StartDate < ' . date("Ymd") . ' ' .
'AND Tournaments.EndDate > ' .  $oneYearAgo . ' ' .
'AND Tournaments.MatchPlay = 0 ' .
"AND TeeTimesPlayers.Name <> '' " .
'ORDER BY TeeTimesPlayers.Name ASC, Tournaments.StartDate ASC';


$teeTimes = $connection->prepare ( $sqlCmd );

if (! $teeTimes) {
	die ( $sqlCmd . " prepare failed: " . $connection->error );
}

if (! $teeTimes->execute ()) {
	die ( $sqlCmd . " execute failed: " . $connection->error );
}

$teeTimes->bind_result ( $name, $requestedTime, $startTime, $tournamentName, $startDate, $tournamentKey );

$t = null;
while($teeTimes->fetch ()){
	echo '"' . $name . '"' . ',' . $requestedTime . ',' . $startTime . ',' . $tournamentName . ',' . $startDate . '<br>';
}

$connection->close ();

?>