<?php

class WaitingListEntry {
	public $RecordKey;
	public $Position;
	public $Name;
	public $DateAdded;
	public $PaymentDue;
	public $Payment;
}

function GetWaitlistEntry($connection, $recordKey)
{
    $sqlCmd = "SELECT * FROM `WaitingList` WHERE `Active` = 1 AND `RecordKey` = ?";
    $query = $connection->prepare ( $sqlCmd );

    if (! $query->bind_param ( 'i', $recordKey )) {
        die ( $sqlCmd . " bind_param failed: " . $connection->error );
    }

    if (! $query) {
        die ( $sqlCmd . " prepare failed: " . $connection->error );
    }

    if (! $query->execute ()) {
        die ( $sqlCmd . " execute failed: " . $connection->error );
    }

    $query->bind_result ( $recordKey, $position, $name, $dateAdded, $active, $paymentDue, $payment, $paymentDateTime, $PayerName );

    $waitingListEntry = new WaitingListEntry();
    while ( $query->fetch () ) {
        $waitingListEntry->RecordKey = $recordKey;
        $waitingListEntry->Position = $position;
        $waitingListEntry->Name = $name;
        $waitingListEntry->DateAdded = $dateAdded;
        $waitingListEntry->PaymentDue = $paymentDue;
        $waitingListEntry->Payment = $payment;
    }

    return $waitingListEntry;
}

?>
