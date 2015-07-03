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
			echo '<iframe src="http://' . $web_site . '/results/' . $td->ScoresFile . '" name="resultsframe" width="80%" height="500" style="border: none;display: block;margin-left:auto;margin-right:auto" />' . PHP_EOL;
			/*
			 * echo '<embed type="application/pdf" src="http://' . $web_site . '/pdf/' . $td->ScoresFile . '" width="100%" height="1275">'; echo '<iframe src="http://docs.google.com/gview?url=http://' . $web_site . '/pdf/' . $td->ScoresFile . '&embedded=true" style="width:100%; height:1300px;" frameborder="0"></iframe>';
			 
			$ua = strtolower ( $_SERVER ['HTTP_USER_AGENT'] );
			if (stripos ( $ua, 'android' ) !== false) {		
				echo '    <div id="content2" style="background-color: #BBB;">' . PHP_EOL;
				echo '<iframe src="http://docs.google.com/viewer?url=http%3A%2F%2F' . $web_site . '%2Fpdf%2F' . urlencode ( $td->ScoresFile ) . '&embedded=true" width="80%" height="500" style="border: none;display: block;margin-left:auto;margin-right:auto"></iframe>' . PHP_EOL;
				echo '    </div><!-- #content2 -->';
			} else {
				echo '<embed type="application/pdf" src="http://' . $web_site . '/pdf/' . $td->ScoresFile . '" width="990" height="1275">';
			}
			*/
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