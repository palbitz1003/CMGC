<?php
require_once realpath ( $_SERVER ["DOCUMENT_ROOT"] ) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/dues_functions.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$ghin = $_POST ['GHIN'];
if (empty($ghin)) {
	die ( "Which GHIN number?" );
}

$connection = new mysqli ( 'p:' . $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( $connection->connect_error );

$player = GetPlayerDues($connection, $ghin);

if(empty($player)){
	$player = new Dues();
	$player->GHIN = 0;
	$player->Payment = 0;
}

echo json_encode($player);

$connection->close ();
?>