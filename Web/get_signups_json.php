<?php
require_once realpath ( $_SERVER ["DOCUMENT_ROOT"] ) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/functions.php';
require_once realpath ( $_SERVER ["DOCUMENT_ROOT"] ) . $script_folder . '/signup functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

class TeeTimeRequest {
    public $Preference;
    public $PaymentMade;
    public $PaymentDue;
    public $PaymentDateTime;
    public $AccessCode;
    public $SignupKey;
    public $PayerName;
    public $PayerEmail;
    public $Players;
}

// Player class is duplicated in get_tee_times_json.php
class Player {
    public $Name;
    public $Position;
    public $GHIN;
    public $Extra;
    public $Email;
    public $SignupKey;
    public $SignupPriority;
}

// login() requires headers functions.php and wp-blog-header.php
login($_POST ['Login'], $_POST ['Password']);

$tournamentKey = $_POST ['tournament'];
if (! $tournamentKey || !is_numeric($tournamentKey)) {
	die ( "Which tournament?" );
}

$connection = new mysqli ( 'p:' . $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( $connection->connect_error );

$tournament = GetTournament($connection, $tournamentKey);
$signUpArray = GetSignups ( $connection, $tournamentKey, 'ORDER BY `SubmitKey` ASC' );

$eightyYearsAgo = new DateTime ( $tournament->StartDate );
$eightyYearsAgo->sub(new DateInterval ( 'P80Y' ));

//var_dump($signUpArray);

$teeTimeRequests = array();
for($i = 0; $i < count ( $signUpArray ); ++ $i) {
	$playersSignedUp = GetPlayersForSignUp ( $connection, $signUpArray[$i]->SignUpKey );

	$playersArray = array();
	for($p = 0; $p < count ( $playersSignedUp ); ++ $p) {

		$extra = $playersSignedUp [$p]->Extra;
        /* 
		if((strlen($extra) == 0) && ($playersSignedUp [$p]->GHIN === 0)){
			$extra = "N";
		}
        
		if(($extra === "G") && $tournament->MemberGuest)
		{
			$extra = $playersSignedUp [$p]->GHIN;
		}
        */
        $email = "";
        $signupPriority = "G";
		if($playersSignedUp [$p]->GHIN !== 0){
			$rosterEntry = GetRosterEntry ( $connection, $playersSignedUp [$p]->GHIN );
			if($rosterEntry){
                $email = $rosterEntry-> Email;
                $signupPriority = $rosterEntry->SignupPriority;
			}
        }

        $player = new Player();
        $player->Name = $playersSignedUp [$p]->LastName;  // is actually full name
        $player->Position = $playersSignedUp [$p]->Position;
        $player->GHIN = $playersSignedUp [$p]->GHIN;
        $player->Extra = $extra;
        $player->Email = $email;
        $player->SignupKey = $playersSignedUp [$p]->SignUpKey;
        $player->SignupPriority = $signupPriority;

        $playersArray[] = $player;
    }
	
	if (count($playersArray) > 0) {
		if($tournament->RequirePayment){
			$paymentDateTime = "None";
		}
		else {
			$paymentDateTime = "No payment required";
		}
		if (!empty($signUpArray [$i]->PaymentDateTime) && (strpos ( $signUpArray [$i]->PaymentDateTime, "00:00:00" ) === false)) {
			$paymentDateTime = $signUpArray [$i]->PaymentDateTime;
        }
        
        $teeTimeRequest = new TeeTimeRequest();
        $teeTimeRequest->Preference = $signUpArray [$i]->RequestedTime;
        $teeTimeRequest->PaymentMade = $signUpArray [$i]->Payment;
        $teeTimeRequest->PaymentDue = $signUpArray [$i]->PaymentDue;
        $teeTimeRequest->PaymentDateTime = $paymentDateTime;
        $teeTimeRequest->AccessCode = $signUpArray [$i]->AccessCode;
        $teeTimeRequest->SignupKey = $signUpArray [$i]->SignUpKey;
        // Next line needs utf8_encode because payer name can have some non-standard
        // characters, like the "r" registered trademark 
        $teeTimeRequest->PayerName = utf8_encode($signUpArray [$i]->PayerName);
        $teeTimeRequest->PayerEmail = $signUpArray [$i]->PayerEmail;
        $teeTimeRequest->Players = $playersArray;
        $teeTimeRequests[] = $teeTimeRequest;
	}
}

try
{
    echo json_encode($teeTimeRequests, JSON_THROW_ON_ERROR);
}
catch(JsonException $e)
{
    echo "JSON error: (from get_signups_json.php json_encode): " . $e->getMessage();
}

$connection->close ();

?>