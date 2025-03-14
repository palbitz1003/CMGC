<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$overrideTitle = "Pending Applications";
get_header ();

class ApplicationEntry {
	public $RecordKey;
	public $LastName;
	public $FirstName;
	public $FullName;
    public $GHIN;
    public $DateTimeAdded;
	public $Payment;
}

$connection = new mysqli ('p:' . $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( $connection->connect_error );

$sqlCmd = "SELECT RecordKey,LastName,FirstName,GHIN,DateTimeAdded,Payment FROM `MembershipApplication` WHERE `Active` = 1";
$query = $connection->prepare ( $sqlCmd );

if (! $query) {
	die ( $sqlCmd . " prepare failed: " . $connection->error );
}

if (! $query->execute ()) {
	die ( $sqlCmd . " execute failed: " . $connection->error );
}

$query->bind_result ($recordKey, $lastName, $firstName, $ghin, $dateTimeAdded, $payment );

$applicationEntryList = array();
$showPaymentMessage = false;

while ( $query->fetch () ) {
    $applicationEntry = new ApplicationEntry();
	$applicationEntry->RecordKey = $recordKey;
	$applicationEntry->LastName = $lastName;
	$applicationEntry->FirstName = $firstName;
	$applicationEntry->FullName = $lastName . ', ' . $firstName;
	$applicationEntry->GHIN = $ghin;
    $applicationEntry->DateTimeAdded = $dateTimeAdded;
	$applicationEntry->Payment = $payment;

    if($payment == 0){
        $showPaymentMessage = true;
    }

    $applicationEntryList[] = $applicationEntry;
}

$query->close ();

if (! $applicationEntryList || (count ( $applicationEntryList ) == 0)) {
    echo "All applications have been processed.";
    $connection->close ();

    get_footer ();
	return;
}

echo ' <div id="content-container" class="entry-content">';
echo '    <div id="content" role="main">';

echo '<p>' . PHP_EOL;
echo 'NOTE: The order in which applications are completed does not indicate the order in which you will be placed ';
echo 'on the waiting list. After 87 applications are completed or at the end of February, whichever happens first, ';
echo 'the applications will be assigned a random number and moved to the waiting list in random number order. ';
echo '</p>' . PHP_EOL;

echo '<p>There are currently ' . count($applicationEntryList) . ' applications filled out.</p>' . PHP_EOL;

if($showPaymentMessage){
	echo '<p>Click on the link next to your name to pay half of your initiation fee.</p>';
}

echo PHP_EOL;

echo '<table style="border:none;margin-left:auto;margin-right:auto">' . PHP_EOL;
echo '<thead><tr class="header"><th></th><th>Name</th><th>Submit Date</th></tr></thead>' . PHP_EOL;
echo '<tbody>' . PHP_EOL;

for($i = 0; $i < count ( $applicationEntryList ); ++ $i) {
	if(($i % 2) == 0){
		echo '<tr class="d1">';
	}
	else {
		echo '<tr class=d0>';
	}
	echo '<td>';
    if($applicationEntryList[$i]->Payment > 0){
        echo "&nbsp;&nbsp;Paid&nbsp;&nbsp;";
    }
    else {
        echo '<a href="pay_initiation_fee.php?application_id=' . $applicationEntryList[$i]->RecordKey . '">Pay Fee</a>';
    }
	echo '</td>';
	echo '<td>&nbsp;&nbsp;' . $applicationEntryList[$i]->FullName . '&nbsp;&nbsp;</td>';
    echo '<td>&nbsp;&nbsp;' . $applicationEntryList[$i]->DateTimeAdded . '</td>';
	echo '</tr>' . PHP_EOL;
}

echo '</tbody>' . PHP_EOL;
echo '</table>' . PHP_EOL;


echo '    </div><!-- #content -->';
echo ' </div><!-- #content-container -->';

$connection->close ();

get_footer ();
?>