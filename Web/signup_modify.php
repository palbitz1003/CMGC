<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/signup functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_descriptions_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$overrideTitle = "Modify Signup";

get_header ();

get_sidebar ();

$tournamentKey = $_GET ['tournament'];
if (! $tournamentKey || !is_numeric($tournamentKey)) {
	die ( "Which tournament?" );
}

$signupKey = $_GET['signup'];
if(! $signupKey || !is_numeric($signupKey)) {
	die ( "Which signup?" );
}

$connection = new mysqli ( $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( $connection->connect_error );

$signup = GetSignup($connection, $signupKey);
if(empty($signup)){
	die("There is no data for signup key: " . $signupKey);
}

$players = GetPlayersForSignUp($connection, $signupKey);

if(count($players) == 0){
	die ("There are no players for signup code " . $_GET ['signup']);
}

$t = GetTournament($connection, $tournamentKey);
if(empty($t)){
	die("There is no tournament numbered " . $tournamentKey);
}

if($players[0]->TournamentKey != $tournamentKey){
	die("The players for signup key  " . $signupKey . " are part of tournament " . $players[0]->TournamentKey . " not tournament " . $tournamentKey);
}

if(IsPastSignupPeriod($t)) {
	echo '<div style = "position:relative; top:80px;text-align: center;">';
	echo "The signup period has ended for this tournament." . PHP_EOL;
    echo '</div>' . PHP_EOL;
	
	if (isset ( $connection )) {
		$connection->close ();
	}
	get_footer ();
	return;
}

//var_dump($_POST);

$error = null;
$flightErrorList = array();
$Extra = array();
$RequestedTime = $signup->RequestedTime;

for($i = 0; $i < count($players); ++ $i) {
	$Extra[$i] = $players[$i]->Extra;
}

if (isset ( $_POST ['RequestedTime'] )) {
	
	$accessCode = trim($_POST['AccessCode']);
	
	if(empty($accessCode)){
		$error = "Fill in the Access Code";
	}
	else if($signup->AccessCode != $accessCode){
		$error = "Invalid access code";
	}
	
	$RequestedTime = $_POST ['RequestedTime'];

	for($i = 0; $i < count($players); ++ $i) {
		
		if(($t->TeamSize == 2) && ($t->SCGAQualifier))
		{
			if(($i == 0) || ($i == 1)){
				$teamFlightIndex = GetTeamFlightIndex(1);
				$Extra[$i] = $_POST[$teamFlightIndex];
			}
			else {
				$teamFlightIndex = GetTeamFlightIndex(2);
				$Extra[$i] = $_POST[$teamFlightIndex];
			}
			if(empty ($_POST[$teamFlightIndex]))
			{
				$flightErrorList[$i] = "Select Flight";
			}
		}
		else if($t->SrClubChampionship)
		{
			$playerFlightIndex = GetPlayerFlightIndex($i + 1);
			$Extra[$i] = $_POST[$playerFlightIndex];
			if(empty ($_POST[$playerFlightIndex]))
			{
				$flightErrorList[$i] = "Select Flight";
			}
		}
	}
}


$hasError = false;
for($i = 0; $i < 4; ++ $i) {
	if (!empty($error) || ! empty ($flightErrorList [$i])) {
		$hasError = true;
	}
}

echo '<div id="content-container" class="entry-content">' . PHP_EOL;
echo '<div id="content" role="main">' . PHP_EOL;
echo '<h2 class="entry-title">Modify Signup</h2>' . PHP_EOL;
echo '<h3>' . $t->Name . '</h3>' . PHP_EOL;

// If this page has not been filled in or there is an error, show the form
if ($hasError || !isset ( $_POST ['RequestedTime'] )) {

	echo '<p>' . PHP_EOL;
	echo 'Change the settings below and click Submit' . PHP_EOL;
	echo '</p>' . PHP_EOL;
	echo '<form name="input" method="post">' . PHP_EOL;
	
	echo 'Access Code: <input type="text" name="AccessCode" maxlength="4" size="4" value="' .  $_POST['AccessCode'] . '"><br><br>' . PHP_EOL;
	if(!empty($error)){
		echo '<p style="color:red;">' . $error . '</p>' . PHP_EOL;
	}
	
	if($t->TeamSize == 2)
	{
		echo '<table style="width: 450px">' . PHP_EOL;
		echo '	<colgroup>' . PHP_EOL;
		echo '		<col style="width: 100px">' . PHP_EOL;
		echo '		<col>' . PHP_EOL;
		echo '	</colgroup>' . PHP_EOL;
		echo '<tr>' . PHP_EOL;
		TeamNumber($t, 1, $flightErrorList, $Extra);
		
		echo '<td>' . PHP_EOL;
		AddPlayerTable($t);
		
		AddPlayer($t, 1, $players, $Extra, $flightErrorList);
		AddPlayer($t, 2, $players, $Extra, $flightErrorList);
		
		echo '</table>' . PHP_EOL;
		echo '</td>' . PHP_EOL;
		echo '</tr>' . PHP_EOL;
		
		if(count($players) > 2){
			echo '<tr>' . PHP_EOL;
			TeamNumber($t, 2, $flightErrorList, $Extra);
			
			echo '<td>' . PHP_EOL;
			AddPlayerTable($t);
			
			AddPlayer($t, 3, $players, $Extra, $flightErrorList);
			AddPlayer($t, 4, $players, $Extra, $flightErrorList);
			
			echo '</table>' . PHP_EOL;
			echo '</td>' . PHP_EOL;
			echo '</tr>' . PHP_EOL;
		}
		
		echo '<tr>' . PHP_EOL;
		echo '<td></td>' . PHP_EOL;  // empty team number
		
		echo '<td>' . PHP_EOL;
		AddPlayerTable($t);
		
		RequestedTime($RequestedTime);
		
		echo '</table>' . PHP_EOL;
		echo '</td>' . PHP_EOL;
		echo '</tr>' . PHP_EOL;
		echo '</table>' . PHP_EOL;
		
	}
	else 
	{
		AddPlayerTable($t);
		
		AddPlayer($t, 1, $players, $Extra, $flightErrorList);
		AddPlayer($t, 2, $players, $Extra, $flightErrorList);
		AddPlayer($t, 3, $players, $Extra, $flightErrorList);
		AddPlayer($t, 4, $players, $Extra, $flightErrorList);
	
		RequestedTime($RequestedTime);
		echo '</table>' . PHP_EOL;
	}

	echo '<input type="hidden" name="tournament" value="' . $tournamentKey . '">' . PHP_EOL;
	echo '<input type="submit" value="Submit"> <br> <br>' . PHP_EOL;
	echo '</form>' . PHP_EOL;


} else {
	// Make the modifications
	UpdateSignup($connection, $signupKey, 'RequestedTime', $RequestedTime, 's');
	
	if(($t->TeamSize == 2) && ($t->SCGAQualifier))
	{
		for($i = 0; $i < count($players); ++ $i) {
			UpdateSignupPlayer($connection, $signupKey, $players[$i]->GHIN, 'Extra', $Extra[$i], 's');
		}
	}
	else if($t->SrClubChampionship){
		for($i = 0; $i < count($players); ++ $i) {
			UpdateSignupPlayer($connection, $signupKey, $players[$i]->GHIN, 'Extra', $Extra[$i], 's');
		}
	}
	
	echo '<p>' . PHP_EOL;
	echo 'Your changes have been saved.' . PHP_EOL;
	echo '</p>' . PHP_EOL;
} // end of else clause

echo '<p>' . PHP_EOL;
echo '<a href="signups.php?tournament=' . $tournamentKey . '">View Signups</a>' . PHP_EOL;
echo '</p>' . PHP_EOL;
echo '</div><!-- #content -->' . PHP_EOL;
echo '</div><!-- #content-container -->' . PHP_EOL;


function AddPlayer($t, $playerNumber, $players, $extra, $error)
{
	$index = $playerNumber - 1;
	if($index < count($players)){
		echo '<tr>' . PHP_EOL;
		echo '<td style="border: none;">Player ' . $playerNumber . ' Name:</td>' . PHP_EOL;
		echo '<td style="border: none;">' . $players[$index]->LastName . '</td>' . PHP_EOL;
		AddFlights($t, $playerNumber, $extra[$index], $error[$index], 1);
		echo '</tr>';
	}
}


if (isset ( $connection )) {
	$connection->close ();
}

get_footer ();
?>