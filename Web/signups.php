<?php
require_once realpath ( $_SERVER ["DOCUMENT_ROOT"] ) . '/login.php';
require_once realpath ( $_SERVER ["DOCUMENT_ROOT"] ) . $script_folder . '/signup functions.php';
require_once realpath ( $_SERVER ["DOCUMENT_ROOT"] ) . $wp_folder . '/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$tournamentKey = $_GET ['tournament'];
if (! $tournamentKey) {
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

echo '    </div><!-- #content -->';
echo ' </div><!-- #content-container -->';

$connection->close ();
get_footer ();
?>