<?php
require_once realpath ( $_SERVER ["DOCUMENT_ROOT"] ) . '/login.php';
require_once realpath ( $_SERVER ["DOCUMENT_ROOT"] ) . $wp_folder . '/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

class ApplicationEntry {
	public $RecordKey;
	public $LastName;
	public $FirstName;
	public $FullName;
    public $GHIN;
	public $Payment;
}

class PayPalDetailsMembership {
	public $PayPayButton;
	public $Dues;
	public $PaymentType;
}

$recordKey = $_GET ['application_id'];
if (! $recordKey || !is_numeric($recordKey)) {
	die ( "Invalid application ID" );
}

$testMode = false;
if(!empty($_GET ['mode']) && ($_GET ['mode'] == "test")){
	$testMode = true;
}
$testMode = true;

$overrideTitle = "Pay Initiation Fee";
get_header ();
get_sidebar ();

$connection = new mysqli ( $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( $connection->connect_error );

$sqlCmd = "SELECT LastName,FirstName,GHIN,Payment FROM `MembershipApplication` WHERE `Active` = 1 AND `RecordKey` = ?";
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

$query->bind_result ($lastName, $firstName, $ghin, $payment );

$applicationEntry = new ApplicationEntry();

while ( $query->fetch () ) {
	$applicationEntry->RecordKey = $recordKey;
	$applicationEntry->LastName = $lastName;
	$applicationEntry->FirstName = $firstName;
	$applicationEntry->FullName = $lastName . ', ' . $firstName;
	$applicationEntry->GHIN = $ghin;
	$applicationEntry->Payment = $payment;
}

cmgc_start_initiation_fee_page();

if(empty($applicationEntry->RecordKey)){
    cmgc_finish_initiation_fee_page($connection, "Failed to find application id " . $recordKey);
    die ();
}

if($applicationEntry->Payment > 0){
    cmgc_finish_initiation_fee_page($connection, 'Initiation fee for "' . $applicationEntry->FullName . '" has already paid');
    die();
}

$paymentType = "Initiation";
if($testMode){
    $paymentType = "Sandbox_Initiation";
}
$paypalDetails = cmgc_initiation_fee_GetPayPalDetails($connection, $paymentType);

if(empty($paypalDetails)){
    die("Unable to get PayPal details for payment type: " . $paymentType);
}

echo '<h2 class="entry-title" style="text-align:center">Pay Initiation Fee</h2>' . PHP_EOL;
if($testMode){
    echo '<h2 class="entry-title" style="text-align:center">Test...</h2>' . PHP_EOL;  
}

echo '<p>You are paying for: ' . $applicationEntry->FullName . '</p><p> Your payment covers half of the initiation fee.</p>' . PHP_EOL;
echo "<p>The link below takes you to PayPal to make your payment.  You can pay with credit card even if you do not have a PayPal account. No credit card or account information is kept on the Coronado Men's Golf web site.</p>";
echo '<p>PayPal will notify the CMGC website of your payment and the link next to your name on the application list will change to "Paid". If the application list does not update to "Paid" within 24hrs, contact the membership chairman.</p>' . PHP_EOL;
echo '<p>If you have problems reaching PayPal, turn off your VPN if you are using one, or try a different device.</p>' . PHP_EOL;
echo '<p style="text-align: center;"><b>Entry Fees: $' . $paypalDetails->Dues . '</b></p>' . PHP_EOL;

if($testMode){
    echo '<form style="text-align:center" action="https://www.sandbox.paypal.com/cgi-bin/webscr" method="post" target="_top">' . PHP_EOL;
}
else {
    echo '<form style="text-align:center" action="https://www.paypal.com/cgi-bin/webscr" method="post" target="_top">' . PHP_EOL;
}

// Variables described in https://developer.paypal.com/api/nvp-soap/paypal-payments-standard/integration-guide/Appx-websitestandard-htmlvariables/
echo '<input type="hidden" name="cmd" value="_s-xclick">' . PHP_EOL;
echo '<input type="hidden" name="hosted_button_id" value="' . $paypalDetails->PayPayButton . '">' . PHP_EOL;
echo '<input type="hidden" name="item_name" value="Membership Dues">' . PHP_EOL;
echo '<input type="hidden" name="custom" value="FinalPayment;' . $applicationEntry->RecordKey . ';' . $applicationEntry->FullName . ';' . $applicationEntry->GHIN . '">' . PHP_EOL;
// Don't need the on0 (option name 0) and os0 (option selection 0) since we are not paying for a tournament
//echo '<input type="hidden" name="on0" value="Entry Fees">' . PHP_EOL;
//echo '<input type="hidden" name="os0" value="' .  $payPalComboBoxChoice . '">' . PHP_EOL; 
echo '<input type="hidden" name="currency_code" value="USD">' . PHP_EOL;
echo '<input type="hidden" name="notify_url" value="https://' . $web_site . '/' . $ipn_membership_file . '">' . PHP_EOL;
echo '<input type="hidden" name="return" value="https://' . $web_site . '/index.php">' . PHP_EOL;
// rm is return method. Value 1 is: The buyer's browser is redirected to the return URL by using the GET method, but no payment variables are included.
echo '<input type="hidden" name="rm" value="1">' . PHP_EOL;
if($testMode){
    echo '<input type="image" src="https://www.sandbox.paypal.com/en_US/i/btn/btn_buynowCC_LG.gif" name="submit" alt="PayPal - The safer, easier way to pay online!">' . PHP_EOL;
    echo '<img alt="" src="https://www.sandbox.paypal.com/en_US/i/scr/pixel.gif" width="1" height="1">' . PHP_EOL;
}
else {
    echo '<input type="image" src="https://www.paypalobjects.com/en_US/i/btn/btn_paynowCC_LG.gif" name="submit" alt="PayPal - The safer, easier way to pay online!">' . PHP_EOL;
    echo '<img alt="" src="https://www.paypalobjects.com/en_US/i/scr/pixel.gif" width="1" height="1">' . PHP_EOL;
}

echo '</form>' . PHP_EOL;

cmgc_finish_initiation_fee_page($connection, "");

function cmgc_start_initiation_fee_page(){
    echo ' <div id="content-container" class="entry-content">';
    echo '    <div id="content" role="main">';
}

function cmgc_finish_initiation_fee_page($connection, $error){
    if(!empty($error)){
        echo $error . "<br>";
    }
    echo '    </div><!-- #content -->';
    echo ' </div><!-- #content-container -->';

    $connection->close ();
    get_footer ();
}

function cmgc_initiation_fee_GetPayPalDetails($connection, $paymentType){
	$sqlCmd = "SELECT * FROM `PayPalMembership` WHERE `PaymentType` = ?";
	$payPal = $connection->prepare ( $sqlCmd );

	if (! $payPal) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}

	if (! $payPal->bind_param ( 's', $paymentType )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}

	if (! $payPal->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}

	$payPal->bind_result ( $payPalButton, $dues, $type );

	$details = new PayPalDetailsMembership();
	if($payPal->fetch ()){
		$details->PayPayButton = $payPalButton;
		$details->Dues = $dues;
		$details->PaymentType = $type;
	}

	$payPal->close ();

	return $details;
}
?>