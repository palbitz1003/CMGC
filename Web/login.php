<?php 
// login.php
$root = realpath($_SERVER["DOCUMENT_ROOT"]);
$web_site = 'paulalbitz.com';
$db_hostname = 'mysql.' . $web_site;
$db_database = 'cmgc_test'; 
$db_username = 'cmgc';
$db_password = 'cor53Onado'; 
$wp_folder = '/cmgc1';
$script_folder = '/v2';
$script_folder_href = 'v2/';
$ipn_file = 'paypal_ipn.php';
$ipn_dues_file = 'paypal_dues_ipn.php';
?>