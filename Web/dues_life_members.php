<?php
require_once realpath ( $_SERVER ["DOCUMENT_ROOT"] ) . '/login.php';
require_once realpath ( $_SERVER ["DOCUMENT_ROOT"] ) . $wp_folder . '/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$overrideTitle = "Dues For Life Members";
get_header ();

get_sidebar ();

echo ' <div id="content-container" class="entry-content">';
echo '    <div id="content" role="main">' . PHP_EOL;

echo '<h2 class="entry-title" style="text-align:center">Dues For Life Members</h2>' . PHP_EOL;

echo '<p>If you are currently a lifetime member please submit a check for $30.00 to cover the cost of maintaining your SCGA handicap. If you will be eligible for a lifetime membership in the coming year, submit your proof of age along with a check for $30.00 if you wish to maintain an official handicap. If you are a current lifetime member and do not wish to maintain a handicap, no action is required</p>';

echo '    </div><!-- #content -->';
echo ' </div><!-- #content-container -->' . PHP_EOL;

get_footer ();
?>