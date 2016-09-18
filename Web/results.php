<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/results_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$connection = new mysqli ('p:' . $db_hostname, $db_username, $db_password, $db_database );

$result = $_GET ['result'];
switch ($result) {
	case 'scores' :
		$overrideTitle = "Scores";
		break;
	case 'chits' :
		$overrideTitle = "Chits";
		break;
	case 'pool' :
		$overrideTitle = "Pool";
		break;
	default:
		$overrideTitle = "Results";
		break;
}

get_header ();

echo ' <div id="content-container" class="entry-content">';
echo '    <div id="content" role="main">' . PHP_EOL;

$tournamentKey = $_GET ['tournament'];

if (! isset ( $result )) {
	die ( "The requested result (scores/chits/pool) was not included in the URL" );
}
if ($tournamentKey) {
	$t = GetTournament ( $connection, $tournamentKey );
	
	if (isset ( $t )) {
		
		$td = GetTournamentDetails ( $connection, $tournamentKey );
		
		switch ($result) {
			case 'scores' :	
				ShowResultsHeader($connection, $t, 'Scores', '');

				if(isset($td->ScoresFile) && !empty($td->ScoresFile)){
					ShowFile($web_site, $td->ScoresFile);
				}
				else {
					if($t->MatchPlay){
						ShowMatchResults($connection, $t->TournamentKey);
					} else {
						$scoresResults = GetScoresResults($connection, $tournamentKey, $t->Stableford);
						ShowScoresResults($scoresResults, $t->Stableford, $t->StartDate !== $t->EndDate);
					}
				}
				break;
				case 'chits' :
					ShowResultsHeader($connection, $t, 'Chits', '');
					
					if(isset($td->ChitsFile) && !empty($td->ChitsFile)){
						ShowFile($web_site, $td->ChitsFile);
					}
					else {
						$chitsResults = GetChitsResults($connection, $tournamentKey);
						ShowChitsResults($chitsResults, $t->MatchPlay);
					}
				break;
				case 'pool' :
					ShowResultsHeader($connection, $t, 'Pool', '');
					
					echo '<p style="width:75%;margin-left:auto;margin-right:auto">Note: All players pool payouts are rounded to the nearest $5 for ease of distribution. Uneven amounts paid within a particular <i>Place</i> are determined by a card-off in accordance with the tournament manual.</p>';
					
					if(isset($td->PoolFile) && !empty($td->PoolFile)){
						ShowFile($web_site, $td->PoolFile);
					}
					else 
					{
						$poolResults = GetPoolResults($connection, $tournamentKey);
						ShowPoolResults($poolResults);
					}
				break;
				default:
					die('You requested an unsupported result type: ' . $result);
		}
	} else {
		echo "Invalid tournament: " . $tournamentKey;
	}
	
	echo '    </div><!-- #content -->';
	echo ' </div><!-- #content-container -->';
}

function ShowFile($web_site, $file)
{
	echo '<iframe src="https://' . $web_site . '/results/' . $file . '" name="resultsframe" width="80%" height="1300" style="border: none;display: block;margin-left:auto;margin-right:auto" />' . PHP_EOL;
}


function ShowPoolResults($poolResults)
{
	echo '<table style="border:none;;margin-left:auto;margin-right:auto"><tbody>' . PHP_EOL;

	$day1 = null;
	$day2 = null;

	for($i = 0; $i < count ( $poolResults ) && empty($day2); ++ $i) {
		$day = date ( 'l', strtotime ( $poolResults [$i]->Date) );
		if(empty($day1)){
			$day1 = $day;
		}
		else if($day != $day1)
		{
			$day2 = $day;
		}
	}

	$isSkins = false;
	$currentFlight = -1;
	$currentDate = "";
	for($i = 0; $i < count ( $poolResults ); ++ $i) {

		if ($i == 0) {
			$currentFlight = $poolResults [$i]->Flight;
			echo '<tr>' . PHP_EOL;
			
			if(!empty($day2)){
				echo '<td style="width:50%;border:none;"><b>' . $day1 . '</b></td>' . PHP_EOL;
				echo '<td style="width:50%;border:none;"><b>' . $day2 . '</b></td>' . PHP_EOL;
			}
			else {
				echo '<td style="width:100%;border:none;"><b>' . $day1 . '</b></td>' . PHP_EOL;
			}
			echo '</tr><tr>'. PHP_EOL;
		}

		if (($currentFlight != $poolResults [$i]->Flight) || ($currentDate != $poolResults[$i]->Date)) {
			
			// If the hole is set, then the results are for skins
			$isSkins = ($poolResults [$i]->Hole != 0);
			
			if ($i != 0) {
				// Finish previous table
				echo '</tbody>' . PHP_EOL;
				echo '</table>' . PHP_EOL;
				echo '</td>' . PHP_EOL;
			}
				
			// If the flight changed, start table on new row
			if ($currentFlight != $poolResults [$i]->Flight) {
				echo '</tr><tr>' . PHP_EOL;
			}
				
			echo '<td style="width:50%;border:none;">' . PHP_EOL;
			echo '<table>' . PHP_EOL;
			if($isSkins){
				echo '<thead><tr class="header"><th>Flight ' . $poolResults [$i]->Flight . '</th><th>Hole</th><th>Score</th><th>Won</th></tr></thead>' . PHP_EOL;
			}
			else {
				echo '<thead><tr class="header"><th>Flight ' . $poolResults [$i]->Flight . '</th><th>Score</th><th>Won</th><th>Place</th></tr></thead>' . PHP_EOL;
			}
			echo '<tbody>' . PHP_EOL;
				
			$currentDate = $poolResults[$i]->Date;
			$currentFlight = $poolResults [$i]->Flight;
		}

		if (($i % 2) == 0) {
			echo '<tr class="d1">';
		} else {
			echo '<tr class="d0">';
		}
		echo '<td style="width:200px">' . $poolResults [$i]->Name . '</td>' . PHP_EOL;
		if($isSkins){
			echo '<td style="text-align:center">' . $poolResults [$i]->Hole . '</td>' . PHP_EOL;
			echo '<td style="text-align:center">' . $poolResults [$i]->Score . '</td>' . PHP_EOL;
			echo '<td style="text-align:center">$' . $poolResults [$i]->Winnings . '</td>' . PHP_EOL;
		}
		else {
			echo '<td style="text-align:center">' . $poolResults [$i]->Score . '</td>' . PHP_EOL;
			echo '<td style="text-align:center">$' . $poolResults [$i]->Winnings . '</td>' . PHP_EOL;
			echo '<td style="text-align:center">' . $poolResults [$i]->Place . '</td>' . PHP_EOL;
		}
		
		echo '</tr>' . PHP_EOL;
	}

	// Finish the inner table
	echo '</tbody>' . PHP_EOL;
	echo '</table>' . PHP_EOL;

	// Finish the outer table
	echo '</td>' . PHP_EOL;
	echo '</tr>' . PHP_EOL;
	echo '</tbody></table>' . PHP_EOL;
}

function ShowChitsResults($chitsResults, $matchPlay)
{
	echo '<table style="border:none;;margin-left:auto;margin-right:auto"><tbody>' . PHP_EOL;

	$day1 = null;
	$day2 = null;

	for($i = 0; $i < count ( $chitsResults ) && empty($day2); ++ $i) {
		$day = date ( 'l', strtotime ( $chitsResults [$i]->Date) );
		if(empty($day1)){
			$day1 = $day;
		}
		else if($day != $day1)
		{
			$day2 = $day;
		}
	}

	$currentFlight = -1;
	$currentDate = "";
	for($i = 0; $i < count ( $chitsResults ); ++ $i) {

		if ($i == 0) {
			$currentFlight = $chitsResults [$i]->Flight;
			echo '<tr>' . PHP_EOL;
				
			if(!empty($day2)){
				echo '<td style="width:50%;border:none;"><b>' . $day1 . '</b></td>' . PHP_EOL;
				echo '<td style="width:50%;border:none;"><b>' . $day2 . '</b></td>' . PHP_EOL;
			}
			else {
				// The day ends up being the first day of the tournament, so don't show it
				echo '<td style="width:100%;border:none;"> </td>' . PHP_EOL;
			}
			echo '</tr><tr>'. PHP_EOL;
		}

		if (($currentFlight != $chitsResults[$i]->Flight) || ($currentDate != $chitsResults[$i]->Date)) {
			if ($i != 0) {
				// Finish previous table
				echo '</tbody>' . PHP_EOL;
				echo '</table>' . PHP_EOL;
				echo '</td>' . PHP_EOL;
			}

			// If the flight changed, start table on new row
			if ($currentFlight != $chitsResults [$i]->Flight) {
				echo '</tr><tr>' . PHP_EOL;
			}
			
			if(empty($chitsResults [$i]->FlightName)){
				$flightName = 'Flight ' . $chitsResults [$i]->Flight;
			}
			else {
				$flightName = $chitsResults [$i]->FlightName;
			}

			echo '<td style="width:50%;border:none;">' . PHP_EOL;
			echo '<table>' . PHP_EOL;
			if($matchPlay){
				echo '<thead><tr class="header"><th>' . $flightName . '</th><th>Won</th><th>Place</th></tr></thead>' . PHP_EOL;
			} else {
				echo '<thead><tr class="header"><th>' . $flightName . '</th><th>Score</th><th>Won</th><th>Place</th></tr></thead>' . PHP_EOL;
			}
			echo '<tbody>' . PHP_EOL;

			$currentDate = $chitsResults[$i]->Date;
			$currentFlight = $chitsResults [$i]->Flight;
		}

		if (($i % 2) == 0) {
			echo '<tr class="d1">';
		} else {
			echo '<tr class="d0">';
		}
		echo '<td style="width:200px">' . $chitsResults [$i]->Name . '</td>' . PHP_EOL;
		if(!$matchPlay){
			echo '<td style="text-align:center">' . $chitsResults [$i]->Score . '</td>' . PHP_EOL;
		}
		echo '<td style="text-align:center">$' . $chitsResults [$i]->Winnings . '</td>' . PHP_EOL;
		echo '<td style="text-align:center">' . $chitsResults [$i]->Place . '</td>' . PHP_EOL;
		echo '</tr>' . PHP_EOL;
	}

	// Finish the inner table
	echo '</tbody>' . PHP_EOL;
	echo '</table>' . PHP_EOL;

	// Finish the outer table
	echo '</td>' . PHP_EOL;
	echo '</tr>' . PHP_EOL;
	echo '</tbody></table>' . PHP_EOL;
}

function ShowScoresResults($scoresResults, $stableford, $multiDay) {
	
	$currentFlight = - 1;
	if($stableford == 0){
		$header = '<th>Net Score</th>';
	} else {
		$header = '<th>Stableford Points</th>';
	}
	
	// Check if only day 1 of a 2 day tournament is filled in
	if ($multiDay){
		$round2FilledIn = false;
		for($i = 0; ($i < count ( $scoresResults )) && !$round2FilledIn; ++ $i) {
			if($scoresResults [$i]->ScoreRound2 != 0){
				$round2FilledIn = true;
			}
		}
		
		if(!$round2FilledIn){
			$multiDay = false;
		}
	}
	
	for($i = 0; $i < count ( $scoresResults ); ++ $i) {
		
		if ($currentFlight != $scoresResults [$i]->Flight) {
			if ($i == 0) {
				if ($multiDay) {
					if($stableford == 0){
						$header = '<th>Net Score R1</th><th>Net Score R2</th><th>Net Score Total</th>';
					} else {
						$header = '<th>Stableford R1</th><th>Stableford R2</th><th>Stableford Total</th>';
					}
				}
			}
			if ($i != 0) {
				// Finish previous table
				echo '</tbody>' . PHP_EOL;
				echo '</table>' . PHP_EOL;
			}
			
			echo '<table style="margin-left:auto;margin-right:auto">' . PHP_EOL;
			echo '<thead><tr class="header"><th>Flight ' . $scoresResults [$i]->Flight . '</th>' . $header . '</tr></thead>' . PHP_EOL;
			echo '<tbody>' . PHP_EOL;
			$currentFlight = $scoresResults [$i]->Flight;
		}
		
		if (($i % 2) == 0) {
			echo '<tr class="d1">';
		} else {
			echo '<tr class="d0">';
		}
		echo '<td style="width:200px">' . $scoresResults [$i]->Name1;
		if (! empty ( $scoresResults [$i]->Name2 )) {
			echo '<br>' . $scoresResults [$i]->Name2;
			if (! empty ( $scoresResults [$i]->Name3 )) {
				echo '<br>' . $scoresResults [$i]->Name3;
				if (! empty ( $scoresResults [$i]->Name4 )) {
					echo '<br>' . $scoresResults [$i]->Name4;
				}
			}
		}
		echo '</td>' . PHP_EOL;
		
		if ($multiDay) {
			echo '<td style="text-align:center">' . $scoresResults [$i]->ScoreRound1 . '</td>' . PHP_EOL;
			echo '<td style="text-align:center">' . $scoresResults [$i]->ScoreRound2 . '</td>' . PHP_EOL;
			echo '<td style="text-align:center">' . $scoresResults [$i]->ScoreTotal . '</td>' . PHP_EOL;
		} else {
			echo '<td style="text-align:center">' . $scoresResults [$i]->ScoreTotal . '</td>' . PHP_EOL;
		}
		echo '</tr>' . PHP_EOL;
	}
	
	// Finish the inner table
	echo '</tbody>' . PHP_EOL;
	echo '</table>' . PHP_EOL;
}

get_footer ();
?>