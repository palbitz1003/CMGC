<?php
use PHPMailer\PHPMailer\PHPMailer;
use PHPMailer\PHPMailer\Exception;
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_functions.php';
require '/home/' . $accountUser . '/PHPMailer/src/Exception.php';
require '/home/' . $accountUser . '/PHPMailer/src/PHPMailer.php';
require '/home/' . $accountUser . '/PHPMailer/src/SMTP.php';

class SignUpClass {
	public $SignUpKey;
	public $TournamentKey;
	public $SubmitDate;
	public $RequestedTime;
	public $Payment;
	public $PaymentDue;
	public $PaymentDateTime;
	public $AccessCode;
	public $PayerName;
	public $PayerEmail;
	public $PaymentEnabled;
}

class PlayerSignUpClass {
	public $SignUpKey;
	public $TournamentKey;
	public $Position;
	public $GHIN;
	public $LastName;
	public $Extra;
}

class SignUpWaitingListClass {
	public $TournamentKey;
	public $Position;
	public $GHIN1;
	public $Name1;
	public $GHIN2;
	public $Name2;
	public $GHIN3;
	public $Name3;
	public $GHIN4;
	public $Name4;
}

class RosterEntry {
	public $GHIN;
	public $LastName;
	public $FirstName;
	public $Active;
	public $Email;
	public $BirthDate;
	public $DateAdded;
	public $MembershipType;
	public $SignupPriority;
}

function InsertSignUp($connection, $tournamentKey, $requestedTime, $entryFees, $accessCode, $paymentEnabled) {
	$sqlCmd = "INSERT INTO `SignUps` VALUES (NULL, ?, ?, ?, ?, ?, NULL, ?, ?, ?, ?)";
	$insert = $connection->prepare ( $sqlCmd );
	
	if (! $insert) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	$date = date ( 'Y-m-d H:i:s' );
	$payment = 0.0;
	$payerName = "";
	$payerEmail = "";
	// Convert boolean to bit value for SQL
	if($paymentEnabled === true){
		$paymentEnabled = 1;
	}
	if($paymentEnabled === false){
		$paymentEnabled = 0;
	}
	if (! $insert->bind_param ( 'issddsssi', $tournamentKey, $date, $requestedTime, $payment, $entryFees, $accessCode, $payerName, $payerEmail, $paymentEnabled )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}
	
	if (! $insert->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
	
	// echo 'insert id is: ' . $insert->insert_id . '<br>';
	return $insert->insert_id;
}
function DeleteSignup($connection, $submitKey) {
	$sqlCmd = "DELETE FROM `SignUps` WHERE `SubmitKey` = ?";
	$clear = $connection->prepare ( $sqlCmd );

	if (! $clear) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}

	if (! $clear->bind_param ( 'i', $submitKey )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}

	if (! $clear->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
}
function InsertSignUpPlayers($connection, $tournamentKey, $submitKey, $GHIN, $Name, $extra) {
	$sqlCmd = "INSERT INTO `SignUpsPlayers` VALUES (?, ?, ?, ?, ?, ?)";
	$insert = $connection->prepare ( $sqlCmd );
	
	if (! $insert) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	// var_dump($GHIN);
	// echo "<br>";
	$playersAdded = 0;
	$position = 0;
	for($i = 0; $i < count ( $GHIN ); ++ $i) {
		if (isset ( $GHIN [$i] ) && (strlen($GHIN [$i]) > 0) ) {
			++ $playersAdded;
			if (! $insert->bind_param ( 'iiiiss', $submitKey, $tournamentKey, $position, $GHIN [$i], $Name [$i], $extra[$i] )) {
				die ( $sqlCmd . " bind_param failed: " . $connection->error );
			}
			
			if (! $insert->execute ()) {
				die ( $sqlCmd . " execute failed: " . $connection->error );
			}
			++ $position;
			// echo "inserted " . $GHIN[$i] . " for key " . $submitKey . "<br>";
		}
	}
	
	return $playersAdded;
}
/*
 * Get the record for a signed up player by GHIN.  There must only be 1 record.
 */
function GetPlayerSignUp($connection, $tournamentKey, $playerGHIN) {
	
	$sqlCmd = "SELECT * FROM `SignUpsPlayers` WHERE `TournamentKey` = ? AND `PlayerGHIN` = ?";
	$player = $connection->prepare ( $sqlCmd );
	
	if (! $player) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	if (! $player->bind_param ( 'ii', $tournamentKey, $playerGHIN )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}
	
	if (! $player->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
	
	$player->bind_result ( $key, $tournament, $position, $GHIN, $LastName, $extra );
	
	$playerSignUp = null;

	$count = 1;
	 while($player->fetch()) {
	 	if($count > 1){
	 		die('Player ' . $playerGHIN . ' is signed up more than once');
	 	}
	 	//echo 'found player<br>';
	 	$playerSignUp = new PlayerSignUpClass();
	 	$playerSignUp->SignUpKey = $key;
	 	$playerSignUp->TournamentKey = $tournamentKey;
	 	$playerSignUp->GHIN = $GHIN;
	 	$playerSignUp->LastName = $LastName;
	 	$playerSignUp->Extra = $extra;
	 	$count++;
	 }
	 //if(!isset($playerSignUp)) { echo 'did not find player<br>'; }
	
	$player->close ();
	
	return $playerSignUp;
}

/*
 * Get the record for a signed up player by name.  There must only be 1 record.
 */
function GetPlayerSignUpByName($connection, $tournamentKey, $playerName) {
	
	$sqlCmd = "SELECT * FROM `SignUpsPlayers` WHERE `TournamentKey` = ? AND `LastName` = ?";
	$player = $connection->prepare ( $sqlCmd );
	
	if (! $player) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	if (! $player->bind_param ( 'is', $tournamentKey, $playerName )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}
	
	if (! $player->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
	
	$player->bind_result ( $key, $tournament, $position, $GHIN, $LastName, $extra );
	
	$playerSignUp = null;

	$count = 1;
	 while($player->fetch()) {
	 	if($count > 1){
	 		die('Player ' . $playerName . ' is signed up more than once');
	 	}
	 	//echo 'found player<br>';
	 	$playerSignUp = new PlayerSignUpClass();
	 	$playerSignUp->SignUpKey = $key;
	 	$playerSignUp->TournamentKey = $tournamentKey;
	 	$playerSignUp->GHIN = $GHIN;
	 	$playerSignUp->LastName = $LastName;
	 	$playerSignUp->Extra = $extra;
	 	$count++;
	 }
	 //if(!isset($playerSignUp)) { echo 'did not find player<br>'; }
	
	$player->close ();
	
	return $playerSignUp;
}

/*
 * Return true or false whether a player is signed up
 */
function IsPlayerSignedUp($connection, $tournamentKey, $playerGHIN) {
	if (empty ( $playerGHIN )) {
		return false;
	}
	
	$playerSignUp = GetPlayerSignUp($connection, $tournamentKey, $playerGHIN);
	
	return isset($playerSignUp);
}
/*
 * Remove a player from the signup list
 */
function RemoveSignedUpPlayer($connection, $tournamentKey, $playerGHIN, $lastName) {

	$sqlCmd = "DELETE FROM `SignUpsPlayers` WHERE `TournamentKey` = ? AND `PlayerGHIN` = ? AND `LastName` = ?";
	$player = $connection->prepare ( $sqlCmd );

	if (! $player) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}

	if (! $player->bind_param ( 'iis', $tournamentKey, $playerGHIN, $lastName )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}

	if (! $player->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
	
	//echo 'rows deleted: ' . mysqli_affected_rows($connection) . "<br>";

	$player->close ();
}
/*
 * Get all the players for a signup.
 */
function GetPlayersForSignUp($connection, $signUpKey){
	$sqlCmd = "SELECT * FROM `SignUpsPlayers` WHERE SignUpKey = ? ORDER BY `Position` ASC";
	$signups = $connection->prepare ( $sqlCmd );
	
	if (! $signups) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	if (! $signups->bind_param ( 'i', $signUpKey )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}
	
	if (! $signups->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
	
	$signups->bind_result ( $key, $tournament, $position, $GHIN, $LastName, $extra );

	$players = Array();
	while ( $signups->fetch () ) {
		$playerSignUp = new PlayerSignUpClass();
	 	$playerSignUp->SignUpKey = $key;
	 	$playerSignUp->TournamentKey = $tournament;
	 	$playerSignUp->Position = $position;
	 	$playerSignUp->GHIN = $GHIN;
	 	$playerSignUp->LastName = $LastName;
	 	$playerSignUp->Extra = $extra;
	 	$players[] = $playerSignUp;
	}
	
	$signups->close();
	
	return $players;
}

/*
 * Get the player count for a tournament
 */
function GetPlayerCountForTournament($connection, $tournamentKey){
	// We're just getting a count, so "SELECT TournamentKey" instead of "SELECT *"
	$sqlCmd = "SELECT TournamentKey FROM `SignUpsPlayers` WHERE TournamentKey = ?";
	$signups = $connection->prepare ( $sqlCmd );
	
	if (! $signups) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	if (! $signups->bind_param ( 'i', $tournamentKey )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}
	
	if (! $signups->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}

	// There must be a better way than this, but all my
	// attempts failed ....
	$count = 0;
	while ( $signups->fetch () ) {
		$count++;
	}
	return $count;
	
}

/*
 * Decide if payment is enabled given the list of players
 */
function DecideIfPaymentEnabled($connection, $previousTournamentKey, $GHIN, $logFile)
{
	for($i = 0; $i < count ( $GHIN ); ++ $i) {
		if (isset ( $GHIN [$i] ) && (strlen($GHIN [$i]) > 0) ) {
			$rosterEntry = GetRosterEntry($connection, $GHIN [$i]);
			// Signup priority B is board members. They can pay immediately.
			if(!empty($rosterEntry) && ($rosterEntry->SignupPriority === "B")){
				if(!empty($logFile)){
					error_log(date ( '[Y-m-d H:i e] ' ) . "Board member signed up: ". $rosterEntry->LastName . PHP_EOL, 3, $logFile);
				}
				return true;
			}
		}
	}

	if($previousTournamentKey != -1){

		$waitingList = GetSignUpWaitingList($connection, $previousTournamentKey);

		for($i = 0; $i < count ( $waitingList ); ++ $i) {
			for($j = 0; $j < count ($GHIN); ++$j){
				if (isset ( $GHIN [$j] ) && (strlen($GHIN [$j]) > 0) ) {
					$GHINint = intval($GHIN[$j]);
					if($GHINint != 0){
						if((($waitingList[$i]->GHIN1 != 0) && ($waitingList[$i]->GHIN1 === $GHINint)) ||
							(($waitingList[$i]->GHIN2 != 0) && ($waitingList[$i]->GHIN2 === $GHINint)) ||
							(($waitingList[$i]->GHIN3 != 0) && ($waitingList[$i]->GHIN3 === $GHINint)) ||
							(($waitingList[$i]->GHIN4 != 0) && ($waitingList[$i]->GHIN4 === $GHINint))){
								if(!empty($logFile)){
									// While the waiting list supports 4 players in each entry, in reality, only the 1st is used
									error_log(date ( '[Y-m-d H:i e] ' ) . "Waiting list member signed up: ". $waitingList[$i]->Name1 . PHP_EOL, 3, $logFile);
								}
								return true;
						}
					}
				}
			}
		}
	}

	return false;
}

/*
 * Get the roster data for a player
 */
function GetRosterEntry($connection, $playerGHIN) {
	if (empty ( $playerGHIN )) {
		return '';
	}

	$sqlCmd = "SELECT * FROM `Roster` WHERE `GHIN` = ? ";
	$player = $connection->prepare ( $sqlCmd );

	if (! $player) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}

	if (! $player->bind_param ( 'i', $playerGHIN )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}

	if (! $player->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}

	$player->bind_result ( $GHIN, $lastName, $firstName, $active, $email, $birthDate, $dateAdded, $membershipType, $signupPriority );

	if ( $player->fetch () ) {
		$rosterEntry = new RosterEntry();
		$rosterEntry->LastName = $lastName;
		$rosterEntry->FirstName = $firstName;
		$rosterEntry->Active = $active;
		$rosterEntry->Email = $email;
		$rosterEntry->BirthDate = $birthDate;
		$rosterEntry->DateAdded = $dateAdded;
		$rosterEntry->MembershipType = $membershipType;
		$rosterEntry->SignupPriority = $signupPriority;
		return $rosterEntry;
	}
	else {
		return null;
	}
	
	$player->close();
}
function GetSignups($connection, $tournamentKey, $sqlClause) {

	$sqlCmd = "SELECT * FROM `SignUps` WHERE `TournamentKey` = ? " . $sqlClause;
	$signups = $connection->prepare ( $sqlCmd );
	//echo $sqlCmd . '<br>';

	if (! $signups) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}

	if (! $signups->bind_param ( 'i', $tournamentKey )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}

	if (! $signups->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}

	$signups->bind_result ( $key, $tournament, $date, $requestedTime, $payment, $paymentDue, $paymentDateTime, $accessCode, $payerName, $payerEmail, $paymentEnabled );

	while ( $signups->fetch () ) {
		$signUpObj = new SignUpClass ();
		$signUpObj->SignUpKey = $key;
		$signUpObj->TournamentKey = $tournament;
		$signUpObj->SubmitDate = $date;
		$signUpObj->RequestedTime = $requestedTime;
		$signUpObj->Payment = $payment;
		$signUpObj->PaymentDue = $paymentDue;
		$signUpObj->PaymentDateTime = $paymentDateTime;
		$signUpObj->AccessCode = $accessCode;
		$signUpObj->PayerName = $payerName;
		$signUpObj->PayerEmail = $payerEmail;
		$signUpObj->PaymentEnabled = boolval($paymentEnabled);  // convert bit value to true/false
		$signUpArray [] = $signUpObj;
	}

	$signups->close ();
	
	return $signUpArray;
}
function GetSignup($connection, $submitKey){

	$sqlCmd = "SELECT * FROM `SignUps` WHERE `SubmitKey` = ?";
	$signups = $connection->prepare ( $sqlCmd );
	
	if (! $signups) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	if (! $signups->bind_param ( 'i', $submitKey )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}
	
	if (! $signups->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
	
	$signups->bind_result ( $key, $tournament, $date, $requestedTime, $payment, $paymentDue, $paymentDateTime, $accessCode, $payerName, $payerEmail, $paymentEnabled );
	
	$signUpObj = null;
	if($signups->fetch ()){
		$signUpObj = new SignUpClass ();
		$signUpObj->SignUpKey = $key;
		$signUpObj->TournamentKey = $tournament;
		$signUpObj->SubmitDate = $date;
		$signUpObj->RequestedTime = $requestedTime;
		$signUpObj->Payment = $payment;
		$signUpObj->PaymentDue = $paymentDue;
		$signUpObj->PaymentDateTime = $paymentDateTime;
		$signUpObj->AccessCode = $accessCode;
		$signUpObj->PayerName = $payerName;
		$signUpObj->PayerEmail = $payerEmail;
		$signUpObj->PaymentEnabled = boolval($paymentEnabled);  // convert bit value to true/false
	}

	$signups->close();
	
	return $signUpObj;
}
function UpdateSignup($connection, $submitKey, $field, $value, $paramType){

	$sqlCmd = "UPDATE `SignUps` SET `" . $field . "`= ? WHERE `SubmitKey` = ?";
	$update = $connection->prepare ( $sqlCmd );

	if (! $update) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}

	if (! $update->bind_param ( $paramType . 'i',  $value, $submitKey)) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}

	if (! $update->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
	$update->close ();
}
function UpdateSignupPlayer($connection, $submitKey, $ghin, $field, $value, $paramType){

	$sqlCmd = "UPDATE `SignUpsPlayers` SET `" . $field . "`= ? WHERE `SignUpKey` = ? AND `PlayerGHIN` = ?";
	$update = $connection->prepare ( $sqlCmd );

	if (! $update) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}

	if (! $update->bind_param ( $paramType . 'ii',  $value, $submitKey, $ghin)) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}

	if (! $update->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
	$update->close ();
}
function ShowSignups($connection, $tournamentKey) {

	$t = GetTournament ( $connection, $tournamentKey );

	if(empty($t)){
		die("Tournament " . $tournamentKey . " does not exist");
	}
	
	echo '<h2 style="text-align:center">' . $t->Name . ' Signups</h2>' . PHP_EOL;
	
	echo '<table style="border: none; margin-left:auto;margin-right:auto;">' . PHP_EOL;
	echo '<tbody>' . PHP_EOL;
	
	if($t->SrClubChampionship){
		echo '<tr><td style="border: none">' . PHP_EOL;
		echo 'F1 = Flight 1 (under 60)<br>' . PHP_EOL;
		echo 'F2 = Flight 2 (60-69)<br>' . PHP_EOL;
		echo 'F3 = Flight 3 (70 and older)<br>' . PHP_EOL;
		echo 'CH = Championship Flight (55 and older)<br><br>' . PHP_EOL;
		echo '</td></tr>' . PHP_EOL;
	}
	else if($t->ClubChampionship){
		echo '<tr><td style="border: none">' . PHP_EOL;
		//echo 'F1 = Handicap-based Flights (actual flight determined later)<br>' . PHP_EOL;
		echo 'CH = Championship Flight<br><br>' . PHP_EOL;
		echo '</td></tr>' . PHP_EOL;
	}

	$count = GetPlayerCountForTournament($connection, $tournamentKey);
	/*
	if(($count > 0) && ($t->MaxSignups > 0)){
		echo '<tr><td style="border: none">';
		if($count >= $t->MaxSignups){
			echo '<p>This tournament is full.</p>';
			echo '<p>Note: Signing up does not guarantee that you are in the tournament. ';
			echo 'We try to match the number of signups with the available tee times. ';
			echo 'When the tee times are assigned and we are over-subscribed, a random draw decides which players do not get a tee time. ';
			echo 'Those players are refunded their signup fee.</p>' . PHP_EOL;
		} else {
			
			//echo 'There are ' . $count . ' players signed up. Signups will close at ' . $t->MaxSignups . ' players.';
		}
		echo '</td></tr><tr><td style="border: none"></td></tr>' . PHP_EOL;
	}
	*/
	echo '<tr><td style="border: none">There are ' . $count . ' players signed up.</td></tr><tr><td style="border: none"></td></tr>';
	
	$signUpArray = GetSignups ( $connection, $tournamentKey, ' AND `Payment` < `PaymentDue` ORDER BY `SubmitKey` DESC' );
	
	if(!empty($signUpArray)){
		echo '<tr><td style="border: none">';
		echo 'These players are part of the blind draw to decide who plays.';
		echo '</td></tr>' . PHP_EOL;
		//echo '<tr><td style="border: none">';
		//echo 'If you have completed payment, but it is not yet recorded, be patient as PayPal may not have notified us yet. ';
		//echo 'Notify the tournament director if payment is not recorded after 24 hours.';
		//echo '</td></tr>' . PHP_EOL;
		echo '<tr><td style="border: none">' . PHP_EOL;
		ShowSignupsTable($connection, $tournamentKey, $signUpArray, $t);
		echo '</td></tr>' . PHP_EOL;
	}
	
	$signUpArray = GetSignups ( $connection, $tournamentKey, ' AND `Payment` >= `PaymentDue` ORDER BY `SubmitKey` DESC' );
	
	if(!empty($signUpArray)){
		if($t->RequirePayment){
			echo '<tr><td style="border: none">These players have signed up and have paid.</td></tr>';
		}
		else {
			echo '<tr><td style="border: none">These players have signed up (no payment required).</td></tr>';
		}
		echo '<tr><td style="border: none">' . PHP_EOL;
		ShowSignupsTable($connection, $tournamentKey, $signUpArray, $t);
		echo '</td></tr>' . PHP_EOL;
	}
	echo '</tbody></table>' . PHP_EOL;
}

function ShowSignupsTable($connection, $tournamentKey, $signUpArray, $t)
{
	echo '<table style="border: none; width: 100%">' . PHP_EOL;
	echo '<thead><tr class="header"><th style="width:10%">Requested Time</th><th style="width:70%">Players</th><th style="width:20%;text-align:center">Actions</th></tr></thead>' . PHP_EOL;
	echo '<tbody>' . PHP_EOL;
	
	for($i = 0; $i < count ( $signUpArray ); ++ $i) {
		$playersSignedUp = GetPlayersForSignUp ( $connection, $signUpArray [$i]->SignUpKey );
		
		$players = null;
		for($p = 0; $p < count ( $playersSignedUp ); ++ $p) {
			if (! empty ( $players )) {
				$players = $players . " --- ";
			}
			$players = $players . " " . $playersSignedUp [$p]->LastName;
			if($playersSignedUp [$p]->Extra == "Flight1"){
				$players = $players . ' (F1)';
			}
			else if($playersSignedUp [$p]->Extra == "Flight2"){
				$players = $players . ' (F2)';
			}
			else if(!empty($playersSignedUp [$p]->Extra)) {
				if($t->ClubChampionship && ($playersSignedUp [$p]->Extra == "F1")){
					// don't show anything since flight is not yet determined
				} else {
					$players = $players . ' (' . $playersSignedUp [$p]->Extra . ')';
				}
			}
		}
		
		if ($players) {
			if (($i % 2) == 0) {
				echo '<tr class="d1">';
			} else {
				echo '<tr class="d0">';
			}
			echo '<td>' . $signUpArray [$i]->RequestedTime . '</td>';
			echo '<td>' . $players . '</td>';
			echo '<td>';
			$needToPay = $signUpArray [$i]->Payment < $signUpArray [$i]->PaymentDue;
			if($needToPay && $signUpArray [$i]->PaymentEnabled) {
				echo '<a href="' . $script_folder_href . 'pay.php?tournament=' . $tournamentKey . '&signup=' . $signUpArray [$i]->SignUpKey . '">Pay</a>&nbsp;&nbsp;&nbsp;';
			}
			else {
				// Place blanks where the "Pay" link would be, using approximately the same length
				echo '&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;';
			}
			echo '<a href="' . $script_folder_href . 'signup_remove_players.php?tournament=' . $tournamentKey . '&amp;signup=' . $signUpArray [$i]->SignUpKey . '">Remove</a>&nbsp;&nbsp;&nbsp;';
			// Since some players can sign up their group if they are on the waiting list, disable replace, because that would allow someone
			// to sign up off the waiting list and the replace the waiting list player.
			echo '<a href="' . $script_folder_href . 'signup_replace_players.php?tournament=' . $tournamentKey . '&amp;signup=' . $signUpArray [$i]->SignUpKey . '">Replace</a>&nbsp;&nbsp;&nbsp;';
			echo '<a href="' . $script_folder_href . 'signup_modify.php?tournament=' . $tournamentKey . '&amp;signup=' . $signUpArray [$i]->SignUpKey . '">Modify</a>';
			echo '&nbsp;&nbsp;&nbsp;<a href="' . $script_folder_href . 'signup_add.php?tournament=' . $tournamentKey . '&amp;signup=' . $signUpArray [$i]->SignUpKey . '">Add</a>';
			echo '</td></tr>' . PHP_EOL;
		}
	}
	
	echo '</tbody></table>' . PHP_EOL;
}
function InsertSignUpWaitingListEntry($connection, $signUpWaitingList){
	$sqlCmd = "INSERT INTO `SignUpsWaitingList` VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
	$insert = $connection->prepare ( $sqlCmd );
	
	if (! $insert) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	if (! $insert->bind_param ( 'iiisisisis', $signUpWaitingList->TournamentKey, $signUpWaitingList->Position, $signUpWaitingList->GHIN1, $signUpWaitingList->Name1, $signUpWaitingList->GHIN2, $signUpWaitingList->Name2, $signUpWaitingList->GHIN3, $signUpWaitingList->Name3, $signUpWaitingList->GHIN4, $signUpWaitingList->Name4 )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}
	
	if (! $insert->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
}

function GetSignUpWaitingList($connection, $tournamentKey){
	$sqlCmd = "SELECT * FROM `SignUpsWaitingList` WHERE TournamentKey = ? ORDER BY `Position` ASC";
	$entries = $connection->prepare ( $sqlCmd );
	
	if (! $entries) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	if (! $entries->bind_param ( 'i', $tournamentKey )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}
	
	if (! $entries->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
	
	$entries->bind_result ( $key, $position, $ghin1, $name1, $ghin2, $name2, $ghin3, $name3, $ghin4, $name4 );
	
	$waitingList = array();
	while ( $entries->fetch () ) {
		$entry = new SignUpWaitingListClass();
		$entry->TournamentKey = $tournamentKey;
		$entry->Position = $position;
		$entry->GHIN1 = $ghin1;
		$entry->Name1 = $name1;
		$entry->GHIN2 = $ghin2;
		$entry->Name2 = $name2;
		$entry->GHIN3 = $ghin3;
		$entry->Name3 = $name3;
		$entry->GHIN4 = $ghin4;
		$entry->Name4 = $name4;
		$waitingList[] = $entry;
	}
	
	$entries->close ();
	
	return $waitingList;
}

function GetPayPalDetails($connection, $tournamentFee){
	$sqlCmd = "SELECT * FROM `PayPal` WHERE `TournamentFee` = ?";
	$payPal = $connection->prepare ( $sqlCmd );

	if (! $payPal) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}

	if (! $payPal->bind_param ( 'i', $tournamentFee )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}

	if (! $payPal->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}

	$payPal->bind_result ( $payPalButton, $fee, $processingFee, $players1, $players2, $players3, $players4 );

	$details = new PayPalDetails();
	if($payPal->fetch ()){
		$details->PayPayButton = $payPalButton;
		$details->TournamentFee = $fee;
		$details->ProcessingFee = $processingFee;
		$details->Players1 = $players1;
		$details->Players2 = $players2;
		$details->Players3 = $players3;
		$details->Players4 = $players4;
	}

	$payPal->close ();

	return $details;
}

function ShowPayment($web_site, $ipn_file, $script_folder_href, $connection, $tournament, $submitKey, $accessCode, $testMode)
{
	$signup = GetSignup($connection, $submitKey);
	if(empty($signup)){
		die("Unable to find signup key " . $submitKey);
	}

	if(!$signup->PaymentEnabled){
		die("Payment is not enabled for this signup");
	}
	
	$playersSignedUp = GetPlayersForSignUp($connection, $submitKey);
	
	$players = "";
	$playersNameOnly = "";
	$playerCount = 0;
	for($i = 0; $i < count ( $playersSignedUp ); ++ $i) {
		if (! empty ( $playersSignedUp [$i] )) {
			++$playerCount;
			// Save names and GHIN for sending to PayPal and PayPal sending back
			$players = $players . $playersSignedUp[$i]->LastName . " (" . $playersSignedUp[$i]->GHIN . ") ";

			// Save names only to display on web page
			if(!empty($playersNameOnly)){
				$playersNameOnly = $playersNameOnly . " --- ";
			}
			$playersNameOnly = $playersNameOnly . $playersSignedUp[$i]->LastName;
		}
	}
	if(empty($players)){
		die("No players found for submit key " . $submitKey);
	}

	if($signup->Payment > 0){
		echo "Payment of $" . $signup->Payment . " has been received for: " . $playersNameOnly;
		return;
	}
	
	$cost = $tournament->Cost;
	if($testMode){
		$cost = 3;
	}
	$paypalDetails = GetPayPalDetails($connection, $cost);
	if(empty($paypalDetails)){
		die("Unable to get PayPal details for tournament cost " . $cost);
	}
	
	$calculatedPlayerCount = $signup->PaymentDue / ($paypalDetails->TournamentFee + $paypalDetails->ProcessingFee);
	
	if($calculatedPlayerCount != $playerCount){
		die("Internal error: There are " . $playerCount . " players signed up, but the amount due is only for " . $calculatedPlayerCount . " players.  Contact the tournament director.");
	}
	
	$payPalComboBoxChoice = null;
	switch($calculatedPlayerCount){
		case 1:
			$payPalComboBoxChoice = $paypalDetails->Players1;
			break;
		case 2:
			$payPalComboBoxChoice = $paypalDetails->Players2;
			break;
		case 3:
			$payPalComboBoxChoice = $paypalDetails->Players3;
			break;
		case 4:
			$payPalComboBoxChoice = $paypalDetails->Players4;
			break;
		default:
			die('Unexpected number of players signed up: ' . $calculatedPlayerCount . ". Expected 1-4.");
	}
	
	echo '<h2 class="entry-title" style="text-align:center">' . $tournament->Name . ' Entry Fees</h2>' . PHP_EOL;
	if(!empty($accessCode)){
		echo '<p>Your signup data has been saved.  Here is your access code to make changes to your signup. Save this code for later!</p>';
		echo '<p style="text-align: center;"><b>' . $accessCode . '</b> </p>' . PHP_EOL;
	}
	echo '<p>You must pay the tournament fees to complete your signup.  You are not signed up until your payment is complete!</p>' . PHP_EOL;
	echo '<p>You are paying for these players: ' . $playersNameOnly . '</p>' . PHP_EOL;
	echo "<p>The link below takes you to PayPal to make your payment.  You can pay with credit card even if you do not have a PayPal account. No credit card or account information is kept on the Coronado Men's Golf web site.</p>" . PHP_EOL;
	echo '<p style="text-align: center;"><b>Entry Fees: $' . number_format( $signup->PaymentDue, 2) . '</b></p>' . PHP_EOL;
	
	echo '<form style="text-align:center" action="https://www.paypal.com/cgi-bin/webscr" method="post" target="_top">' . PHP_EOL;
	echo '<input type="hidden" name="cmd" value="_s-xclick">' . PHP_EOL;
	echo '<input type="hidden" name="hosted_button_id" value="' . $paypalDetails->PayPayButton . '">' . PHP_EOL;
	echo '<input type="hidden" name="item_name" value="' . $tournament->Name . '">' . PHP_EOL;
	echo '<input type="hidden" name="custom" value="' . $tournament->TournamentKey . ';' . $submitKey . ';' . $players . '">' . PHP_EOL;
	echo '<input type="hidden" name="on0" value="Entry Fees">' . PHP_EOL;
	echo '<input type="hidden" name="os0" value="' .  $payPalComboBoxChoice . '">' . PHP_EOL; 
	echo '<input type="hidden" name="currency_code" value="USD">' . PHP_EOL;
	echo '<input type="hidden" name="notify_url" value="https://' . $web_site . '/' . $ipn_file . '">' . PHP_EOL;
	echo '<input type="hidden" name="return" value="https://' . $web_site . '/' . $script_folder_href . 'signup_complete.php?tournament=' . $tournament->TournamentKey . '">' . PHP_EOL;
	echo '<input type="hidden" name="rm" value="1">' . PHP_EOL;
	echo '<input type="image" src="https://www.paypalobjects.com/en_US/i/btn/btn_paynowCC_LG.gif" border="0" name="submit" alt="PayPal - The safer, easier way to pay online!">' . PHP_EOL;
	echo '<img alt="" border="0" src="https://www.paypalobjects.com/en_US/i/scr/pixel.gif" width="1" height="1">' . PHP_EOL;
	echo '</form>' . PHP_EOL;
}

// The $to can be a comma separated list of addresses
function SendEmail($from, $fromPassword, $to, $subject, $message){
    try {
        $mail = new PHPMailer(true);                              // Passing `true` enables exceptions

        //Server settings
        $mail->SMTPDebug = 0;      // use 2 for verbose 
        $mail->isSMTP();                                      // Set mailer to use SMTP
        $mail->Host = 'smtp.dreamhost.com';                  // Specify main and backup SMTP servers
        $mail->SMTPAuth = true;                               // Enable SMTP authentication
        $mail->Username = $from; 
        $mail->Password = $fromPassword; 
        $mail->SMTPSecure = 'ssl';                            // Enable SSL encryption, TLS also accepted with port 465
        $mail->Port = 465;                                    // TCP port to connect to

        //Recipients
        $mail->setFrom($from, 'DoNotReply');          //This is the email your form sends From
		$addresses = explode(',', $to);
		foreach ($addresses as $address) {
    		$mail->AddAddress($address);
		}
        //$mail->addAddress($to, ''); // Add a recipient address
        //$mail->addAddress('contact@example.com');               // Name is optional
        //$mail->addReplyTo('info@example.com', 'Information');
        //$mail->addCC('cc@example.com');
        //$mail->addBCC('bcc@example.com');

        //Attachments
        //$mail->addAttachment('/var/tmp/file.tar.gz');         // Add attachments
        //$mail->addAttachment('/tmp/image.jpg', 'new.jpg');    // Optional name

        //Content
        $mail->isHTML(false);                                  // Set email format to HTML
        $mail->Subject = $subject;
        $mail->Body    = $message;
        //$mail->AltBody = 'This is the body in plain text for non-HTML mail clients';

        $mail->send();
        //echo 'Message has been sent';
    } catch (Exception $e) {
        echo '<p>Message could not be sent. Mailer Error: ' . $mail->ErrorInfo . "</p>";
    }
}

function SendSignupEmail($connection, $tournament, $tournamentDates, $signupKey, $doNotReplyEmailAddress, $doNotReplyEmailPassword){
	
	$signup = GetSignup($connection, $signupKey);
	if(empty($signup)){
		return "Did not find a signup for key: " . $signupKey;
	}
	
	$players = GetPlayersForSignUp($connection, $signupKey);
	
	if(count($players) == 0){
		return "There are no players for signup code " . $signupKey;
	}
	
	// compose message
	$message = "You are signed up for the Coronado Mens Golf " . $tournament->Name . ' tournament on ' . $tournamentDates . '.';
	$message .= "\n\nPlayers in your group:";
	for($i = 0; $i < count($players); ++$i){
		$message .= "\n    " . $players[$i]->LastName;
	}
	$message .= "\n\nRequested time: " . $signup->RequestedTime;
	$message .= "\n\nDo not reply to this email.  Contact the tournament director if you have any questions.\n";
	
	$message .= "\nTo make changes to your signup or to cancel your tee time, use this access code: " . $signup->AccessCode . "\n";
	
	// make sure each line doesn't exceed 70 characters
	//$message = wordwrap($message, 70);
	
	for($i = 0; $i < count($players); ++$i){
		$rosterEntry = GetRosterEntry($connection, $players[$i]->GHIN);
	
		if(!empty($rosterEntry) && !empty($rosterEntry->Email)){
			// send email
			SendEmail($doNotReplyEmailAddress, $doNotReplyEmailPassword, $rosterEntry->Email, 'Coronado Mens Golf Tournament Signup', $message);
		}
	}
	
	return null;
}
function SendRefundEmail($connection, $tournament, $signup, $players, $playersRemoved, $refundFees, $doNotReplyEmailAddress, $doNotReplyEmailPassword){

	$message = $tournament->ChairmanName . "," . "\n\n";
	$payerEmail = "";
	// compose message
	if(!empty($signup->PayerName)){
		$message .= "Please refund $" . $refundFees . " to " . $signup->PayerName . ".  The original payment was made through PayPal on " . $signup->PaymentDateTime . ".";
		if(!empty($signup->PayerEmail)){
			$payerEmail = "," . $signup->PayerEmail;
		}
	}
	else {
		$message .= "Please refund $" . $refundFees . " to the person that paid you.  We have no record that this was paid through PayPal.";
	}
	
	$message .= "\n\nOriginal players in the group:";
	for($i = 0; $i < count($players); ++$i){
		$message .= "\n    " . $players[$i]->LastName;
	}
	$message .= "\n\nRemoved players:";
	for($i = 0; $i < count($playersRemoved); ++$i){
		$message .= "\n    " . $playersRemoved[$i];
	}

	// make sure each line doesn't exceed 70 characters
	//$message = wordwrap($message, 70);

	// Send to both the chairman and the payer
	SendEmail($doNotReplyEmailAddress, $doNotReplyEmailPassword, $tournament->ChairmanEmail . $payerEmail, 'Coronado Mens Golf tournament refund request', $message);

	return null;
}

function SendMergeEmail($tournament, $playersGroup1, $playersGroup2, $doNotReplyEmailAddress, $doNotReplyEmailPassword){

	$message = "";

	// compose message
	$message .= "\n\nFYI\n\nThese players:";
	for($i = 0; $i < count($playersGroup2); ++$i){
		$message .= "\n    " . $playersGroup2[$i]->LastName;
	}
	$message .= "\n\nwere added to the group with these players:";
	for($i = 0; $i < count($playersGroup1); ++$i){
		$message .= "\n    " . $playersGroup1[$i]->LastName;
	}

	SendEmail($doNotReplyEmailAddress, $doNotReplyEmailPassword, $tournament->ChairmanEmail, 'Coronado Mens Golf tournament groups merged', $message);

	return null;
}

function GetTeamFlightIndex($teamNumber)
{
	if($teamNumber == 1) return 'Team1Flight';
	if($teamNumber == 2) return 'Team2Flight';
	die("Unexpected team number: " . $teamNumber);
}

function TeamNumber($t, $teamNumber, $flightErrorList, $extra)
{
	if($t->TeamSize != 2) return;

	echo '<td style="vertical-align:middle">Team '. $teamNumber . PHP_EOL;
	if($t->SCGAQualifier){
		AddSCGAQualifier($t, $teamNumber, $flightErrorList, $extra);
	}
	echo '</td>' . PHP_EOL;
}
function AddSCGAQualifier($t, $teamNumber, $flightErrorList, $extra){

	$teamFlightIndex = GetTeamFlightIndex($teamNumber);
	echo '<br><input  type="radio" name="'. $teamFlightIndex . '" value="F1"';
	if($extra[2 * ($teamNumber - 1)] == 'F1')
	{
		echo ' checked';
	}
	echo '>Flight 1' . PHP_EOL;
	echo '<br><input  type="radio" name="'. $teamFlightIndex . '" value="F2"';
	if($extra[2 * ($teamNumber - 1)] == 'F2')
	{
		echo ' checked';
	}
	echo '>Flight 2' . PHP_EOL;
	if(($teamNumber == 1) && !empty($flightErrorList[0])){
		echo '<br><p style="color:red">' . $flightErrorList[0] . '</p>';
	}
	else if(($teamNumber == 2) && !empty($flightErrorList[2])){
		echo '<br><p style="color:red">' . $flightErrorList[2] . '</p>';
	}
}

function AddPlayerTable($t)
{
	echo '<table style="border: none;">' . PHP_EOL;
	echo '	<colgroup>' . PHP_EOL;
	if($t->MemberGuest){
		echo '		<col style="width: 170px">' . PHP_EOL;
	}
	else {
		echo '		<col style="width: 140px">' . PHP_EOL;
	}
	
	echo '		<col>' . PHP_EOL;
	if($t->SrClubChampionship || $t->ClubChampionship){
		echo '		<col>' . PHP_EOL;
	}
	echo '	</colgroup>' . PHP_EOL;
}

function GetPlayerFlightIndex($playerNumber){
	return 'Player' . $playerNumber . 'Flight';
}

function AddFlights($t, $playerNumber, $extraForPlayer, $errorForPlayer, $rowSpan){
	if($t->SrClubChampionship){
		$flightLabel = GetPlayerFlightIndex($playerNumber);
		if($rowSpan > 1){
			echo '<td rowspan = "' . $rowSpan . '" style="border: none;">' . PHP_EOL;
		}
		else {
			echo '<td style="border: none">' . PHP_EOL;
		}
		echo '<input  type="radio" name="' . $flightLabel . '" value="AGE"';
		if(($extraForPlayer == 'F1') || ($extraForPlayer == 'F2') ||
		($extraForPlayer == 'F3') || ($extraForPlayer == 'AGE'))
		{
			echo ' checked';
		}
		echo '>Age-based flights (F1, F2, F3)' . PHP_EOL;
		
		echo '<br><input  type="radio" name="' . $flightLabel . '" value="CH"';
		if($extraForPlayer == 'CH')
		{
			echo ' checked';
		}
		echo '>Championship' . PHP_EOL;
		if(!empty($errorForPlayer)){
			echo '<p style="color:red">' . $errorForPlayer . '</p>' . PHP_EOL;
		}
		echo '</td>' . PHP_EOL;
	} else if($t->ClubChampionship){
		$flightLabel = GetPlayerFlightIndex($playerNumber);
		if($rowSpan > 1){
			echo '<td rowspan = "' . $rowSpan . '" style="border: none;">' . PHP_EOL;
		}
		else {
			echo '<td style="border: none">' . PHP_EOL;
		}
		echo '<input  type="radio" name="' . $flightLabel . '" value="F1"';
		if($extraForPlayer == 'F1')
		{
			echo ' checked';
		}
		echo '>Handicap-based flights' . PHP_EOL;
		
		echo '<br><input  type="radio" name="' . $flightLabel . '" value="CH"';
		if($extraForPlayer == 'CH')
		{
			echo ' checked';
		}
		echo '>Championship flight' . PHP_EOL;
		if(!empty($errorForPlayer)){
			echo '<p style="color:red">' . $errorForPlayer . '</p>' . PHP_EOL;
		}
		echo '</td>' . PHP_EOL;

	}
}

function AddFlightsError($t, $errorForPlayer){
	if($t->SrClubChampionship || $t->ClubChampionship){
		if(!empty($errorForPlayer)){
			echo '<td style="border: none;color:red">' . $errorForPlayer . '</td>' . PHP_EOL;
		}
		else {
			echo '<td style="border: none;"></td>' . PHP_EOL;
		}
	}
}

function RequestedTime($RequestedTime)
{
	echo '<tr>' . PHP_EOL;
	echo '		<td style="border: none;">Requested time:</td>' . PHP_EOL;
	echo '		<td style="border: none;"><select name="RequestedTime">' . PHP_EOL;

	new_list_option ( 'None', $RequestedTime );
	new_list_option ( '6am-7am', $RequestedTime );
	new_list_option ( '7am-8am', $RequestedTime );
	new_list_option ( '8am-9am', $RequestedTime );
	new_list_option ( '9am-10am', $RequestedTime );
	new_list_option ( '10am-11am', $RequestedTime );

	echo '		</select></td>' . PHP_EOL;
	echo '	</tr>' . PHP_EOL;
}

?>