<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/results_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$overrideTitle = "Closest To The Pin";
get_header ();

echo ' <div id="content-container" class="entry-content">';
echo '    <div id="content" role="main">';

$connection = new mysqli ('p:' . $db_hostname, $db_username, $db_password, $db_database );

$tournamentKey = $_GET ['tournament'];
if ($tournamentKey) {
	
	$t = GetTournament ( $connection, $tournamentKey );
	
	if (isset($t)) {
		
		ShowResultsHeader($connection, $t, 'Closest-To-The-Pin', '');
		
		echo '<table style="border:none;margin-left:auto;margin-right:auto"><tbody>' . PHP_EOL;
		$ctpArray = GetClosestToThePin ( $connection, $tournamentKey, $t->StartDate );
		
		echo '<tr><td  style="border:none;">' . PHP_EOL;
		ShowClosestToThePin ( $t->StartDate, $ctpArray );
		echo '</td></tr>' . PHP_EOL;
		
		if (strcmp ( $t->StartDate, $t->EndDate ) != 0) {
			$ctpArray = GetClosestToThePin ( $connection, $tournamentKey, $t->EndDate );
			
			echo '<tr><td  style="border:none;">' . PHP_EOL;
			ShowClosestToThePin ( $t->EndDate, $ctpArray );
			echo '</td></tr>' . PHP_EOL;
		}
		echo '</tbody></table>' . PHP_EOL;
	}
	else {
		echo "Invalid tournament: " . $tournamentKey;
	}
	
	echo '    </div><!-- #content -->';
	echo ' </div><!-- #content-container -->';
	
	$connection->close ();
}

function ShowClosestToThePin($date, $ctpArray) {
	echo '<strong>' . date ( 'l', strtotime ( $date ) ) . '</strong><br>' . PHP_EOL;
	
	echo '<table style="border:none;">' . PHP_EOL;
	echo '<thead><tr class="header">';
	echo '<th style="width:50px">Hole</th>';
	echo '<th style="width:200px">Name</th>';
	echo '<th style="width:150px">Distance</th>';
	echo '<th style="width:250px">Prize</th>';
	echo '</tr></thead>' . PHP_EOL;
	echo '<tbody>' . PHP_EOL;
	
	for($i = 0; $i < count ( $ctpArray ); ++ $i) {
		echo '<tr>';
		echo '<td>' . $ctpArray [$i]->Hole . '</td>';
		echo '<td>' . $ctpArray [$i]->Name . '</td>';
		echo '<td>' . $ctpArray [$i]->Distance . '</td>';
		echo '<td>' . $ctpArray [$i]->Prize . ' ' . $ctpArray [$i]->Business . '</td>';
		echo '</tr>' . PHP_EOL;
	}
	
	echo '</tbody></table>' . PHP_EOL;
}

get_sidebar ();
get_footer ();
?>