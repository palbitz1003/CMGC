<?php
require_once realpath ( $_SERVER ["DOCUMENT_ROOT"] ) . '/login.php';
require_once realpath ( $_SERVER ["DOCUMENT_ROOT"] ) . $script_folder . '/signup functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_functions.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$tournamentKey = $_POST ['tournament'];
if (! $tournamentKey) {
	die ( "Which tournament?" );
}

$connection = new mysqli ( 'p:' . $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( $connection->connect_error );

$tournament = GetTournament($connection, $tournamentKey);
$signUpArray = GetSignups ( $connection, $tournamentKey, 'ORDER BY `SubmitKey` ASC' );

//var_dump($signUpArray);

for($i = 0; $i < count ( $signUpArray ); ++ $i) {
	$playersSignedUp = GetPlayersForSignUp ( $connection, $signUpArray[$i]->SignUpKey );
	
	$players = null;
	for($p = 0; $p < count ( $playersSignedUp ); ++ $p) {
		if (! empty ( $players )) {
			$players = $players . ",";
		}
		$extra = $playersSignedUp [$p]->Extra;
		if((strlen($extra) == 0) && ($playersSignedUp [$p]->GHIN === 0)){
			$extra = "N";
		}
		if(($extra === "G") && $tournament->MemberGuest)
		{
			$extra = $playersSignedUp [$p]->GHIN;
		}
		$players = $players . '"' . $playersSignedUp [$p]->LastName . '",' . $playersSignedUp [$p]->GHIN . ',' . $extra;
	}
	
	if (!empty($players)) {
		if($tournament->RequirePayment){
			$paymentDateTime = "None";
		}
		else {
			$paymentDateTime = "No payment required";
		}
		if (!empty($signUpArray [$i]->PaymentDateTime) && (strpos ( $signUpArray [$i]->PaymentDateTime, "00:00:00" ) === false)) {
			$paymentDateTime = $signUpArray [$i]->PaymentDateTime;
		}
		echo $signUpArray [$i]->RequestedTime;
		echo "," . $signUpArray [$i]->Payment;
		echo "," . $signUpArray [$i]->PaymentDue;
		echo "," . $paymentDateTime;
		echo "," . $signUpArray [$i]->AccessCode;
		echo "," . $signUpArray [$i]->SignUpKey;
		echo "," . $signUpArray [$i]->PayerName;
		echo "," . $signUpArray [$i]->PayerEmail;
		echo "," . $players . PHP_EOL;
	}
}

$connection->close ();
?>