<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/signup functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$overrideTitle = "Remove Players";
get_header ();

get_sidebar ();

$tournamentKey = $_GET ['tournament'];
if (! $tournamentKey) {
	die ( "Which tournament?" );
}
$signupKey = $_GET['signup'];
if(! $signupKey) {
	die ( "Which signup?" );
}

$testMode = false;
if($_GET ['mode'] == "test"){
	$testMode = true;
}

//$everything = get_defined_vars();
//ksort($everything);
//echo '<pre>';
//print_r($everything);
//echo '</pre>';

//var_dump($_POST);

$connection = new mysqli ( $db_hostname, $db_username, $db_password, $db_database );
if ($connection->connect_error)
	die ( $connection->connect_error );

$players = GetPlayersForSignUp($connection, $signupKey);

if(count($players) == 0){
	die ("There are no players for signup code " . $_GET ['signup']);
}

$tournament = GetTournament($connection, $tournamentKey);
if(empty($tournament)){
	die("There is no tournament numbered " . $tournamentKey);
}

$refundRequested = false;
$refundFees = 0;
$allPlayersRemoved = false;
$playerIndexRemoved = 100;
if (isset ( $_POST ['PlayersToRemove'] ) && (count($_POST ['PlayersToRemove']) > 0)) {
	$playersToRemove = $_POST ['PlayersToRemove'];
	$accessCode = trim($_POST['AccessCode']);
	
	if(empty($accessCode)){
		$error = "Fill in the Access Code";
	}
	else {
		
		$signup = GetSignup($connection, $signupKey);
		if(empty($signup)){
			die("There is no data for signup key: " . $signupKey);
		}
		
		if($signup->AccessCode != $accessCode){
			$error = "Invalid access code";
		}
		else {
			$playersRemoved = array();
			for($i = 0; ($i < count($playersToRemove)) && empty($error); ++$i){
				$removed = false;
				for($j = 0; ($j < count($players)) && !$removed; ++$j){
					if($players[$j]->GHIN == $playersToRemove[$i]){
						RemoveSignedUpPlayer ( $connection, $tournamentKey, $playersToRemove[$i], $players[$j]->LastName );
						$playersRemoved[] = $players[$j]->LastName;
						$removed = true;
						$playerIndexRemoved = $players[$j]->Position;
					}
				}
				
				if(!$removed){
					$error = "Player with GHIN " . $playersToRemove[$i] . " was not part of the group with signup key " . $signupKey;
				}
			}
			
			if(count($playersRemoved) > 0){
				
				if($tournament->RequirePayment){
					$cost = $tournament->Cost;
					if($testMode){
						$cost = 3;
					}
				
					$paypalDetails = GetPayPalDetails($connection, $cost);
				
					if(!isset($paypalDetails->PayPayButton)){
						die("No PayPal button for tournament fee " . $cost);
					}
				
					$refundFees = count($playersRemoved) * ($paypalDetails->TournamentFee + $paypalDetails->ProcessingFee);
					$remainingFees = $signup->PaymentDue - $refundFees;
					if($remainingFees < 0){
						$remainingFees = 0;
					}
					
					UpdateSignup($connection, $signupKey, 'PaymentDue', $remainingFees, 'd');
					// If they have paid, request a refund
					if($signup->Payment > 0){
						SendRefundEmail($connection, $tournament, $signup, $players, $playersRemoved, $refundFees, $web_site);
						$refundRequested = true;
					}
				}
			}
			
			// This is a team event.  We removed a player from team 1 and there is a full team 2.  Move team 2 to
			// team 1 and move the single to team 2 so the empty slot is last.
			if(($tournament->TeamSize == 2) && (count($playersRemoved) == 1) && (count($players) == 4) && ($playerIndexRemoved < 2)){
				
				for($i = 0; $i < 4; ++$i){
					if($i != $playerIndexRemoved){
						if(($players[$i]->Position == 0) || ($players[$i]->Position == 1)){
							// Move the single from the 1st team to the second team
							UpdateSignupPlayer($connection, $signupKey, $players[$i]->GHIN, 'Position', 2, 'i');
						}
						else if($players[$i]->Position == 2){
							// Move to team 1
							UpdateSignupPlayer($connection, $signupKey, $players[$i]->GHIN, 'Position', 0, 'i');
						}
						else if($players[$i]->Position == 3){
							// Move to team 1
							UpdateSignupPlayer($connection, $signupKey, $players[$i]->GHIN, 'Position', 1, 'i');
						}
					}
				}
			}
			
			$players = GetPlayersForSignUp($connection, $signupKey);
			if(empty($players)){
				DeleteSignup($connection, $signupKey);
				$allPlayersRemoved = true;
			}
		}
	}
}

echo '<div id="content-container" class="entry-content">' . PHP_EOL;
echo '<div id="content" role="main">' . PHP_EOL;
echo '<h2 class="entry-title" style="text-align:center">Remove Players</h2>' . PHP_EOL;
echo '<p>Use this form to remove players in your group. Use the checkbox to indicate which players to remove.  Multiple players can be removed in a single request.</p>';
if($tournament->RequirePayment){
	echo '<p>An email will be sent to the tournament director requesting a refund if you have paid your tournament fee.</p>';
}
echo PHP_EOL;
		
if(!empty($error)){
	echo '<p style="color:red;">' . $error . '</p>' . PHP_EOL;
}

if($allPlayersRemoved){
	echo '<p>' . PHP_EOL;
	echo "All players have been removed";
	echo '</p>' . PHP_EOL;
}
else {
	echo '<form name="input" method="post">' . PHP_EOL;
	echo 'Access Code: <input type="text" name="AccessCode" maxlength="4" size="4" value="' .  $_POST['AccessCode'] . '"><br><br>' . PHP_EOL;
	echo 'Players to remove:<p>' . PHP_EOL;
	
	for($i = 0; $i < count($players); ++$i){
		echo '&nbsp;&nbsp;&nbsp;<input  type="checkbox" name="PlayersToRemove[]" value="' . $players[$i]->GHIN . '">' . $players[$i]->LastName . '<br>' . PHP_EOL;
	}
	
	echo '</p>' . PHP_EOL;
	echo '<input type="submit" value="Remove Players"> <br> <br>' . PHP_EOL;
	echo '</form>' . PHP_EOL;
}
echo '<p>' . PHP_EOL;
echo '<a href="signups.php?tournament=' . $tournamentKey . '">View Signups</a>' . PHP_EOL;
echo '</p>' . PHP_EOL;
echo '</div><!-- #content -->' . PHP_EOL;
echo '</div><!-- #content-container -->' . PHP_EOL;

if (isset ( $connection )) {
	$connection->close ();
}
get_footer ();
?>