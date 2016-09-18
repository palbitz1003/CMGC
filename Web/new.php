<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

get_header ();

echo ' <div id="content-container" class="entry-content">';
echo '    <div id="content" role="main">';

echo 'This page is obsolete. Please delete your bookmark if you have this page bookmarked and instead use ';
echo '<a href="https://coronadomensgolf.org">Coronado Men\'s Golf Club Home Page</a>';

echo '    </div><!-- #content -->';
echo ' </div><!-- #content-container -->';

get_sidebar ();
get_footer ();
?>