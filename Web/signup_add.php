<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/signup functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_descriptions_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$overrideTitle = "Add Players";

class MergeSignUpClass {
	public $SignUpKey;
	public $PlayerNames;
}

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

$signupHasPaid = $signup->Payment >= $signup->PaymentDue;

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

if(($t->ClubChampionship || $t->SrClubChampionship) && ($players[0]->Extra == "CH")){
	echo '<div id="content-container" class="entry-content">' . PHP_EOL;;
	echo '<p style = "text-align: center;">' . PHP_EOL;;
	echo "Championship flight players only sign up as singles." . PHP_EOL;
    echo '</div><!-- #content-container -->' . PHP_EOL;
	
	if (isset ( $connection )) {
		$connection->close ();
	}
	get_footer ();
	return;
}

//var_dump($_POST);

$errorAccessCode1 = null;
$errorAccessCode2 = null;
$mergeError = null;
$signup2 = null;
$selectGroupMessage = "Select the players to add to your group:";

if (isset ( $_POST['AccessCode1'] ) || isset ( $_POST['AccessCode2'] ) || isset($_POST['MergeGroup'])) {
	
	$accessCode1 = trim($_POST['AccessCode1']);
	
	if(empty($accessCode1)){
		$errorAccessCode1 = "Fill in the access code for your group";
	}
	else if($signup->AccessCode != $accessCode1){
		$errorAccessCode1 = "Invalid access code";
	}
	
	if(!isset($_POST['MergeGroup'])){
		$mergeError = $selectGroupMessage;
	}
	else {
		// get signup for 2nd group
		$signup2 = GetSignup($connection, $_POST['MergeGroup']);
		
		if(empty($signup2)){
			die("There is no data for signup key (group to add): " . $_POST['MergeGroup']);
		}
		
		$accessCode2 = trim($_POST['AccessCode2']);
	
		if(empty($accessCode2)){
			$errorAccessCode2 = "Fill in the access code for the group to add";
		}
		else if($signup2->AccessCode != $accessCode2){
			$errorAccessCode2 = "Invalid access code";
		}
	}
}

$hasError = false;
if (!empty($errorAccessCode1) || ! empty ($errorAccessCode2) || !empty($mergeError)) {
	$hasError = true;
}

echo '<div id="content-container" class="entry-content">' . PHP_EOL;
echo '<div id="content" role="main">' . PHP_EOL;
echo '<h2 class="entry-title">Add Players</h2>' . PHP_EOL;
echo '<h3>' . $t->Name . '</h3>' . PHP_EOL;

// If this page has not been filled in or there is an error, show the form
if ($hasError || !isset ( $_POST ['AccessCode1'] )) {

	if($signupHasPaid){
		echo '<p>You can add players to your group. The players must have already signed up and paid.</p>' . PHP_EOL;
	}
	else {
		echo '<p>You can add players to your group. The players must have already signed up and not paid yet.</p>' . PHP_EOL;
	}
	echo '<p>Select a group to add, fill in the access codes for both groups, and click Submit</p>' . PHP_EOL;
	
	$maxSize = 4 - count($players);
	$potentialMergeGroups = GetSignupsOfSize($connection, $tournamentKey, $maxSize, $signupKey, $signupHasPaid, ($t->ClubChampionship || $t->SrClubChampionship));
	
	if(count($potentialMergeGroups) == 0){
		if(count($players) == 4){
			echo '<p style="color:red;">Your group is already full.</p>' . PHP_EOL;
		} else {
			echo '<p style="color:red;">There are no groups of size ' . $maxSize . ' or less available for merging.</p>' . PHP_EOL;
		}
	} else {
		echo '<form name="input" method="post">' . PHP_EOL;
		$accessCode1 = "";
		if(isset($_POST['AccessCode1'])){
			$accessCode1 = $_POST['AccessCode1'];
		}
		echo "Your Group's Access Code: " . '<input type="text" name="AccessCode1" maxlength="4" size="4" value="' .  $accessCode1 . '"><br><br>' . PHP_EOL;
		if(!empty($errorAccessCode1)){
			echo '<p style="color:red;">' . $errorAccessCode1 . '</p>' . PHP_EOL;
		}
		
		$accessCode2 = "";
		if(isset($_POST['AccessCode2'])){
			$accessCode2 = $_POST['AccessCode2'];
		}
		echo "Added Group's Access Code: " . '<input type="text" name="AccessCode2" maxlength="4" size="4" value="' .  $accessCode2 . '"><br><br>' . PHP_EOL;
		if(!empty($errorAccessCode2)){
			echo '<p style="color:red;">' . $errorAccessCode2 . '</p>' . PHP_EOL;
		}
		
		if(!empty($mergeError)){
			echo '<p style="color:red;">' . $mergeError . '</p>' . PHP_EOL;
		} else {
			echo '<p>' . $selectGroupMessage . '</p>' . PHP_EOL;
		}
		echo '<select name="MergeGroup" style="margin-left:50px" size=' . count($potentialMergeGroups) . '>' . PHP_EOL;
		
		for($i = 0; $i < count ( $potentialMergeGroups ); ++ $i) {
			echo '<option ';
			if((isset($signup2) && ($signup2->SignUpKey == $potentialMergeGroups [$i]->SignUpKey)) || (count($potentialMergeGroups) == 1)){
				echo 'selected="selected" ';
			}
			echo 'value="' . $potentialMergeGroups [$i]->SignUpKey . '">' . $potentialMergeGroups [$i]->PlayerNames . '</option>' . PHP_EOL;
		}
		
		echo '</select><br><br>' . PHP_EOL;
	
		echo '<input type="submit" value="Submit"> <br> <br>' . PHP_EOL;
		echo '</form>' . PHP_EOL;
	}
} else {
	// Make the modifications
	$players2 = GetPlayersForSignUp($connection, $signup2->SignUpKey);
	
	if(count($players2) == 0){
		die ("There are no players for added group signup code " . $signup2->SignUpKey);
	}
	
	// Update the signup position for the players merging into the
	// group. Position is 0 based.
	$position = count($players);
	for($i = 0; $i < count($players2); ++$i){
		UpdateSignupPlayer($connection, $signup2->SignUpKey, $players2[$i]->GHIN, 'Position', $position, 'i');
		++$position;
		// Change the signup key to be the key for the 1st group
		UpdateSignupPlayer($connection, $signup2->SignUpKey, $players2[$i]->GHIN, 'SignUpKey', $signupKey, 'i');
	}
	
	// Update the payment and payment due to be the sum of the 2 groups
	UpdateSignup($connection, $signupKey, 'Payment', $signup->Payment + $signup2->Payment, 'd');
	UpdateSignup($connection, $signupKey, 'PaymentDue', $signup->PaymentDue + $signup2->PaymentDue, 'd');

	// If PaymentEnabled is true for group 2 and not for group 1, should we change PaymentEnabled to true
	// for the merged group?
	
	// Remove the 2nd signup
	DeleteSignup($connection, $signup2->SignUpKey);
	
	echo '<p>The new players have been added to your group.</p>' . PHP_EOL;
	
	// Save original group for email below
	$players1 = $players;
	$players = GetPlayersForSignUp($connection, $signupKey);
	
	if(($t->TeamSize == 2) && ($t->SCGAQualifier)){
		if(count($players) >= 2){
			if($players[0]->Extra !== $players[1]->Extra){
				echo '<p style="color:red;">The flight choices for players 1 & 2 do not match. Use the Modify action from the View Signups page to update the flight choice.' . PHP_EOL;
			}
		}
		if(count($players) == 4){
			if($players[2]->Extra !== $players[3]->Extra){
				echo '<p style="color:red;">The flight choices for players 3 & 4 do not match. Use the Modify action from the View Signups page to update the flight choice.' . PHP_EOL;
			}
		}
	}
	
	echo '<p>New group:</p><p>' . PHP_EOL;
	
	for($i = 0; $i < count($players); ++$i){
		echo '&nbsp;&nbsp;&nbsp;' . $players[$i]->LastName . '<br>' . PHP_EOL;
	}
	echo '</p>' . PHP_EOL;

	SendMergeEmail($t, $players1, $players2, $doNotReplyEmailAddress, $doNotReplyEmailPassword);
} // end of else clause

echo '<p>' . PHP_EOL;
echo '<a href="signups.php?tournament=' . $tournamentKey . '">View Signups</a>' . PHP_EOL;
echo '</p>' . PHP_EOL;
echo '</div><!-- #content -->' . PHP_EOL;
echo '</div><!-- #content-container -->' . PHP_EOL;


if (isset ( $connection )) {
	$connection->close ();
}

get_footer ();

function GetSignupsOfSize($connection, $tournamentKey, $maxSize, $signupKey, $signupHasPaid, $isClubChampionship)
{
	$signups = array();
	if($signupHasPaid){
		$samePaymentGroupSignups = GetSignups ( $connection, $tournamentKey, ' AND `Payment` >= `PaymentDue` ORDER BY `SubmitKey` DESC' );
	}
	else {
		$samePaymentGroupSignups = GetSignups ( $connection, $tournamentKey, ' AND `Payment` = 0 ORDER BY `SubmitKey` DESC' );
	}
	
	for($i = 0; $i < count ( $samePaymentGroupSignups ); ++ $i) {
		if($signupKey != $samePaymentGroupSignups [$i]->SignUpKey){
			$playersSignedUp = GetPlayersForSignUp ( $connection, $samePaymentGroupSignups [$i]->SignUpKey );
			
			if($isClubChampionship && (count($playersSignedUp) > 0) && ($playersSignedUp [0]-> Extra == "CH")){
				// skip over championship players since they sign up as singles
			}
			else {
				if(count($playersSignedUp) <= $maxSize){
					$playerNames = null;
					for($p = 0; $p < count ( $playersSignedUp ); ++ $p) {
						if (! empty ( $playerNames )) {
							$playerNames = $playerNames . " --- ";
						}
						$playerNames = $playerNames . " " . $playersSignedUp [$p]->LastName;
					}
					
					$m = new MergeSignUpClass();
					$m->SignUpKey = $samePaymentGroupSignups [$i]->SignUpKey;
					$m->PlayerNames = '&nbsp;&nbsp;' . $playerNames . '&nbsp;&nbsp;'; // add spaces for listbox
					$signups[] = $m;
				}
			}
		}
	}
	
	return $signups;
}
?>