<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/signup functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

get_header ();

$connection = new mysqli ( $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( $connection->connect_error );

if (! isset ( $_POST ['Player'] )) {
	die ( "No list of players" );
} else {
	for($i = 0; $i < 4; ++ $i) {
		$GHIN [$i] = trim ( $_POST ['Player'] [$i] ['GHIN'] );
		$LastName [$i] = trim ( $_POST ['Player'] [$i] ['LastName'] );
		$Extra [$i] = trim ( $_POST ['Player'] [$i] ['Extra'] );
		$RequestedTime = $_POST ['RequestedTime'];
	}
}

$tournamentKey = $_POST['tournament'];
if (! $tournamentKey) {
	//var_dump($_POST);
	die ( "Which tournament?" );
}

if (empty ( $GHIN [0] )) {
	echo 'Player 1 GHIN is not filled in<br>';
} else if (empty ( $LastName [0] )) {
	echo 'Player 1 Last Name is not filled in<br>';
} else {
	
	$anyoneAlreadySignedUp = false;
	for($i = 0; $i < count ( $GHIN ); ++ $i) {
		if (IsPlayerSignedUp ( $connection, $tournamentKey, $GHIN [$i] )) {
			echo 'Player ' . $GHIN [$i] . ' is already signed up<br>';
			$anyoneAlreadySignedUp = true;
		}
	}
	
	if ($anyoneAlreadySignedUp) {
		return;
	}
	
	$insertId = InsertSignUp ( $connection, $tournamentKey, $RequestedTime );
	// echo 'insert id is: ' . $insertId . '<br>';
	
	InsertSignUpPlayers ( $connection, $tournamentKey, $insertId, $GHIN, $LastName, $Extra );
}

ShowSignups ( $connection, $tournamentKey);

$connection->close ();

get_sidebar ();

?>