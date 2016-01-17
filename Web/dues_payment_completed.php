<?php
require_once realpath ( $_SERVER ["DOCUMENT_ROOT"] ) . '/login.php';
require_once realpath ( $_SERVER ["DOCUMENT_ROOT"] ) . $wp_folder . '/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$overrideTitle = "Dues Payment Complete";
get_header ();

get_sidebar ();

echo ' <div id="content-container" class="entry-content">';
echo '    <div id="content" role="main">' . PHP_EOL;

echo '<h2 class="entry-title" style="text-align:center">Dues Payment Complete</h2>' . PHP_EOL;

echo '<p>Thank you for your payment. Your transaction has been completed, and a receipt for your tournament fees has been emailed to you. ';
echo 'You may log into your account at <a href="https://www.paypal.com/us">www.paypal.com/us</a> to view details of this transaction.</p>' . PHP_EOL;

//$everything = get_defined_vars();
//ksort($everything);
//echo '<pre>';
//print_r($everything);
//echo '</pre>';

echo '    </div><!-- #content -->';
echo ' </div><!-- #content-container -->' . PHP_EOL;

get_footer ();
?>