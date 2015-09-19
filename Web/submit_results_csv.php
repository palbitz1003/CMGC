<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tee times functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/results_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$connection = new mysqli ( $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( $connection->connect_error );
	
	// var_dump($_POST);
	// echo '<br>';

login ( $_POST ['Login'], $_POST ['Password'] );

if (! isset ( $_POST ['TournamentKey'] )) {
	die ( "Missing tournament key" );
}

$tournamentKey = $_POST ['TournamentKey'];

$t = GetTournament ( $connection, $tournamentKey );
if (! isset ( $t )) {
	die ( 'This is not a valid tournament key: ' . $tournamentKey );
}

if ($_POST ['Action'] == 'Submit') {
	
	// First, clear any tables requested
	if(strcasecmp($_POST ["Clear"], 'scores') == 0){
		ClearResults($connection, $tournamentKey, 'Scores');
	}
	else if(strcasecmp($_POST ["Clear"], 'match play scores') == 0){
		ClearResults($connection, $tournamentKey, 'MatchPlayResults');
	}
	else if(strcasecmp($_POST ["Clear"], 'chits') == 0){
		ClearResults($connection, $tournamentKey, 'Chits');
	}
	else if(strcasecmp($_POST ["Clear"], 'pool') == 0){
		ClearResults($connection, $tournamentKey, 'Pool');
	}
		
	if (isset ( $_POST ['ResultsPool'] )) {
		
		$poolResults = Array();
		for($i = 0; $i < count ( $_POST ['ResultsPool'] ); ++ $i) {
			$p = new Pool();
			$p->TournamentKey = $tournamentKey;
			$p->Date = $_POST ['ResultsPool'] [$i] ['Date'];
			$p->Flight = $_POST ['ResultsPool'] [$i] ['Flight'];
			$p->GHIN = $_POST ['ResultsPool'] [$i] ['GHIN'];
			$p->Name = $_POST ['ResultsPool'] [$i] ['Name'];
			if(isset($_POST ['ResultsPool'] [$i] ['Place'])){
				$p->Place = $_POST ['ResultsPool'] [$i] ['Place'];
			}
			else {
				$p->Place = 0;
			}
			$p->Score = $_POST ['ResultsPool'] [$i] ['Score'];
			$p->TeamNumber = $_POST ['ResultsPool'] [$i] ['TeamNumber'];
			$p->Winnings = $_POST ['ResultsPool'] [$i] ['Winnings'];
			if(isset($_POST ['ResultsPool'] [$i] ['Hole'])){
				$p->Hole = $_POST ['ResultsPool'] [$i] ['Hole'];
			}
			else {
				$p->Hole = 0;
			}
			
			$poolResults[] = $p;
		}
		
		AddPoolResults($connection, $tournamentKey, $poolResults);
		UpdateTournamentDetails ( $connection, $tournamentKey, 'PoolPostedDate', date ( 'Y-m-d' ) );
	}
	else if (isset ( $_POST ['ResultsChits'] )) {
		
		$chitsResults = Array();
		for($i = 0; $i < count ( $_POST ['ResultsChits'] ); ++ $i) {
			$c = new Chits();
			$c->TournamentKey = $tournamentKey;
			$c->Date = $_POST ['ResultsChits'] [$i] ['Date'];
			$c->Flight = $_POST ['ResultsChits'] [$i] ['Flight'];
			$c->GHIN = $_POST ['ResultsChits'] [$i] ['GHIN'];
			$c->Name = $_POST ['ResultsChits'] [$i] ['Name'];
			$c->Place = $_POST ['ResultsChits'] [$i] ['Place'];
			$c->Score = $_POST ['ResultsChits'] [$i] ['Score'];
			$c->TeamNumber = $_POST ['ResultsChits'] [$i] ['TeamNumber'];
			$c->Winnings = $_POST ['ResultsChits'] [$i] ['Winnings'];
			$c->FlightName = $_POST ['ResultsChits'] [$i] ['FlightName'];
			
			$chitsResults[] = $c;
		}
		
		AddChitsResults($connection, $tournamentKey, $chitsResults);
		UpdateTournamentDetails ( $connection, $tournamentKey, 'ChitsPostedDate', date ( 'Y-m-d' ) );
	}
	else if (isset ( $_POST ['ResultsScores'] )) {
	
		$ScoresResults = Array();
		for($i = 0; $i < count ( $_POST ['ResultsScores'] ); ++ $i) {
			$c = new Scores();
			$c->TournamentKey = $tournamentKey;
			$c->Date = $_POST ['ResultsScores'] [$i] ['Date'];
			$c->Flight = $_POST ['ResultsScores'] [$i] ['Flight'];
			$c->GHIN1 = $_POST ['ResultsScores'] [$i] ['GHIN1'];
			$c->Name1 = $_POST ['ResultsScores'] [$i] ['Name1'];
			$c->GHIN2 = $_POST ['ResultsScores'] [$i] ['GHIN2'];
			$c->Name2 = $_POST ['ResultsScores'] [$i] ['Name2'];
			$c->GHIN3 = $_POST ['ResultsScores'] [$i] ['GHIN3'];
			$c->Name3 = $_POST ['ResultsScores'] [$i] ['Name3'];
			$c->GHIN4 = $_POST ['ResultsScores'] [$i] ['GHIN4'];
			$c->Name4 = $_POST ['ResultsScores'] [$i] ['Name4'];
			$c->ScoreRound1 = $_POST ['ResultsScores'] [$i] ['ScoreRound1'];
			$c->ScoreRound2 = $_POST ['ResultsScores'] [$i] ['ScoreRound2'];
			$c->ScoreTotal = $_POST ['ResultsScores'] [$i] ['ScoreTotal'];
			$c->TeamNumber = $_POST ['ResultsScores'] [$i] ['TeamNumber'];
				
			$ScoresResults[] = $c;
		}
	
		AddScoresResults($connection, $tournamentKey, $ScoresResults);
		UpdateTournamentDetails ( $connection, $tournamentKey, 'ScoresPostedDate', date ( 'Y-m-d' ) );
	} else if(isset ( $_POST ['MatchPlayResultsScores'] )) {
	
		$ScoresResults = Array();
		for($i = 0; $i < count ( $_POST ['MatchPlayResultsScores'] ); ++ $i) {
			$c = new Match();
			$c->TournamentKey = $tournamentKey;
			$c->Round = $_POST ['MatchPlayResultsScores'] [$i] ['Round'];
			$c->MatchNumber = $_POST ['MatchPlayResultsScores'] [$i] ['MatchNumber'];
			$c->Name1 = $_POST ['MatchPlayResultsScores'] [$i] ['Player1'];
			$c->Name2 = $_POST ['MatchPlayResultsScores'] [$i] ['Player2'];
			
			$ScoresResults[] = $c;
		}
		
		AddMatchPlayScoresResults($connection, $tournamentKey, $ScoresResults);
		UpdateTournamentDetails ( $connection, $tournamentKey, 'ScoresPostedDate', date ( 'Y-m-d' ) );
	}
} else if ($_POST ['Action'] == 'Clear') {
	
	if(strcasecmp($_POST ["Result"], 'scores') == 0){
		ClearResults($connection, $tournamentKey, 'Scores');
	} else if(strcasecmp($_POST ["Result"], 'match play scores') == 0){
		ClearResults($connection, $tournamentKey, 'MatchPlayResults');
	}
	else if(strcasecmp($_POST ["Result"], 'chits') == 0){
		ClearResults($connection, $tournamentKey, 'Chits');
	} 
	else if(strcasecmp($_POST ["Result"], 'pool') == 0){
		ClearResults($connection, $tournamentKey, 'Pool');
	}
	else {
		die('Expected to clear scores, match play scores, chits, or pool, got: ' . $_POST ["Result"]);
	}
	
} else {
	die ( 'Unknown action requested: ' . $_POST ['Action'] );
}

echo "Success";

$connection->close ();
?>