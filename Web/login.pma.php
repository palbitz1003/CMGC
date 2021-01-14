<?php 
// login.php
$root = realpath($_SERVER["DOCUMENT_ROOT"]);
$web_site = 'paulalbitz.com';
$db_hostname = 'cmgcdb.paulalbitz.com';
$db_database = 'coronadomensgolf_test'; 
$db_username = 'cmgc';
$db_password = 'pau58Onado'; 
$wp_folder = '/wp';
$script_folder = '/v2';
$script_folder_href = 'v2/';
$ipn_file = 'paypal_ipn.php';
$ipn_dues_file = 'paypal_dues_ipn.php';
// 5am, P is for "period" and T is for "time", 05H is 5 hours, 00M is 0 minutes
$signup_start_time = 'PT05H00M';
// noon, P is for "period" and T is for "time", 12H is 12 hours, 00M is 0 minutes
$signup_end_time = 'PT12H00M';
?>