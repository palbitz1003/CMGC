<?php
require_once realpath ( $_SERVER ["DOCUMENT_ROOT"] ) . '/login.php';
require_once realpath ( $_SERVER ["DOCUMENT_ROOT"] ) . $script_folder . '/signup functions.php';
require_once realpath ( $_SERVER ["DOCUMENT_ROOT"] ) . $script_folder . '/tournament_functions.php';
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

$t = GetTournament ( $connection, $tournamentKey );

$now = new DateTime ( "now" );
$startSignUp = new DateTime($t->SignupStartDate);
$startSignUp->add(new DateInterval ( $signup_start_time ));
$endSignUp = new DateTime($t->SignupEndDate);
$endSignUp->add(new DateInterval ( $signup_end_time ));

$allowModifications = false;
if(($now >= $startSignUp) && ($now <= $endSignUp)){
	$allowModifications = true;
}

echo ' <div id="content-container" class="entry-content">';
echo '    <div id="content" role="main">';

ShowSignups($connection, $tournamentKey, $allowModifications);

echo '    </div><!-- #content -->';
echo ' </div><!-- #content-container -->';

$connection->close ();
get_footer ();
?>