<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$overrideTitle = "Waiting List";
get_header ();

class WaitingListEntry {
	public $RecordKey;
	public $Position;
	public $Name;
	public $DateAdded;
	public $PaymentDue;
	public $Payment;
}

$connection = new mysqli ('p:' . $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( $connection->connect_error );

echo ' <div id="content-container" class="entry-content">';
echo '    <div id="content" role="main">';

$sqlCmd = "SELECT * FROM `WaitingList` WHERE `Active` = 1 ORDER BY `Position` ASC";
$query = $connection->prepare ( $sqlCmd );

if (! $query) {
	die ( $sqlCmd . " prepare failed: " . $connection->error );
}

if (! $query->execute ()) {
	die ( $sqlCmd . " execute failed: " . $connection->error );
}

/*
* Record Key (int) primary, unique, auto_increment
* Position (int)
* Name (varchar 50)
* Date Added (date)
* Active (tiny int)
* Payment Due (int)
* Payment (int)
* Payment DateTime (datetime)
* Payer Name (varchar 50)
*/

$query->bind_result ( $recordKey, $position, $name, $dateAdded, $active, $paymentDue, $payment, $paymentDateTime, $PayerName );

$waitingListEntriesByPosition = array();
$showPaymentMessage = false;

while ( $query->fetch () ) {
	$waitingListEntry = new WaitingListEntry();
	$waitingListEntry->RecordKey = $recordKey;
	$waitingListEntry->Position = $position;
	$waitingListEntry->Name = $name;
	$waitingListEntry->DateAdded = $dateAdded;
	$waitingListEntry->PaymentDue = $paymentDue;
	$waitingListEntry->Payment = $payment;
	$waitingListEntriesByPosition [] = $waitingListEntry;

	if(($paymentDue > 0) && ($payment == 0)){
		$showPaymentMessage = true;
	}
}

$query->close ();
if (! $waitingListEntriesByPosition || (count ( $waitingListEntriesByPosition ) == 0)) {
	return;
}

$sqlCmd = "SELECT * FROM `WaitingList` WHERE `Active` = 1 ORDER BY `Name` ASC";
$query = $connection->prepare ( $sqlCmd );

if (! $query) {
	die ( $sqlCmd . " prepare failed: " . $connection->error );
}

if (! $query->execute ()) {
	die ( $sqlCmd . " execute failed: " . $connection->error );
}

$query->bind_result ( $recordKey, $position, $name, $dateAdded, $active, $paymentDue, $payment, $paymentDateTime, $PayerName );

$waitingListEntriesByName = array();

while ( $query->fetch () ) {
	$waitingListEntry = new WaitingListEntry();
	$waitingListEntry->RecordKey = $recordKey;
	$waitingListEntry->Position = $position;
	$waitingListEntry->Name = $name; 
	$waitingListEntry->DateAdded = $dateAdded;
	$waitingListEntry->PaymentDue = $paymentDue;
	$waitingListEntry->Payment = $payment;
	$waitingListEntriesByName [] = $waitingListEntry;
}

if (! $waitingListEntriesByName || (count ( $waitingListEntriesByName ) == 0)) {
	$query->close ();
	return;
}

if($showPaymentMessage){
	echo '<p style="text-align: center">Click on the link next to your name to pay your remaining membership fee and your yearly dues.</p>';
}


// A single table with 1 row.  The row has 2 data elements, each a table.
echo PHP_EOL;
echo '<table style="border:none;margin-left:auto;margin-right:auto"><tbody>' . PHP_EOL;
echo '<tr>' . PHP_EOL;
//echo '<td style="width:50%;border:none;">' . PHP_EOL;
echo '<td style="border:none;">' . PHP_EOL;
echo '<table>' . PHP_EOL;
echo '<thead><tr class="header"><th></th><th>Pos  </th><th>Name</th><th>Date Added</th></tr></thead>' . PHP_EOL;
echo '<tbody>' . PHP_EOL;

for($i = 0; $i < count ( $waitingListEntriesByPosition ); ++ $i) {
	if(($i % 2) == 0){
		echo '<tr class="d1">';
	}
	else {
		echo '<tr class=d0>';
	}
	echo '<td>';
	if($waitingListEntriesByPosition[$i]->PaymentDue > 0){
		if($waitingListEntriesByPosition[$i]->Payment > 0){
			echo "Paid";
		}
		else {
			echo '<a href="membership_invitation_application.php?waiting_list_id=' . $waitingListEntriesByPosition[$i]->RecordKey . '">Pay Membership</a>';
		}
	}
	echo '</td>';
	echo '<td>' . $waitingListEntriesByPosition[$i]->Position . '</td>';
	echo '<td>' . $waitingListEntriesByPosition[$i]->Name . '</td>';
	echo '<td>' . date ( 'n/j/Y', strtotime ( $waitingListEntriesByPosition[$i]->DateAdded ) ) . '</td>';
	echo '</tr>' . PHP_EOL;
}


// Finish the first column table.  Show the 2nd column table.
echo '</tbody>' . PHP_EOL;
echo '</table>' . PHP_EOL;
echo '</td>' . PHP_EOL;
//echo '<td style="width:50%;border:none;">' . PHP_EOL;
echo '<td style="border:none;">' . PHP_EOL;

echo '<table>' . PHP_EOL;
echo '<thead><tr class="header"><th>Pos  </th><th>Name - Alphabetical</th></tr></thead>' . PHP_EOL;
echo '<tbody>' . PHP_EOL;
for($i = 0; $i < count ( $waitingListEntriesByName ); ++ $i) {
	if(($i % 2) == 0){
		echo '<tr class="d1">';
	}
	else {
		echo '<tr class="d0">';
	}
	echo '<td>' . $waitingListEntriesByName[$i]->Position . '</td>';
	echo '<td>' . $waitingListEntriesByName[$i]->Name . '</td>';
	echo '</tr>' . PHP_EOL;
}
echo '</tbody>' . PHP_EOL;
echo '</table>' . PHP_EOL;

echo '</td></tr></tbody></table>' . PHP_EOL;

echo '    </div><!-- #content -->';
echo ' </div><!-- #content-container -->';

$query->close ();

$connection->close ();

get_footer ();
?>