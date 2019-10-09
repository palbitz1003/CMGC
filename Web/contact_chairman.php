<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/results_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$connection = new mysqli ( $db_hostname, $db_username, $db_password, $db_database );

get_header ();
$overrideTitle = "Contact Touranment Director";

echo ' <div id="content-container" class="entry-content">';
echo '    <div id="content" role="main">' . PHP_EOL;

echo '<h2 style="text-align:center">Contact Tournament Director</h2><br>';

$tournamentKey = $_GET ['tournament'];
if (empty($tournamentKey)) {
	echo "Which tournament?";
}
else {
	$t = GetTournament ( $connection, $tournamentKey );

	if (isset ( $t )) {
		echo '<p>The online signup period has ended. Contact ' . $t->ChairmanName . ' directly for any changes at ';
		if(!empty($t->ChairmanPhone)){
			echo " phone number " . $t->ChairmanPhone . " or ";
		}
		echo '<a href="mailto:' . $t->ChairmanEmail . '">email.</a></p>';
	} else {
		echo "Invalid tournament: " . $tournamentKey;
	}
}

echo '    </div><!-- #content -->';
echo ' </div><!-- #content-container -->';


get_footer ();
?>