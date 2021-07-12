<?php
require_once realpath ( $_SERVER ["DOCUMENT_ROOT"] ) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/functions.php';
date_default_timezone_set ( 'America/Los_Angeles' );

class TournamentName {
	public $Name;
	public $StartDate;
	public $EndDate;
	public $SignupStartDate;
	public $TournamentKey;
	public $IsEclectic;
	public $MatchPlay;
	public $TeamSize;
	public $IsStableford;
	public $AnnouncementOnly;
	public $MemberGuest;
	
	private function IntToBool($value){
		return (!isset($value) || ($value == 0)) ? "false" : "true";
	}
	
	// I didn't figure out how to convert 0/1
	// to true/false on the receiving end ...
	public function ConvertToBool()
	{
		$this->IsEclectic = IntToBool($this->IsEclectic);
		$this->MatchPlay = IntToBool($this->MatchPlay);
		$this->IsStableford = IntToBool($this->IsStableford);
		$this->AnnouncementOnly = IntToBool($this->AnnouncementOnly);
		$this->MemberGuest = IntToBool($this->MemberGuest);
	}
}
$connection = new mysqli ( 'p:' . $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( $connection->connect_error );

$sqlCmd = "SELECT TournamentKey,Name,StartDate,EndDate,SignUpStartDate,Eclectic,MatchPlay,TeamSize,Stableford,AnnouncementOnly,MemberGuest FROM `Tournaments` ORDER BY `StartDate`";
$tournament = $connection->prepare ( $sqlCmd );

if (! $tournament) {
	die ( $sqlCmd . " prepare failed: " . $connection->error );
}

if (! $tournament->execute ()) {
	die ( $sqlCmd . " execute failed: " . $connection->error );
}

$tournament->bind_result ( $tournamentKey, $name, $startDate, $endDate, $signupStartDate, $isEclectic, $matchPlay, $teamSize, $isStableford, $announcementOnly, $memberGuest );

$tournamentNames = array();
while ( $tournament->fetch () ) {
	$t = new TournamentName();
	$t->Name = $name;
	$t->StartDate = $startDate;
	$t->EndDate = $endDate;
	$t->SignupStartDate = $signupStartDate;
	$t->TournamentKey = $tournamentKey;
	$t->IsEclectic = $isEclectic;
	$t->MatchPlay = $matchPlay;
	$t->TeamSize = $teamSize;
	$t->IsStableford = $isStableford;
	$t->AnnouncementOnly = $announcementOnly;
	$t->MemberGuest = $memberGuest;
	$t->ConvertToBool();
	$tournamentNames[] = $t;
}

echo json_encode($tournamentNames);

$tournament->close ();

$connection->close ();
?>