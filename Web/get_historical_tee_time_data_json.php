<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/signup functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tee times functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

class TournamentAndTeeTimes
{
    public $Tournament;
    public $TeeTimes;
}

// login() requires headers functions.php and wp-blog-header.php
//login($_POST ['Login'], $_POST ['Password']);

$connection = new mysqli ('p:' . $db_hostname, $db_username, $db_password, $db_database );

$tournamentKey = 139;
$monthsOfData = 12;

/*
$tournamentKey = $_POST ['tournament'];
if (! $tournamentKey || !is_numeric($tournamentKey)) {
	die ( "Which tournament?" );
}

$monthsOfData = $_POST ['months'];
if (! $monthsOfData || !is_numeric($monthsOfData)) {
	die ( "How many months of data?" );
}
*/

if ($connection->connect_error){
	die ( $connection->connect_error );
}

// Get all tournaments, sorted by start date
$tournaments = GetTournaments($connection, '');

// Find the tournament starting point
$currentIndex = -1;
for($i = 0; ($i < count($tournaments)) && ($currentIndex == -1); ++$i){
    if($tournamentKey == $tournaments[$i]->TournamentKey){
        $currentIndex = $i;
        $beginningDate = new DateTime ( $tournaments[$i]->StartDate);
        $beginningDate->sub(new DateInterval ( 'P' . $monthsOfData . 'M' ));
    }
}

$tournamentAndTeeTimesArray = array();

if($currentIndex >= 0){
    for($i = $currentIndex - 1; $i >= 0; --$i){
        $startDate = new DateTime ( $tournaments[$i]->StartDate);
        if(!$tournaments[$i]->AnnouncementOnly && ($startDate >= $beginningDate)){
            $tournamentAndTeeTimes = new TournamentAndTeeTimes();
            $tournaments[$i]->ConvertToBool();  // convert ints to bools for json encoding
            $tournamentAndTeeTimes->Tournament = $tournaments[$i];
            $tournamentAndTeeTimes->TeeTimes = GetTeeTimes($connection, $tournamentKey);

            $tournamentAndTeeTimesArray[] = $tournamentAndTeeTimes;
        }
    }
}

echo json_encode($tournamentAndTeeTimesArray);

$connection->close ();

?>