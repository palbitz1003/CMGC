<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/functions.php';
require_once realpath ( $_SERVER ["DOCUMENT_ROOT"] ) . $script_folder . '/signup functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tee times functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

class TeeTimeComposite
{
    public $TeeTimes;
    public $WaitlistPlayers;
    public $CancelledPlayers;
}

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
    public $Tee;
}

// login() requires headers functions.php and wp-blog-header.php
login($_POST ['Login'], $_POST ['Password']);

$connection = new mysqli ('p:' . $db_hostname, $db_username, $db_password, $db_database );

$tournamentKey = $_POST ['tournament'];
if (! $tournamentKey || !is_numeric($tournamentKey)) {
	die ( "Which tournament?" );
}

if ($connection->connect_error){
	die ( $connection->connect_error );
}

$activeRoster = GetAllActiveRosterEntries($connection);

$teeTimeComposite = new TeeTimeComposite();

FillInTeeTimes($connection, $tournamentKey, $teeTimeComposite, $activeRoster);

FillInWaitListPlayers($connection, $tournamentKey, $teeTimeComposite, $activeRoster);

FillInCancelledPlayers($connection, $tournamentKey, $teeTimeComposite, $activeRoster);

try
{
    echo json_encode($teeTimeComposite, JSON_THROW_ON_ERROR);
}
catch(JsonException $e)
{
    echo "JSON error: (from get_tee_times_json.php json_encode): " . $e->getMessage();
}

function FillInTeeTimes($connection, $tournamentKey, $teeTimeComposite, $activeRoster){

    $teeTimes = GetTeeTimes($connection, $tournamentKey);

    $teeTimeComposite->TeeTimes = array();
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

            $player->Tee = "W";
            $player->Email = "";
            if($player->GHIN !== 0){
                if(array_key_exists($player->GHIN, $activeRoster)){
                    $rosterEntry = $activeRoster[$player->GHIN];
                    $player->Email = $rosterEntry-> Email;
                    $player->Tee = $rosterEntry->Tee;
                }
            }
    
            $teeTime->Players[] = $player;
        }
    
        $teeTimeComposite->TeeTimes[] = $teeTime;
    }
}

function FillInWaitListPlayers($connection, $tournamentKey, $teeTimeComposite, $activeRoster){

    $entries = GetTeeTimeWaitingList($connection, $tournamentKey);

    $teeTimeComposite->WaitlistPlayers = array();
    for($i = 0; $i < count($entries); ++$i){
        
        if(intval($entries[$i]->GHIN) === 0) {
            $playerSignUp = GetPlayerSignUpByName($connection, $tournamentKey, $entries[$i]->Name);
        }
        else {
            $playerSignUp = GetPlayerSignUp($connection, $tournamentKey, $entries[$i]->GHIN);
        }

        $player = new Player();
        if($playerSignUp){
            
            $player->Name = $playerSignUp->LastName;  // is actually full name
            $player->Position = $entries[$i]->Position; // waitlist position
            $player->GHIN = $playerSignUp->GHIN;
            $player->Extra = $playerSignUp->Extra;
            $player->SignupKey = $playerSignUp->SignUpKey;
        }
        else {
            // Take what info we have. If this player makes it into the
            // tournament, the tee time submission will create a signup.
            $player->Name = $entries[$i]->Name;
            $player->Position = $entries[$i]->Position; // waitlist position
            $player->GHIN = $entries[$i]->GHIN;
            $player->Extra = "";
            $player->SignupKey = 0;
        }

        $player->Email = "";
        $player->Tee = "W";
        if(array_key_exists($entries[$i]->GHIN, $activeRoster)){
            $player->Email = $activeRoster[$entries[$i]->GHIN]-> Email;
            $player->Tee = $activeRoster[$entries[$i]->GHIN]-> Tee;
        }

        $teeTimeComposite->WaitlistPlayers[] = $player;
    }
}

function FillInCancelledPlayers($connection, $tournamentKey, $teeTimeComposite, $activeRoster){

    $cancelledList = GetTeeTimesCancelledList($connection, $tournamentKey);

    $teeTimeComposite->CancelledPlayers = array();
    for($i = 0; $i < count($cancelledList); ++$i){
        $player = new Player();
        $player->Name = $cancelledList[$i]->Name;
        $player->GHIN = $cancelledList[$i]->GHIN;
        $player->Position = $cancelledList[$i]->Position;
        $player->Extra = "";
        $player->SignupKey = 0; // may no longer be signed up

        $player->Tee = "W";
        $player->Email = "";
        if($player->GHIN !== 0){
            if(array_key_exists($player->GHIN, $activeRoster)){
                $player->Email = $activeRoster[$player->GHIN]-> Email;
                $player->Tee = $activeRoster[$player->GHIN]->Tee;
            }
        }

        $teeTimeComposite->CancelledPlayers[] = $player;
    }
}

$connection->close ();

?>