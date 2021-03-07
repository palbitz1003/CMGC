<?php
require_once realpath ( $_SERVER ["DOCUMENT_ROOT"] ) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/functions.php';
require_once realpath ( $_SERVER ["DOCUMENT_ROOT"] ) . $script_folder . '/signup functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

class Player {
    public $Name;
    public $Position;
    public $GHIN;
    public $Extra;
    public $Email;
    public $SignupKey;
}

// login() requires headers functions.php and wp-blog-header.php
login($_POST ['Login'], $_POST ['Password']);

$tournamentKey = $_POST ['tournament'];
if (! $tournamentKey) {
	die ( "Which tournament?" );
}

$connection = new mysqli ('p:' . $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( $connection->connect_error );

$entries = GetSignUpWaitingList($connection, $tournamentKey);

$waitlistPlayers = array();
for($i = 0; $i < count($entries); ++$i){
    
    $playerSignUp = GetPlayerSignUp($connection, $tournamentKey, $entries[$i]->GHIN1);

    if($playerSignUp){
        $player = new Player();
        $player->Name = $playerSignUp->LastName;  // is actually full name
        $player->Position = $entries[$i]->Position; // waitlist position
        $player->GHIN = $playerSignUp->GHIN;
        $player->Extra = $playerSignUp->Extra;
        $player->SignupKey = $playerSignUp->SignUpKey;

        $rosterEntry = GetRosterEntry ( $connection, $entries[$i]->GHIN1 );
        if($rosterEntry){
            $player->Email = $rosterEntry-> Email;
        }

        $waitlistPlayers[] = $player;
    }
    else {
        echo "Lookup of waitlist player " . $entries[$i]->Name1 . " (" . $entries[$i]->GHIN1 . ") failed";
    }
}

try
{
    echo json_encode($waitlistPlayers, JSON_THROW_ON_ERROR);
}
catch(JsonException $e)
{
    echo "JSON error: (from get_signups_json.php json_encode): " . $e->getMessage();
}

$connection->close ();

?>