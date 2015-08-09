<?php

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
?>