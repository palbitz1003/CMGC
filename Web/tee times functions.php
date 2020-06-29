<?php
class DatabaseTeeTime {
	public $Key;
	public $StartTime;
	public $StartHole;
	public $Players;
	public $GHIN;
	public $Position;
}
class DatabasePlayer {
	public $GHIN;
	public $LastName;
	public $FirstName;
	public $Handicap;
}
function InsertTeeTime($connection, $tournamentKey, $teeTime, $startHole) {
	$sqlCmd = "INSERT INTO `TeeTimes` VALUES (NULL, ?, ?, ?)";
	$insert = $connection->prepare ( $sqlCmd );
	
	if (! $insert) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	if (! $insert->bind_param ( 'iss', $tournamentKey, $teeTime, $startHole )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}
	
	if (! $insert->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
	
	// echo 'insert: ' . $teeTime . " id: " . $insert->insert_id . "\n";
	return $insert->insert_id;
}
function InsertTeeTimePlayer($connection, $teeTimeKey, $tournamentKey, $GHIN, $name, $playerIndex) {
	$sqlCmd = "INSERT INTO `TeeTimesPlayers` VALUES (?, ?, ?, ?, ?)";
	$insert = $connection->prepare ( $sqlCmd );
	
	if (! $insert) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	if (! $insert->bind_param ( 'iiisi', $teeTimeKey, $tournamentKey, $GHIN, $name, $playerIndex )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}
	
	if (! $insert->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
	// echo "inserted " . $name . '(' . $GHIN . ") for key " . $teeTimeKey . "\n";
}
function GetTeeTimes($connection, $tournamentKey) {
	$sqlCmd = "SELECT * FROM `TeeTimes` WHERE `TournamentKey` = ? ORDER BY `StartTime` ASC";
	$query = $connection->prepare ( $sqlCmd );
	
	if (! $query) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	if (! $query->bind_param ( 'i', $tournamentKey )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}
	
	if (! $query->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
	
	$query->bind_result ( $key, $tournament, $startTime, $startHole );
	
	while ( $query->fetch () ) {
		$teeTime = new DatabaseTeeTime ();
		$teeTime->Key = $key;
		$teeTime->StartTime = $startTime;
		$teeTime->StartHole = $startHole;
		$teeTimeArray [] = $teeTime;
	}
	
	$query->close ();
	
	if (! $teeTimeArray || (count ( $teeTimeArray ) == 0)) {
		return;
	}
	
	for($i = 0; $i < count ( $teeTimeArray ); ++ $i) {
		$teeTimeArray [$i]->Players = GetPlayersForTeeTime ( $connection, $teeTimeArray [$i]->Key );
	}
	
	return $teeTimeArray;
}
function GetPlayersForTeeTime($connection, $teeTimeKey) {
	$sqlCmd = "SELECT * FROM `TeeTimesPlayers` WHERE `TeeTimeKey` = ? ORDER BY `Position` ASC";
	$query = $connection->prepare ( $sqlCmd );
	
	if (! $query) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	if (! $query->bind_param ( 'i', $teeTimeKey )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}
	
	if (! $query->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
	
	$query->bind_result ( $key, $tournament, $GHIN, $Name, $Position );
	
	// if (! $forWeb) {
	// echo date ( 'g:i', strtotime ( $teeTime ) );
	// }
	
	$playerCount = 0;
	$playerArray = array ();
	while ( $query->fetch () ) {
		$player = new DatabasePlayer ();
		$player->GHIN = $GHIN;
		$player->LastName = $Name;
		$playerArray [] = $player;
	}
	
	$query->close ();
	
	for($i = 0; $i < count ( $playerArray ); ++ $i) {
		$playerArray [$i]->Handicap = GetPlayerHandicap ( $connection, $playerArray [$i]->GHIN );
	}
	return $playerArray;
}
function GetPlayerHandicap($connection, $GHIN) {
	$sqlCmd = "SELECT * FROM `LocalHandicap` WHERE `GHIN` = ?";
	$query = $connection->prepare ( $sqlCmd );
	
	if (! $query) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	if (! $query->bind_param ( 'i', $GHIN )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}
	
	if (! $query->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error . ' (GHIN was ' . $GHIN . ')' );
	}
	
	$query->bind_result ( $GHIN, $SCGAHandicap, $LocalHandicap, $TournamentHandicap );
	
	while ( $query->fetch () ) {
		$query->close ();
		
		$SCGAHandicapNeg = str_replace ( "+", "-", $SCGAHandicap );
		$LocalHandicapNeg = str_replace ( "+", "-", $LocalHandicap );
		if ($SCGAHandicapNeg < $LocalHandicapNeg) {
			$lowerHandicap = $SCGAHandicapNeg;
		} else {
			$lowerHandicap = $LocalHandicapNeg;
		}
		
		return str_replace ( "-", "+", $lowerHandicap );
	}
	
	$query->close ();
	return 0;
}



?>