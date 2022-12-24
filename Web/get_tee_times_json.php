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

$teeTimeComposite = new TeeTimeComposite();

FillInTeeTimes($connection, $tournamentKey, $teeTimeComposite);

FillInWaitListPlayers($connection, $tournamentKey, $teeTimeComposite);

FillInCancelledPlayers($connection, $tournamentKey, $teeTimeComposite);

try
{
    echo json_encode($teeTimeComposite, JSON_THROW_ON_ERROR);
}
catch(JsonException $e)
{
    echo "JSON error: (from get_tee_times_json.php json_encode): " . $e->getMessage();
}

function FillInTeeTimes($connection, $tournamentKey, $teeTimeComposite){

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
                $rosterEntry = GetRosterEntry ( $connection, $player->GHIN );
                if($rosterEntry){
                    $player->Email = $rosterEntry-> Email;
                    $player->Tee = $rosterEntry->Tee;
                }
            }
    
            $teeTime->Players[] = $player;
        }
    
        $teeTimeComposite->TeeTimes[] = $teeTime;
    }
}

function FillInWaitListPlayers($connection, $tournamentKey, $teeTimeComposite){

    $entries = GetSignUpWaitingList($connection, $tournamentKey);

    $teeTimeComposite->WaitlistPlayers = array();
    for($i = 0; $i < count($entries); ++$i){
        
        if(intval($entries[$i]->GHIN1) === 0) {
            $playerSignUp = GetPlayerSignUpByName($connection, $tournamentKey, $entries[$i]->Name1);
        }
        else {
            $playerSignUp = GetPlayerSignUp($connection, $tournamentKey, $entries[$i]->GHIN1);
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
            $player->Name = $entries[$i]->Name1;
            $player->Position = $entries[$i]->Position; // waitlist position
            $player->GHIN = $entries[$i]->GHIN1;
            $player->Extra = "";
            $player->SignupKey = 0;
        }

        $player->Email = "";
        $player->Tee = "W";
        $rosterEntry = GetRosterEntry ( $connection, $entries[$i]->GHIN1 );
        if($rosterEntry){
            $player->Email = $rosterEntry-> Email;
            $player->Tee = $rosterEntry->Tee;
        }

        $teeTimeComposite->WaitlistPlayers[] = $player;
    }
}

function FillInCancelledPlayers($connection, $tournamentKey, $teeTimeComposite){

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
            $rosterEntry = GetRosterEntry ( $connection, $player->GHIN );
            if($rosterEntry){
                $player->Email = $rosterEntry-> Email;
                $player->Tee = $rosterEntry->Tee;
            }
        }

        $teeTimeComposite->CancelledPlayers[] = $player;
    }
}

$connection->close ();

?>