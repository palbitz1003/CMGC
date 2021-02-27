<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath ( $_SERVER ["DOCUMENT_ROOT"] ) . $script_folder . '/signup functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tee times functions.php';
date_default_timezone_set ( 'America/Los_Angeles' );

class TeeTime {
    public $StartTime;
    public $Players;
}

// Player class is duplicated in get_signups_json.php
class Player {
    public $Name;
    public $Position;
    public $GHIN;
    public $Extra;
    public $Email;
    public $SignupKey;
}

$connection = new mysqli ('p:' . $db_hostname, $db_username, $db_password, $db_database );

$tournamentKey = $_POST ['tournament'];
if (! $tournamentKey) {
	die ( "Which tournament?" );
}

if ($connection->connect_error)
	die ( $connection->connect_error );

$teeTimes = GetTeeTimes($connection, $tournamentKey);

$teeTimesResponse = array();
for($i = 0; $i < count($teeTimes); ++$i){
    
    $teeTime = new TeeTime();
    $teeTime->StartTime = date ( 'g:i A', strtotime ($teeTimes[$i]->StartTime));
    $teeTime->Players = array();
    
	for($j = 0; $j < count($teeTimes[$i]->Players); ++$j){
        $player = new Player();
        $player->Name =  $teeTimes[$i]->Players[$j]->LastName;
        $player->GHIN = $teeTimes[$i]->Players[$j]->GHIN;
        $player->Position = $j + 1;
        $player->Extra = $teeTimes[$i]->Players[$j]->Extra;
        $player->SignupKey = $teeTimes[$i]->Players[$j]->SignupKey;

        if($player->GHIN !== 0){
			$rosterEntry = GetRosterEntry ( $connection, $player->GHIN );
			if($rosterEntry){
                $player->Email = $rosterEntry-> Email;
				//if($rosterEntry->BirthDate !== "0001-01-01"){
				//	$birthday = new DateTime($rosterEntry->BirthDate);
				//	if($birthday <= $eightyYearsAgo){
				//		$over80 = " >80";
				//	}
				//}
			}
        }

        $teeTime->Players[] = $player;
    }

    $teeTimesResponse[] = $teeTime;
}

try
{
    echo json_encode($teeTimesResponse, JSON_THROW_ON_ERROR);
}
catch(JsonException $e)
{
    echo "JSON error: (from get_tee_times_json.php json_encode): " . $e->getMessage();
}

$connection->close ();

?>