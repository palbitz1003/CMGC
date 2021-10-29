<?php
require_once realpath ( $_SERVER ["DOCUMENT_ROOT"] ) . '/login.php';
require_once realpath ( $_SERVER ["DOCUMENT_ROOT"] ) . $script_folder . '/signup functions.php';
require_once realpath ( $_SERVER ["DOCUMENT_ROOT"] ) . $wp_folder . '/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$tournamentKey = $_GET ['tournament'];
if (! $tournamentKey || !is_numeric($tournamentKey)) {
	die ( "Which tournament?" );
}
$submitKey = $_GET ['signup'];
if(empty($submitKey) || !is_numeric($submitKey)){
	die ("Which signup?");
}

$testMode = false;
if(!empty($_GET ['mode']) && ($_GET ['mode'] == "test")){
	$testMode = true;
}

$overrideTitle = "Pay";
get_header ();

get_sidebar ();

$connection = new mysqli ( $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( $connection->connect_error );

echo ' <div id="content-container" class="entry-content">';
echo '    <div id="content" role="main">';

$tournament = GetTournament($connection, $tournamentKey);
if(empty($tournament)){
	die("Invalid tournament key: " . $tournamentKey);
}

ShowPayment($web_site, $ipn_file, $script_folder_href, $connection, $tournament, $submitKey, null, $testMode);

echo '    </div><!-- #content -->';
echo ' </div><!-- #content-container -->';

$connection->close ();
get_footer ();
?>