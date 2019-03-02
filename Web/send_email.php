<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/signup functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

get_header ();

get_sidebar ();

$signupKey = $_GET['signup'];
if(! $signupKey) {
	get_header ();
	
	get_sidebar ();
	die ( "Which signup?" );
}

$connection = new mysqli ('p:' . $db_hostname, $db_username, $db_password, $db_database );
if ($connection->connect_error)
	die ( $connection->connect_error );

$signup = GetSignup($connection, $signupKey);
if(empty($signup)){
	die("Did not find a signup for key: " . $signupKey);
}

$tournament = GetTournament($connection, $signup->TournamentKey);

if(empty($tournament)){
	die ("Did not find tournament for key: " . $signup->TournamentKey);
}

$tournamenDates = GetFriendlyNonHtmlTournamentDates($tournament);

//$errors = SendSignupEmail($connection, $tournament, $tournamenDates, $signupKey, $web_site);

if(!empty($errors)){
	echo "Error: " . $errors . "<br>";
}

$players = GetPlayersForSignUp($connection, $signupKey);
$playersRemoved[] = "none";
$refundFees = "as much as you can";
//SendRefundEmail($connection, $tournament, $signupKey, $players, $playersRemoved, $refundFees, $web_site);

if (isset ( $connection )) {
	$connection->close ();
}
get_footer ();
?>