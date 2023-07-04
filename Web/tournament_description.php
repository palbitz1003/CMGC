<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_descriptions_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$overrideTitle = "Tournament Description";
get_header ();

echo ' <div id="content-container" class="entry-content">';
echo '    <div id="content" role="main">';

$connection = new mysqli ('p:' . $db_hostname, $db_username, $db_password, $db_database );

$tournamentKey = $_GET ['tournament'];
if (! $tournamentKey || !is_numeric($tournamentKey)) {
	die ( "Which tournament?" );
} 

if ($tournamentKey) {
	
	$t = GetTournament ( $connection, $tournamentKey );
	
	if (isset($t)) {
		
		echo '<h2>' . $t->Name . '</h2>' . PHP_EOL;
		
		$td = GetTournamentDescription($connection, $t->TournamentDescriptionKey);
		
		if(isset($td)){
			echo $td->Description . PHP_EOL;
		}
		DisplayTournamentDetails($t);
	}
	else {
		echo "Invalid tournament: " . $tournamentKey;
	}
}
else {
	echo 'No tournament specified';
}

echo '    </div><!-- #content -->';
echo ' </div><!-- #content-container -->';

$connection->close ();

get_sidebar ();
get_footer ();
?>