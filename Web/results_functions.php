<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_functions.php';

class ClosestToThePin{
	public $TournamentKey;
	public $Date;
	public $GHIN;
	public $Name;
	public $Hole;
	public $Distance;
	public $Prize;
	public $Business;
}
class Pool {
	public $TournamentKey;
	public $Name;
	public $GHIN;
	public $Score;
	public $Winnings;
	public $Flight;
	public $Place;
	public $TeamNumber;
	public $Date;
	public $Hole;
}
class Chits {
	public $TournamentKey;
	public $Name;
	public $GHIN;
	public $Score;
	public $Winnings;
	public $Flight;
	public $Place;
	public $TeamNumber;
	public $Date;
	public $FlightName;
}
class Scores {
	public $TournamentKey;
	public $Name1;
	public $GHIN1;
	public $Name2;
	public $GHIN2;
	public $Name3;
	public $GHIN3;
	public $Name4;
	public $GHIN4;
	public $ScoreRound1;
	public $ScoreRound2;
	public $ScoreTotal;
	public $Flight;
	public $TeamNumber;
	public $Date;
}
class Match {
	public $TournamentKey;
	public $Round;
	public $MatchNumber;
	public $Name1;
	public $Name2;
}

function ShowResultsHeader($connection, $tournament, $result, $script_folder_href)
{
	$year = date ( 'Y', strtotime ( $tournament->StartDate ) );
	$friendlyDate = GetFriendlyTournamentDates($tournament);
	
	echo '<h2 style="text-align:center">' . $tournament->Name . '</h2>' . PHP_EOL;
	echo '<h4 style="text-align:center">' . $friendlyDate . ', ' . $year . '</h4>' . PHP_EOL;
	echo '<h3 style="text-align:center"><i>' . $result . '</i></h3><br>' . PHP_EOL;
	
	echo '<table style="border:none;margin-left:auto;margin-right:auto"><tbody>' . PHP_EOL;
	echo '<tr style="background-color:white;">';
	ShowTournamentResultsLinks($connection, $tournament, 'style="border:none"', $result, $script_folder_href);
	echo '</tr>' . PHP_EOL;
	echo '</tbody></table>' . PHP_EOL;
}
function CreateTournamentDetails($connection, $tournamentKey){
	$sqlCmd = "SELECT * FROM `TournamentDetails` WHERE `TournamentKey` = ?";
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
	
	$found = false;
	while ( $query->fetch () ) {
		$found = true;
	}
	
	$query->close();
	
	// Create the record if it does not exist
	if(!$found){
		$sqlCmd = "INSERT INTO `TournamentDetails` VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
		$insert = $connection->prepare ( $sqlCmd );
		
		if (! $insert) {
			die (CreateTournamentDetails . ": " . $sqlCmd . " prepare failed: " . $connection->error );
		}
		
		$emptyDate = TournamentDetails::EMPTYDATE;
		$unofficial = false;
		$emptyFile = '';
		$nullString = null;
		if (! $insert->bind_param ( 'isssssissss', $tournamentKey, $emptyDate, $emptyDate, $emptyDate, $emptyDate, $emptyDate, $unofficial, $emptyFile, $emptyFile, $emptyFile, $nullString )) {
			die (CreateTournamentDetails . ": " .  $sqlCmd . " bind_param failed: " . $connection->error );
		}
		
		if (! $insert->execute ()) {
			die (CreateTournamentDetails . ": " .  $sqlCmd . " execute failed: " . $connection->error );
		}
		
		$insert->close();
	}
}

function UpdateTournamentResultsField($connection, $tournamentKey, $field, $value, $paramType){
	
	// Create the record if it does not exist
	CreateTournamentDetails($connection, $tournamentKey);
	
	$sqlCmd = "UPDATE `TournamentDetails` SET `" . $field . "`= ? WHERE `TournamentKey` = ?";
	$update = $connection->prepare ( $sqlCmd );
		
	if (! $update) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
		
	if (! $update->bind_param ( $paramType . 'i',  $value, $tournamentKey)) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}
		
	if (! $update->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
	$update->close ();
}

function GetClosestToThePin($connection, $tournamentKey, $date){
	$sqlCmd = "SELECT * FROM `ClosestToThePin` WHERE `TournamentKey` = ? AND `Date` = ? ORDER BY `Hole` ASC";
	$query = $connection->prepare ( $sqlCmd );
	
	if (! $query) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	if (! $query->bind_param ( 'is', $tournamentKey, $date )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}
	
	if (! $query->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
	
	$query->bind_result ( $key, $ctpDate, $ghin, $name, $hole, $distance, $prize, $business);
	
	$ctpArray = Array();
	while ( $query->fetch () ) {
		$ctp = new ClosestToThePin();
		$ctp->TournamentKey = $tournamentKey;
		$ctp->Date = $date;
		$ctp->GHIN = $ghin;
		$ctp->Name = $name;
		$ctp->Hole = $hole;
		$ctp->Distance = $distance;
		$ctp->Prize = $prize;
		$ctp->Business = $business;
		$ctpArray[] = $ctp;
	}
	
	$query->close();
	
	return $ctpArray;
}

function ClearResults($connection, $tournamentKey, $result)
{
	$emptyDate = TournamentDetails::EMPTYDATE;
	
	$td = GetTournamentDetails ( $connection, $tournamentKey );
	if (isset ( $td )) {
	
		// Clear the fields.
		$file = null;
		switch ($result) {
			case 'Scores' :
			case 'MatchPlayResults' :
				$file = $td->ScoresFile;
				UpdateTournamentDetails ( $connection, $tournamentKey, 'ScoresFile', '' );
				UpdateTournamentDetails ( $connection, $tournamentKey, 'ScoresPostedDate', $emptyDate );
				break;
			case 'Chits' :
				$file = $td->ChitsFile;
				UpdateTournamentDetails ( $connection, $tournamentKey, 'ChitsFile', '' );
				UpdateTournamentDetails ( $connection, $tournamentKey, 'ChitsPostedDate', $emptyDate );
				break;
			case 'Pool' :
				$file = $td->PoolFile;
				UpdateTournamentDetails ( $connection, $tournamentKey, 'PoolFile', '' );
				UpdateTournamentDetails ( $connection, $tournamentKey, 'PoolPostedDate', $emptyDate );
				break;
			case 'GolfGeniusResultsLink':
				UpdateTournamentDetails ( $connection, $tournamentKey, 'GolfGeniusResultsLink', NULL);
				return; // no other data to delete
				break;
			default :
				die ( 'Unknown result type: ' . $result );
		}
	}
	
	// Delete the data
	ClearTableWithTournamentKey ( $connection, $result, $tournamentKey );
	
	// And remove the results html file too
	if (isset ( $file ) && !empty($file)) {
		$file = "results/" . $file;
		if (file_exists ( $file )) {
			unlink ( $file );
		}
	}
}

function AddPoolResults($connection, $tournamentKey, $poolResults)
{
	for($i = 0; $i < count ( $poolResults ); ++ $i) {
		$sqlCmd = "INSERT INTO `Pool` VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
		$insert = $connection->prepare ( $sqlCmd );
		
		if (! $insert) {
			die ( $sqlCmd . " prepare failed: " . $connection->error );
		}
		if (! $insert->bind_param ( 'isiiiiiisi', $poolResults[$i]->TournamentKey, $poolResults[$i]->Name, $poolResults[$i]->GHIN, $poolResults[$i]->Score, $poolResults[$i]->Winnings, $poolResults[$i]->Flight, $poolResults[$i]->Place, $poolResults[$i]->TeamNumber, $poolResults[$i]->Date, $poolResults[$i]->Hole )) 
		{
			die ( $sqlCmd . " bind_param failed: " . $connection->error );
		}
		
		if (! $insert->execute ()) {
			die ( $sqlCmd . " execute failed: " . $connection->error );
		}
		
		$insert->close ();
	}
}
function GetPoolResults($connection, $tournamentKey)
{
	$sqlCmd = "SELECT * FROM `Pool` WHERE `TournamentKey` = ? ORDER BY `Flight` ASC, `Date` ASC, `Place` ASC, `Hole` ASC, `Winnings` DESC, `TeamNumber` ASC";
	$query = $connection->prepare ( $sqlCmd );
	
	if (! $query) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	if (! $query->bind_param ( 'i', $tournamentKey)) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}
	
	if (! $query->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
	
	$query->bind_result ( $key, $name, $GHIN, $score, $winnings, $flight, $place, $teamNumber, $date, $hole);
	
	$poolArray = Array();
	while ( $query->fetch () ) {
		$pool = new Pool();
		$pool->TournamentKey = $tournamentKey;
		$pool->Name = $name;
		$pool->GHIN = $GHIN;
		$pool->Score = $score;
		$pool->Winnings = $winnings;
		$pool->Flight = $flight;
		$pool->Place = $place;
		$pool->TeamNumber = $teamNumber;
		$pool->Date = $date;
		$pool->Hole = $hole;

		$poolArray[] = $pool;
	}
	
	$query->close();
	
	return $poolArray;
}

function AddChitsResults($connection, $tournamentKey, $chitsResults)
{
	for($i = 0; $i < count ( $chitsResults ); ++ $i) {
		$sqlCmd = "INSERT INTO `Chits` VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
		$insert = $connection->prepare ( $sqlCmd );

		if (! $insert) {
			die ( $sqlCmd . " prepare failed: " . $connection->error );
		}
		if (! $insert->bind_param ( 'isiiiiiiss', $chitsResults[$i]->TournamentKey, $chitsResults[$i]->Name, $chitsResults[$i]->GHIN, $chitsResults[$i]->Score, $chitsResults[$i]->Winnings, $chitsResults[$i]->Flight, $chitsResults[$i]->Place, $chitsResults[$i]->TeamNumber, $chitsResults[$i]->Date, $chitsResults[$i]->FlightName ))
		{
			die ( $sqlCmd . " bind_param failed: " . $connection->error );
		}

		if (! $insert->execute ()) {
			die ( $sqlCmd . " execute failed: " . $connection->error );
		}

		$insert->close ();
	}
}
function GetChitsResults($connection, $tournamentKey)
{
	$sqlCmd = "SELECT * FROM `Chits` WHERE `TournamentKey` = ? ORDER BY `Flight` ASC, `Date` ASC, `Place` ASC, `TeamNumber` ASC";
	$query = $connection->prepare ( $sqlCmd );

	if (! $query) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}

	if (! $query->bind_param ( 'i', $tournamentKey)) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}

	if (! $query->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}

	$query->bind_result ( $key, $name, $GHIN, $score, $winnings, $flight, $place, $teamNumber, $date, $flightName);

	$chitsArray = Array();
	while ( $query->fetch () ) {
		$chits = new Chits();
		$chits->TournamentKey = $tournamentKey;
		$chits->Name = $name;
		$chits->GHIN = $GHIN;
		$chits->Score = $score;
		$chits->Winnings = $winnings;
		$chits->Flight = $flight;
		$chits->Place = $place;
		$chits->TeamNumber = $teamNumber;
		$chits->Date = $date;
		$chits->FlightName = $flightName;

		$chitsArray[] = $chits;
	}

	$query->close();

	return $chitsArray;
}
function GetChitsResultsByNameOrGhin($connection, $name, $ghin, $badkey1, $badkey2)
{
	// If GHIN provided, find all with that number. Ignore the name.
	if($ghin !== 0){
		$sqlCmd = "SELECT * FROM `Chits` WHERE `GHIN` = ?  ORDER BY `Date` ASC";
		$query = $connection->prepare ( $sqlCmd );
	
		if (! $query) {
			die ( $sqlCmd . " prepare failed: " . $connection->error );
		}
	
		if (! $query->bind_param ( 'i', $ghin)) {
			die ( $sqlCmd . " bind_param failed: " . $connection->error );
		}
	}
	else {
		// If GHIN is not provided, find all with that name and with GHIN 0
		$sqlCmd = "SELECT * FROM `Chits` WHERE `Name` = ? and `GHIN` = 0  ORDER BY `Date` ASC";
		$query = $connection->prepare ( $sqlCmd );
	
		if (! $query) {
			die ( $sqlCmd . " prepare failed: " . $connection->error );
		}
	
		if (! $query->bind_param ( 's', $name)) {
			die ( $sqlCmd . " bind_param failed: " . $connection->error );
		}
	}
	
	if (! $query->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}

	$query->bind_result ( $key, $name, $GHIN, $score, $winnings, $flight, $place, $teamNumber, $date, $flightName);

	$chitsArray = Array();
	while ( $query->fetch () ) {
		// Skip over tournaments with bad data
		if(($key != $badkey1) && ($key != $badkey2)){
			$chits = new Chits();
			$chits->TournamentKey = $key;
			$chits->Name = $name;
			$chits->GHIN = $GHIN;
			$chits->Score = $score;
			$chits->Winnings = $winnings;
			$chits->Flight = $flight;
			$chits->Place = $place;
			$chits->TeamNumber = $teamNumber;
			$chits->Date = $date;
			$chits->FlightName = $flightName;

			$chitsArray[] = $chits;
		}
	}

	$query->close();

	return $chitsArray;
}


function AddScoresResults($connection, $tournamentKey, $scoresResults)
{
	for($i = 0; $i < count ( $scoresResults ); ++ $i) {
		$sqlCmd = "INSERT INTO `Scores` VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
		$insert = $connection->prepare ( $sqlCmd );

		if (! $insert) {
			die ( $sqlCmd . " prepare failed: " . $connection->error );
		}
		if (! $insert->bind_param ( 'isisisisiiiiiis', 
				$scoresResults[$i]->TournamentKey, 
				$scoresResults[$i]->Name1, 
				$scoresResults[$i]->GHIN1,
				$scoresResults[$i]->Name2,
				$scoresResults[$i]->GHIN2,
				$scoresResults[$i]->Name3,
				$scoresResults[$i]->GHIN3,
				$scoresResults[$i]->Name4,
				$scoresResults[$i]->GHIN4,
				$scoresResults[$i]->ScoreRound1, 
				$scoresResults[$i]->ScoreRound2,
				$scoresResults[$i]->ScoreTotal,
				$scoresResults[$i]->Flight, 
				$scoresResults[$i]->TeamNumber, 
				$scoresResults[$i]->Date ))
		{
			die ( $sqlCmd . " bind_param failed: " . $connection->error );
		}

		if (! $insert->execute ()) {
			die ( $sqlCmd . " execute failed: " . $connection->error );
		}

		$insert->close ();
	}
}
function GetScoresResults($connection, $tournamentKey, $stableford)
{
	if($stableford == 0){
		$sqlCmd = "SELECT * FROM `Scores` WHERE `TournamentKey` = ? ORDER BY `Flight` ASC, `ScoreTotal` ASC";
	}
	else {
		$sqlCmd = "SELECT * FROM `Scores` WHERE `TournamentKey` = ? ORDER BY `Flight` ASC, `ScoreTotal` DESC";
	}
	$query = $connection->prepare ( $sqlCmd );

	if (! $query) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}

	if (! $query->bind_param ( 'i', $tournamentKey)) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}

	if (! $query->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}

	$query->bind_result ( $key, $name1, $GHIN1, $name2, $GHIN2 , $name3, $GHIN3, $name4, $GHIN4, $scoreRound1, $scoreRound2, $scoreTotal, $flight, $teamNumber, $date);

	$scoresArray = Array();
	while ( $query->fetch () ) {
		$scores = new Scores();
		$scores->TournamentKey = $tournamentKey;
		$scores->Name1 = $name1;
		$scores->GHIN1 = $GHIN1;
		$scores->Name2 = $name2;
		$scores->GHIN2 = $GHIN2;
		$scores->Name3 = $name3;
		$scores->GHIN3 = $GHIN3;
		$scores->Name4 = $name4;
		$scores->GHIN4 = $GHIN4;
		$scores->ScoreRound1 = $scoreRound1;
		$scores->ScoreRound2 = $scoreRound2;
		$scores->ScoreTotal = $scoreTotal;
		$scores->Flight = $flight;
		$scores->TeamNumber = $teamNumber;
		$scores->Date = $date;

		$scoresArray[] = $scores;
	}

	$query->close();

	return $scoresArray;
}

function GetScoresResultsByName($connection, $name, $badkey1, $badkey2)
{

	$sqlCmd = "SELECT * FROM `Scores` WHERE `Name1` = ? OR `Name2` = ? OR `Name3` = ? OR `Name4` = ? ORDER BY `Date` ASC";

	$query = $connection->prepare ( $sqlCmd );

	if (! $query) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}

	if (! $query->bind_param ( 'ssss', $name, $name, $name, $name)) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}

	if (! $query->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}

	$query->bind_result ( $key, $name1, $GHIN1, $name2, $GHIN2 , $name3, $GHIN3, $name4, $GHIN4, $scoreRound1, $scoreRound2, $scoreTotal, $flight, $teamNumber, $date);

	$scoresArray = Array();
	while ( $query->fetch () ) {
		if(($key != $badkey1) && ($key != $badkey2)){
			$scores = new Scores();
			$scores->TournamentKey = $key;
			$scores->Name1 = $name1;
			$scores->GHIN1 = $GHIN1;
			$scores->Name2 = $name2;
			$scores->GHIN2 = $GHIN2;
			$scores->Name3 = $name3;
			$scores->GHIN3 = $GHIN3;
			$scores->Name4 = $name4;
			$scores->GHIN4 = $GHIN4;
			$scores->ScoreRound1 = $scoreRound1;
			$scores->ScoreRound2 = $scoreRound2;
			$scores->ScoreTotal = $scoreTotal;
			$scores->Flight = $flight;
			$scores->TeamNumber = $teamNumber;
			$scores->Date = $date;

			$scoresArray[] = $scores;
		}
	}

	$query->close();

	return $scoresArray;
}

function AddMatchPlayScoresResults($connection, $tournamentKey, $scoresResults)
{
	for($i = 0; $i < count ( $scoresResults ); ++ $i) {
		$sqlCmd = "INSERT INTO `MatchPlayResults` VALUES (?, ?, ?, ?, ?)";
		$insert = $connection->prepare ( $sqlCmd );

		if (! $insert) {
			die ( $sqlCmd . " prepare failed: " . $connection->error );
		}
		if (! $insert->bind_param ( 'iiiss',
				$scoresResults[$i]->TournamentKey,
				$scoresResults[$i]->Round,
				$scoresResults[$i]->MatchNumber,
				$scoresResults[$i]->Name1,
				$scoresResults[$i]->Name2 ))
		{
			die ( $sqlCmd . " bind_param failed: " . $connection->error );
		}

		if (! $insert->execute ()) {
			die ( $sqlCmd . " execute failed: " . $connection->error );
		}

		$insert->close ();
	}
}
function ShowMatchResults($connection, $tournamentKey)
{
	$dataProvided = false;
	$matchString = '<svg height="420" width="850">' . PHP_EOL;
	
	for($round = 1; $round <= 4; ++$round){
		$sqlCmd = "SELECT * FROM `MatchPlayResults` WHERE `TournamentKey` = ? AND `Round` = ? ORDER BY `MatchNumber` ASC";
		$query = $connection->prepare ( $sqlCmd );
		
		if (! $query) {
			die ( $sqlCmd . " prepare failed: " . $connection->error );
		}
		
		if (! $query->bind_param ( 'ii', $tournamentKey, $round)) {
			die ( $sqlCmd . " bind_param failed: " . $connection->error );
		}
		
		if (! $query->execute ()) {
			die ( $sqlCmd . " execute failed: " . $connection->error );
		}
		
		$query->bind_result ( $key, $roundNumber, $matchNumber, $name1, $name2);
		
		switch($round){
			case 1: 
				$matchCount = 4; 
				$x1 = 20;
				$x2 = 220;
				$y1 = 50;
				$y2 = 100;
				$yIncrement = 100;
				break;
			case 2: 
				$matchCount = 2; 
				$x1 = 220;
				$x2 = 420;
				$y1 = 75;
				$y2 = 175;
				$yIncrement = 200;
				break;
			case 3: 
				$matchCount = 1; 
				$x1 = 420;
				$x2 = 620;
				$y1 = 125;
				$y2 = 325;
				$yIncrement = 0;
				break;
		}
		
		if($round == 4){
			if ( $query->fetch () ) {
				$matchString = $matchString . '<line x1="620" y1="225" x2="820" y2="225" style="stroke:rgb(0,0,0);stroke-width:2" />' . PHP_EOL;
				$matchString = $matchString . '<text x="630" y="215">' . $name1 . '</text>' . PHP_EOL;
			}
		} else {
			for($currentMatch = 0; $currentMatch < $matchCount; ++$currentMatch){
				
				$y1Adjusted = $y1 + ($currentMatch * $yIncrement);
				$y2Adjusted = $y2 + ($currentMatch * $yIncrement);
				
				$matchString = $matchString . '<polyline points="' . $x1 . ',' . $y1Adjusted . ' ' . $x2 . ',' . $y1Adjusted . ' ' . $x2 . ',' . $y2Adjusted . ' ' . $x1 . ',' . $y2Adjusted . '" style="fill:none;stroke:black;stroke-width:2" />' . PHP_EOL;
				if ( $query->fetch () ) {
					$dataProvided = true;
					$matchString = $matchString . '<text x="' . ($x1 + 10) . '" y="' . ($y1Adjusted - 10) . '">' . $name1 . '</text>' . PHP_EOL;
					$matchString = $matchString . '<text x="' . ($x1 + 10) . '" y="' . ($y2Adjusted - 10) . '">' . $name2 . '</text>' . PHP_EOL;
				}
				else {
					if($round == 1){
						$matchString = $matchString . '<text x="' . ($x1 + 10) . '" y="' . ($y1Adjusted - 10) . '">TBD</text>' . PHP_EOL;
						$matchString = $matchString . '<text x="' . ($x1 + 10) . '" y="' . ($y2Adjusted - 10) . '">TBD</text>' . PHP_EOL;
					}
				}
			}
		}
		
		$query->close();
	}
	$matchString = $matchString . '</svg>' . PHP_EOL;
	if($dataProvided){
		echo $matchString;
	}
}

function UpdateFlightNames($connection, $tournamentKey, $flightNames)
{
	for($i = 0; $i < count ( $flightNames ); ++ $i) {
		if(!empty($flightNames[$i])){
			$sqlCmd = "INSERT INTO `FlightNames` VALUES (?, ?, ?)";
			$insert = $connection->prepare ( $sqlCmd );

			if (! $insert) {
				die ( $sqlCmd . " prepare failed: " . $connection->error );
			}
			if (! $insert->bind_param ( 'iis', $tournamentKey, $i, $flightNames[$i] ))
			{
				die ( $sqlCmd . " bind_param failed: " . $connection->error );
			}

			if (! $insert->execute ()) {
				die ( $sqlCmd . " execute failed: " . $connection->error );
			}
		

			$insert->close ();
		}
	}
}

// Returns a 10 element array (flight numbers 0-9) with flight names
function GetFlightNames($connection, $tournamentKey)
{
	$sqlCmd = "SELECT * FROM `FlightNames` WHERE `TournamentKey` = ? ORDER BY `Number` ASC";
	$query = $connection->prepare ( $sqlCmd );

	if (! $query) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}

	if (! $query->bind_param ( 'i', $tournamentKey)) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}

	if (! $query->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}

	$query->bind_result ( $key, $number, $name);

	// Create flightNames array with 10 empty slots
	$flightNames = array_fill(0, 10, "");

	while ( $query->fetch () ) {
		if($number < count($flightNames)){
			$flightNames[$number] = $name;
		}
	}

	$query->close();

	return $flightNames;
}

?>