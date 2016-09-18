<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/results_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$connection = new mysqli ( $db_hostname, $db_username, $db_password, $db_database );

get_header ();

echo ' <div id="content-container" class="entry-content">';
echo '    <div id="content" role="main">' . PHP_EOL;

ShowRecentlyCompletedTournaments ( $connection , '');

$tournamentKey = $_GET ['tournament'];
if ($tournamentKey) {
	
	$t = GetTournament ( $connection, $tournamentKey );
	
	if (isset ( $t )) {
		$td = GetTournamentDetails ( $connection, $tournamentKey );
		
		if (! empty ( $td->ScoresFile )) {
			echo '<iframe src="https://' . $web_site . '/results/' . $td->ScoresFile . '" name="resultsframe" width="80%" height="500" style="border: none;display: block;margin-left:auto;margin-right:auto" />' . PHP_EOL;
		} else {
			die ( 'No scores for tournament key ' . $tournamentKey );
		}
	} else {
		echo "Invalid tournament: " . $tournamentKey;
	}
	
	echo '    </div><!-- #content -->';
	echo ' </div><!-- #content-container -->';
}

get_footer ();
?>