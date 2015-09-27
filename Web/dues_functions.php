<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/signup functions.php';

class Dues {
	public $Year;
	public $GHIN;
	public $Name;
	public $Payment;
	public $PaymentDateTime;
	public $PayerName;
	public $PayerEmail;
	public $RIGS;
}

class RosterWithDues {
	public $GHIN;
	public $Name;
	public $Active;
	public $Payment;
}

function GetPlayerDues($connection, $playerGHIN) {

	$sqlCmd = "SELECT * FROM `Dues` WHERE `GHIN` = ?";
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

	$player->bind_result ( $year, $ghin, $name, $payment, $paymentDateTime, $payerName, $payerEmail, $rigs);

	$playerDues = null;
	$count = 1;
	while($player->fetch()) {
		if($count > 1){
			die('Player ' . $playerGHIN . ' has payed more than once');
		}

		$playerDues = new Dues();
		$playerDues->Year = $year;
		$playerDues->GHIN = $ghin;
		$playerDues->Name = $name;
		$playerDues->Payment = $payment;
		$playerDues->PaymentDateTime = $paymentDateTime;
		$playerDues->PayerName = $payerName;
		$playerDues->PayerEmail = $payerEmail;
		$playerDues->RIGS = $rigs;
		$count++;
	}
	
	//if(!isset($playerDues)) { echo 'did not find player<br>'; }

	$player->close ();

	return $playerDues;
}

function GetPlayerDuesNotPaid($connection) {

	$sqlCmd = "SELECT Roster.GHIN, Roster.LastName, Roster.FirstName, Roster.Active, Dues.Payment FROM `Roster` LEFT JOIN `Dues` ON Roster.GHIN = Dues.GHIN ORDER BY Roster.LastName ASC, Roster.FirstName ASC";
	$player = $connection->prepare ( $sqlCmd );

	if (! $player) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}

	if (! $player->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}

	$player->bind_result ( $ghin, $lastName, $firstName, $active, $payment);

	$notPaid = array();
	while($player->fetch()) {
		if(($active == 1) && (empty($payment) || ($payment == 0))){
			$p = new RosterWithDues();
			$p->GHIN = $ghin;
			$p->Name = $lastName . ', ' . $firstName;
			$p->Active = $active;
			$p->Payment = $payment;
			$notPaid[] = $p;
		}
	}

	$player->close ();

	return $notPaid;
}

function InsertPlayerForDues($connection, $year, $ghin, $name) {
	$sqlCmd = "INSERT INTO `Dues` VALUES (?, ?, ?, ?, NULL, ?, ?, ?)";
	$insert = $connection->prepare ( $sqlCmd );

	if (! $insert) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}

	$payment = 0.0;
	$payerName = "";
	$payerEmail = "";
	$rigs = false;

	if (! $insert->bind_param ( 'iisdssi',  $year, $ghin, $name, $payment, $payerName, $payerEmail, $rigs )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}

	if (! $insert->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}

	$insert->close();
}

function GetPlayersDuesPaid($connection) {

	$sqlCmd = "SELECT * FROM `Dues` WHERE `Payment` > 0 ORDER BY `Name` ASC";
	$player = $connection->prepare ( $sqlCmd );

	if (! $player) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}

	if (! $player->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}

	$player->bind_result ( $year, $ghin, $name, $payment, $paymentDateTime, $payerName, $payerEmail, $rigs);

	$players = array();
	while($player->fetch()) {

		$p = new Dues();
		$p->Year = $year;
		$p->GHIN = $ghin;
		$p->Name = $name;
		$p->Payment = $payment;
		$p->PaymentDateTime = $paymentDateTime;
		$p->PayerName = $payerName;
		$p->PayerEmail = $payerEmail;
		$p->RIGS = $rigs;

		$players[] = $p;

	}

	$player->close ();

	return $players;
}

function UpdateDuesDatabase($connection, $ghin, $payment, $payerName, $payerEmail, $logMessage){

	if (!file_exists('./logs')) {
		mkdir('./logs', 0755, true);
	}

	$now = new DateTime ( "now" );
	$year = $now->format('Y') + 1;

	$logFile = "./logs/dues." . $year . ".log";
	error_log(date ( '[Y-m-d H:i e] ' ) . $logMessage . PHP_EOL, 3, $logFile);

	if ($connection->connect_error){
		error_log(date ( '[Y-m-d H:i e] ' ) . $connection->connect_error . PHP_EOL, 3, $logFile);
		return;
	}

	$player = GetPlayerDues($connection, $ghin);
	if(empty($player)){
		error_log(date ( '[Y-m-d H:i e] ' ) . "Failed to find ghin " . $ghin . " in the dues table." . PHP_EOL, 3, $logFile);
		return;
	}
	
	// Add to the current amount to handle the refund case
	$payment = $payment + $player->Payment;

	// Duplicate the code here so the die messages can be replace with log messages
	$sqlCmd = "UPDATE `Dues` SET `Payment`= ?, `PaymentDateTime`= ?, `PayerName`= ?, `PayerEmail`= ?, `RIGS` = 0 WHERE `GHIN` = ?";
	$update = $connection->prepare ( $sqlCmd );

	if (! $update) {
		error_log(date ( '[Y-m-d H:i e] ' ) . $sqlCmd . " prepare failed: " . $connection->error . PHP_EOL, 3, $logFile);
		return;
	}

	$date = date ( 'Y-m-d H:i:s' );
	if (! $update->bind_param ( 'dsssi',  $payment, $date, $payerName, $payerEmail, $ghin)) {
		error_log(date ( '[Y-m-d H:i e] ' ) . $sqlCmd . " bind_param failed: " . $connection->error . PHP_EOL, 3, $logFile);
		return;
	}

	if (! $update->execute ()) {
		error_log(date ( '[Y-m-d H:i e] ' ) . $sqlCmd . " execute failed: " . $connection->error . PHP_EOL, 3, $logFile);
		return;
	}
	$update->close ();

	error_log(date ( '[Y-m-d H:i e] ' ) . "Updated player " . $player->Name . " payment to " . $payment . PHP_EOL, 3, $logFile);
}

function SendDuesEmail($connection, $ghin, $payment, $web_site){

	$rosterEntry = GetRosterEntry ( $connection, $ghin );
	if(empty($rosterEntry)){
		return "Did not find a player for ghin: " . $ghin;
	}

	$now = new DateTime ( "now" );
	$year = $now->format('Y') + 1;

	// compose message
	$message = "You have paid your dues ($" . $payment . ") for the Coronado Men's Golf Club for " . $year;

	if(!empty($rosterEntry) && !empty($rosterEntry->Email)){
		mail($rosterEntry->Email, "Coronado Men's Golf Club yearly dues", $message, "From: DoNotReply@" . $web_site);
	}

	return null;
}
?>