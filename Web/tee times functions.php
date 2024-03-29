<?php
class DatabaseTeeTime {
	public $Key;
	public $StartTime;
	public $StartHole;
	public $Players;
	public $GHIN;
	public $Extra;
	public $SignupKey;
}
class DatabaseTeeTimePlayer {
	public $GHIN;
	public $LastName;
	public $FirstName;
	public $Handicap;
	public $Extra;
	public $SignupKey;
}

class TeeTimeWaitingListClass {
	public $TournamentKey;
	public $Position;
	public $GHIN;
	public $Name;
	public $Extra;
}

class TeeTimeCancelledPlayer {
	public $TournamentKey;
	public $Position;
	public $GHIN;
	public $Name;
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
function InsertTeeTimePlayer($connection, $teeTimeKey, $tournamentKey, $GHIN, $name, $extra, $playerIndex, $signupKey) {
	$sqlCmd = "INSERT INTO `TeeTimesPlayers` VALUES (?, ?, ?, ?, ?, ?, ?)";
	$insert = $connection->prepare ( $sqlCmd );
	
	if (! $insert) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	// null is not allowed
	if(empty($extra)){
		$extra = "";
	}
	if (! $insert->bind_param ( 'iiisisi', $teeTimeKey, $tournamentKey, $GHIN, $name, $playerIndex, $extra, $signupKey )) {
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
	
	$teeTimeArray = array();
	
	while ( $query->fetch () ) {
		$teeTime = new DatabaseTeeTime ();
		$teeTime->Key = $key;
		$teeTime->StartTime = $startTime;
		$teeTime->StartHole = $startHole;
		$teeTimeArray [] = $teeTime;
	}
	
	$query->close ();
	
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
	
	$query->bind_result ( $key, $tournament, $GHIN, $Name, $Position, $extra, $signupKey );
	
	// if (! $forWeb) {
	// echo date ( 'g:i', strtotime ( $teeTime ) );
	// }
	
	$playerCount = 0;
	$playerArray = array ();
	while ( $query->fetch () ) {
		$player = new DatabaseTeeTimePlayer ();
		$player->GHIN = $GHIN;
		$player->LastName = $Name;
		$player->Extra = $extra;
		$player->SignupKey = $signupKey;
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

function InsertSignUpWaitingListEntry($connection, $signUpWaitingList){
	$sqlCmd = "INSERT INTO `TeeTimesWaitingList` VALUES (?, ?, ?, ?, ?)";
	$insert = $connection->prepare ( $sqlCmd );
	
	if (! $insert) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	if (! $insert->bind_param ( 'iiiss', $signUpWaitingList->TournamentKey, $signUpWaitingList->Position, $signUpWaitingList->GHIN, $signUpWaitingList->Name, $signUpWaitingList->Extra )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}
	
	if (! $insert->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
}

function GetTeeTimeWaitingList($connection, $tournamentKey){
	$sqlCmd = "SELECT * FROM `TeeTimesWaitingList` WHERE TournamentKey = ? ORDER BY `Position` ASC";
	$entries = $connection->prepare ( $sqlCmd );
	
	if (! $entries) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	if (! $entries->bind_param ( 'i', $tournamentKey )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}
	
	if (! $entries->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
	
	$entries->bind_result ( $key, $position, $ghin, $name, $extra);
	
	$waitingList = array();
	while ( $entries->fetch () ) {
		$entry = new TeetimeWaitingListClass();
		$entry->TournamentKey = $tournamentKey;
		$entry->Position = $position;
		$entry->GHIN = $ghin;
		$entry->Name = $name;
		$entry->Extra = $extra;
		$waitingList[] = $entry;
	}
	
	$entries->close ();
	
	return $waitingList;
}

function InsertTeeTimeCancelledPlayer($connection, $teeTimeCancelledPlayer){
	$sqlCmd = "INSERT INTO `TeeTimesCancelled` VALUES (?, ?, ?, ?)";
	$insert = $connection->prepare ( $sqlCmd );
	
	if (! $insert) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	if (! $insert->bind_param ( 'iiis', $teeTimeCancelledPlayer->TournamentKey, $teeTimeCancelledPlayer->Position, $teeTimeCancelledPlayer->GHIN, $teeTimeCancelledPlayer->Name)) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}
	
	if (! $insert->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
}

function GetTeeTimesCancelledList($connection, $tournamentKey){
	$sqlCmd = "SELECT * FROM `TeeTimesCancelled` WHERE TournamentKey = ? ORDER BY `Position` ASC";
	$entries = $connection->prepare ( $sqlCmd );
	
	if (! $entries) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	if (! $entries->bind_param ( 'i', $tournamentKey )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}
	
	if (! $entries->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
	
	$entries->bind_result ( $key, $position, $ghin, $name );
	
	$teeTimesCancelledList = array();
	while ( $entries->fetch () ) {
		$entry = new TeeTimeCancelledPlayer();
		$entry->TournamentKey = $tournamentKey;
		$entry->Position = $position;
		$entry->GHIN = $ghin;
		$entry->Name = $name;
		$teeTimesCancelledList[] = $entry;
	}
	
	$entries->close ();
	
	return $teeTimesCancelledList;
}

function ShowWaitingListTable($waitingList, $columns){

	if(empty($waitingList) || (count($waitingList) == 0)){
		return;
	}

	$minWidth = "";
	if($columns > 1){
		$minWidth = 'min-width:500px;';
	}

	echo '<table style="' . $minWidth . 'margin-left:auto;margin-right:auto">' . PHP_EOL;
	echo '<thead><tr class="header"><th  colspan="' . $columns . '">Waitlist</th></tr></thead>' . PHP_EOL;
	echo '<tbody>' . PHP_EOL;

	$width = 100 / $columns;
	
	for($i = 0; $i < count ( $waitingList );) {
		echo '<tr>' . PHP_EOL;
		$cols = 0;
		for(; ($cols < $columns) && ($i < count($waitingList)); ++$cols, ++$i){
			$position = "";
			if($columns == 1){
				$position = $i + 1;
				$position = $position . ". ";
			}
			if($cols == 0){
				echo '<td style="width:' . $width . '%;">' . $position . $waitingList[$i]->Name . '</td>' . PHP_EOL;
			}
			else {
				// would be better for a style to provide the border ...
				echo '<td style="border-left: 1px solid #ccc;width:' . $width . '%;">' . $position . $waitingList[$i]->Name . '</td>' . PHP_EOL;
			}
		}
		// Finish the column data to add in all the border lines
		for(;$cols < $columns; ++$cols){
			echo '<td style="border-left: 1px solid #ccc;width:' . $width . '%;"></td>' . PHP_EOL;
		}
		echo '</tr>' . PHP_EOL;
	}
	echo '</tbody>' . PHP_EOL;
	echo '</table>' . PHP_EOL;
}
?>