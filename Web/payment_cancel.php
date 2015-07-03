<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/signup functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

get_header ();

echo ' <div id="content-container" class="entry-content">';
echo '    <div id="content" role="main">' . PHP_EOL;

var_dump($_POST);

echo '    </div><!-- #content -->';
echo ' </div><!-- #content-container -->';

get_footer ();

?>