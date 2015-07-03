<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$year = $_GET ['year'];
if (! $year) {
	$year = date ( "Y" );
}

$overrideTitle = $year . " Tournament Schedule";
get_header ();

echo ' <div id="content-container" class="entry-content">';
echo '    <div id="content" role="main">';

$connection = new mysqli ('p:' . $db_hostname, $db_username, $db_password, $db_database );

$tournaments = GetTournaments ( $connection, $year );

echo '<h2 style="text-align:center">' . $year . ' Tournament Schedule</h2><br>';

echo '<table style="border:none;margin-left:auto;margin-right:auto">' . PHP_EOL;
echo '<thead><tr class="header">';
echo '<th>Date</th>';
echo '<th>Name</th>';
echo '<th>Description</th>';
echo '<th>Scores</th>';
echo '<th>Chits</th>';
echo '<th>Pool</th>';
echo '<th>Closest to Pin</th>';
echo '</tr></thead>' . PHP_EOL;
echo '<tbody>' . PHP_EOL;

for($i = 0; $i < count ( $tournaments ); ++ $i) {
	if (($i % 2) == 0) {
		echo '<tr class="d1">';
	} else {
		echo '<tr class="d0">';
	}
	
	ShowTournamentResults($connection, $tournaments [$i], '', false, '');

	echo '</tr>' . PHP_EOL;
}
echo '</tbody></table>' . PHP_EOL;

echo '    </div><!-- #content -->';
echo ' </div><!-- #content-container -->';

$connection->close ();

get_sidebar ();
get_footer ();
?>