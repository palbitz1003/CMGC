<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tee times functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/results_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$connection = new mysqli ( $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( $connection->connect_error );
	
	// var_dump($_POST);
	// echo '<br>';
	// var_dump($_FILES);
	// echo '<br>';

login ( $_POST ['Login'], $_POST ['Password'] );

if ($_FILES ["file"] ["error"] > 0) {
	echo "Error: " . $_FILES ["file"] ["error"] . "<br>";
} else {
	// echo "Upload: " . $_FILES["file"]["name"] . "<br>";
	// echo "Type: " . $_FILES["file"]["type"] . "<br>";
	// echo "Size: " . ($_FILES["file"]["size"] / 1024) . " kB<br>";
	// echo "Stored in: " . $_FILES["file"]["tmp_name"];
	
	if ($_POST ['Action'] == 'Submit') {
		if (! IsHTML ( $_FILES ['file'] ['tmp_name'] )) {
			die ( 'Submitted file does not start with "<html"' );
		}
		
		$t = GetTournament ( $connection, $_POST ["TournamentKey"] );
		if (! isset ( $t )) {
			die ( 'This is not a valid tournament key: ' . $_POST ["TournamentKey"] );
		}
		
		$start = new DateTime ( $t->StartDate );
		
		$fileName = sprintf ( '%s %s %d.html', $start->format ( 'Y M d' ), $_POST ["Result"], $_POST ["TournamentKey"] );
		
		// echo 'new file name ' . $fileName;
		
		// Update tournament details
		switch ($_POST ["Result"]) {
			case 'scores' :
				UpdateTournamentDetails ( $connection, $_POST ["TournamentKey"], 'ScoresFile', $fileName );
				UpdateTournamentDetails ( $connection, $_POST ["TournamentKey"], 'ScoresPostedDate', date ( 'Y-m-d' ) );
				break;
			case 'chits' :
				UpdateTournamentDetails ( $connection, $_POST ["TournamentKey"], 'ChitsFile', $fileName );
				UpdateTournamentDetails ( $connection, $_POST ["TournamentKey"], 'ChitsPostedDate', date ( 'Y-m-d' ) );
				break;
			case 'pool' :
				UpdateTournamentDetails ( $connection, $_POST ["TournamentKey"], 'PoolFile', $fileName );
				UpdateTournamentDetails ( $connection, $_POST ["TournamentKey"], 'PoolPostedDate', date ( 'Y-m-d' ) );
				break;
			default :
				die ( 'Unknown result type: ' . $_POST ["Result"] );
		}
		
		move_uploaded_file ( $_FILES ["file"] ["tmp_name"], "results/" . $fileName );
	} else if ($_POST ['Action'] == 'Clear') {
		
		if (strcasecmp ( $_POST ["Result"], 'scores' ) == 0) {
			ClearResults ( $connection, $tournamentKey, 'Scores' );
		} else if (strcasecmp ( $_POST ["Result"], 'chits' ) == 0) {
			ClearResults ( $connection, $tournamentKey, 'Chits' );
		} else if (strcasecmp ( $_POST ["Result"], 'pool' ) == 0) {
			ClearResults ( $connection, $tournamentKey, 'Pool' );
		} else {
			die ( 'Expected to clear scores, chits, or pool, got: ' . $_POST ["Result"] );
		}
	}  else {
		die ( 'Unknown action requested: ' . $_POST ['Action'] );
	}
	
	echo "Success";
}

$connection->close ();
?>