<?php
require_once realpath ( $_SERVER ["DOCUMENT_ROOT"] ) . '/login.php';
require_once realpath ( $_SERVER ["DOCUMENT_ROOT"] ) . $script_folder . '/signup functions.php';
require_once realpath ( $_SERVER ["DOCUMENT_ROOT"] ) . $wp_folder . '/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$tournamentKey = $_GET ['tournament'];
if (! $tournamentKey) {
	die ( "Which tournament?" );
}
$submitKey = $_GET ['signup'];
if(empty($submitKey)){
	die ("Which signup?");
}

$overrideTitle = "Change Signup";
get_header ();

get_sidebar ();

if ($connection->connect_error)
	die ( $connection->connect_error );

echo ' <div id="content-container" class="entry-content">';
echo '    <div id="content" role="main">';

echo '<h2 class="entry-title" style="text-align:center">Change Signup</h2>' . PHP_EOL;

echo '<ul>' . PHP_EOL;
echo '<li><a href="https://' . $web_site . '/' . $script_folder_href . 'signup_remove_players.php?tournament=' . $tournamentKey . '&signup=' . $submitKey . '">Remove Players</a>';
echo ' - Remove players from your group.  This will trigger a refund if you have paid the tournament fees.</li>' . PHP_EOL;
echo '<li><a href="https://' . $web_site . '/' . $script_folder_href . 'signup_change_players.php?tournament=' . $tournamentKey . '&signup=' . $submitKey . '">Change Players</a>';
echo ' - Swap one player for another player in your group.  Since the number of players in your group remains the same, there is no change to your tournament fees.</li>' . PHP_EOL;
echo '</ul>' . PHP_EOL;

echo '    </div><!-- #content -->';
echo ' </div><!-- #content-container -->';

get_footer ();
?>