<?php
class TournamentDescription {
	public $TournamentDescriptionKey;
	public $Name;
	public $Description;
}
function GetTournamentDescriptions($connection){
	$sqlCmd = "SELECT * FROM `TournamentDescriptions` ORDER BY `Name` ASC";
	$tournament = $connection->prepare ( $sqlCmd );
	
	if (! $tournament) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	if (! $tournament->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
	
	$tournament->bind_result ( $tournamentDescriptionKey, $name, $description );
	
	$tournamentDescriptions = array();
	while ( $tournament->fetch () ) {
		$t = new TournamentDescription();
		$t->TournamentDescriptionKey = $tournamentDescriptionKey;
		$t->Name = $name;
		$t->Description = $description;
		$tournamentDescriptions[] = $t;
	}
	
	$tournament->close ();
	
	return $tournamentDescriptions;
}
function GetTournamentDescription($connection, $tournamentDescriptionKey){
	$sqlCmd = "SELECT * FROM `TournamentDescriptions` WHERE `TournamentDescriptionKey` = ?";
	$tournament = $connection->prepare ( $sqlCmd );
	
	if (! $tournament) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	if (! $tournament->bind_param ( 'i', $tournamentDescriptionKey )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}
	
	if (! $tournament->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
	
	$tournament->bind_result ( $key, $name, $description );
	
	$t = null;
	if($tournament->fetch ()){
		$t = new TournamentDescription();
		$t->TournamentDescriptionKey = $tournamentDescriptionKey;
		$t->Name = $name;
		$t->Description = $description;
	}
	
	$tournament->close ();
	
	return $t;
}
function InsertTournamentDescription($connection, $name, $description){
	$sqlCmd = "INSERT INTO `TournamentDescriptions` VALUES (NULL, ?, ?)";
	$insert = $connection->prepare ( $sqlCmd );
	
	if (! $insert) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	if (! $insert->bind_param ( 'ss', $name, $description )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}
	
	if (! $insert->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
	
	// echo 'insert id is: ' . $insert->insert_id . '<br>';
	return $insert->insert_id;
}
function UpdateTournamentDescription($connection, $tournamentDescriptionKey, $field, $value){

	$sqlCmd = "UPDATE `TournamentDescriptions` SET `" . $field . "` = ? WHERE `TournamentDescriptionKey` = ?";
	
	$update = $connection->prepare ( $sqlCmd );
	
	if (! $update) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	if (! $update->bind_param ( 'si' ,  $value, $tournamentDescriptionKey)) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}
	
	if (! $update->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
	
	$update->close ();
}
function DeleteTournamentDescription($connection, $tournamentDescriptionKey){
	$sqlCmd = "DELETE FROM `TournamentDescriptions` WHERE `TournamentDescriptionKey` = ?";
	
	$delete = $connection->prepare ( $sqlCmd );
	
	if (! $delete) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	if (! $delete->bind_param ( 'i',  $tournamentDescriptionKey)) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}
	
	if (! $delete->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}

	$delete->close ();
}
?>