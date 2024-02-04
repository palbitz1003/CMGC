<?php
require_once realpath ( $_SERVER ["DOCUMENT_ROOT"] ) . '/login.php';
require_once realpath ( $_SERVER ["DOCUMENT_ROOT"] ) . $script_folder . '/signup functions.php';
require_once realpath ( $_SERVER ["DOCUMENT_ROOT"] ) . $script_folder . '/tee times functions.php';
require_once realpath ( $_SERVER ["DOCUMENT_ROOT"] ) . $script_folder . '/tournament_functions.php';
require_once realpath ( $_SERVER ["DOCUMENT_ROOT"] ) . $wp_folder . '/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$tournamentKey = $_GET ['tournament'];
if (! $tournamentKey || !is_numeric($tournamentKey)) {
	die ( "Which tournament?" );
}

$overrideTitle = "Signups";
get_header ();

get_sidebar ();

$connection = new mysqli ( 'p:' . $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( $connection->connect_error );

echo ' <div id="content-container" class="entry-content">';
echo '    <div id="content" role="main">';

ShowSignups($connection, $tournamentKey);

// Only show waiting list if this tournament is a 2 day tournament
$tournament = GetTournament($connection, $tournamentKey);

if(!empty($tournament) && ($tournament->StartDate != $tournament->EndDate)){

	// Need the previous 2 day tournament key to get the waitlist
	$previousTournamentKey = GetPrevious2DayTournamentKey($connection, $tournamentKey);

	$waitingList = GetTeeTimeWaitingList($connection, $previousTournamentKey);
	
	if(count($waitingList) != 0){
		// Sigh. I couldn't get a paragraph to center properly without putting it in a table
		echo '<table style="width:500px;border:none;margin-left:auto;margin-right:auto">' . PHP_EOL;
		echo '<tbody>' . PHP_EOL;
		echo '<tr><td style="border:none">' . PHP_EOL;
		echo 'These players were on the waitlist for the last 2 day tournament and have been given priority to get into this tournament.' . PHP_EOL;
		echo '</td></tr>' . PHP_EOL;
		echo '</tbody>' . PHP_EOL;
		echo '</table>' . PHP_EOL;

		ShowWaitingListTable($waitingList, 4);
	}
}
echo '    </div><!-- #content -->';
echo ' </div><!-- #content-container -->';

$connection->close ();
get_footer ();
?>