<?php

use PHPMailer\PHPMailer\PHPMailer;
use PHPMailer\PHPMailer\Exception;
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/signup functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';

date_default_timezone_set ( 'America/Los_Angeles' );


$testMode = false;

$connection = new mysqli ( $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error){
	die ( $connection->connect_error );
}

//$now = new DateTime ( "now" );
//$startDate = new DateTime('2023-01-01');
//if($now < $startDate){
/*
	$overrideTitle = "Membership Application";
	get_header ();
?>
	<div id="content-container" class="entry-content">
	<div id="content" role="main">
	<h2 class="entry-title" style="text-align:center">Coronado Men’s Golf Club (CMGC) Membership Application</h2>
	<p>
	The CMGC New Member Application acceptance period has closed for 2023. 
	</p>
	</div><!-- #content -->
	</div><!-- #content-container -->
<?php
	get_footer();
	return;
*/
//}

$error = "";
$ghin = "";
$lastName = "";
$firstName = "";
$birthMonth = "";
$birthDay = "";
$birthYear = "";
$birthDate = "";
$email = "";
$email2 = "";
$phoneNumber = "";
// mailing address was replaced with street/city/state/zip
$mailingAddress = "";
$sponsor1LastName = "";
$sponsor1Ghin = "";
$sponsor1Phone = "";
$sponsor2LastName = "";
$sponsor2Ghin = "";
$sponsor2Phone = "";
$streetAddress = "";
$city = "";
$state = "CA";
$zipCode = "";

$membershipEmail = "cmgcmembership1@gmail.com";

$debug = false;
if(!empty($_GET['debug'])){
	$debug = true;
	$membershipEmail = "cmgc.td@gmail.com";
}

// Remove single and double quotes?
//$LastName[$i] = str_replace("'", "", $LastName[$i]); // remove single quotes
//$LastName[$i] = str_replace('"', "", $LastName[$i]); // remove double quotes

if ($_SERVER["REQUEST_METHOD"] == "POST") {
	$lastName = TestInput($_POST["LastName"]);
	$firstName = TestInput($_POST["FirstName"]);
	// mailing address was replaced with street/city/state/zip
	//$mailingAddress = TestInput($_POST["MailingAddress"]);
	$email = TestInput($_POST["Email"]);
	$email2 = TestInput($_POST["Email2"]);
	$ghin = TestInput($_POST["GHIN"]);
	$phoneNumber = TestInput($_POST["Phone"]);
	$birthMonth = TestInput($_POST["BirthMonth"]);
	$birthDay = TestInput($_POST["BirthDay"]);
	$birthYear = TestInput($_POST["BirthYear"]);
	$sponsor1LastName = TestInput($_POST["Sponsor1LastName"]);
	$sponsor1Ghin = TestInput($_POST["Sponsor1Ghin"]);
	$sponsor1Phone = TestInput($_POST["Sponsor1Phone"]);
	$sponsor2LastName = TestInput($_POST["Sponsor2LastName"]);
	$sponsor2Ghin = TestInput($_POST["Sponsor2Ghin"]);
	$sponsor2Phone = TestInput($_POST["Sponsor2Phone"]);
	$streetAddress = TestInput($_POST["StreetAddress"]);
	$city = TestInput($_POST["City"]);
	$state = TestInput($_POST["State"]);
	$zipCode = TestInput($_POST["ZipCode"]);

	if(empty($state)){
		$state = "CA";
	}

	$birthDate = $birthYear . '-' . $birthMonth . '-' . $birthDay;

	if(strcasecmp($email, $email2) != 0){
		$error = "Email addresses are different: " . $email . ", " . $email2;
	}
	else if(strpos($email, '@') === false){
		$error = "Invalid email address. Format is user@domain";
	}
	else if(!checkdate($birthMonth, $birthDay, $birthYear))
	{
		$error = "Date of birth is not a valid date: " . $birthMonth . "/" . $birthDay . "/" . $birthYear;
	}
	else if(intVal($sponsor1Ghin) === 0){
		$error = "Sponsor 1 GHIN number cannot be 0";
	}
	else if(intVal($sponsor2Ghin) === 0){
		$error = "Sponsor 2 GHIN number cannot be 0";
	}
	else if(intval($sponsor1Ghin) == intval($sponsor2Ghin)){
		$error = "Sponsor 1 and sponsor 2 must be different people (the GHIN numbers match)";
	}
	else {
		$error = CheckGhin($connection, $sponsor1LastName, $sponsor1Ghin);
		if(strlen($error) == 0){
			// only 1 error message possible
			$error = CheckGhin($connection, $sponsor2LastName, $sponsor2Ghin);
		} else {
			// potentially append 2 error messages together
			$error2 = CheckGhin($connection, $sponsor2LastName, $sponsor2Ghin);
			if(strlen($error2) > 0){
				$error = $error . "<br>" . $error2;
			}
		}
		if((strlen($error) == 0) && CheckForExistingApplication($connection, $lastName, $firstName, $ghin)){
			$error = 'Pending application already exists for: ' . $lastName . ', ' . $firstName . ' (' . $ghin . ')';
		}
	}
}


$overrideTitle = "Membership Application";

//var_dump($_POST);

// If this page has not been filled in or there is an error, show the form
if ((strlen($error) != 0) || !isset ( $_POST ['LastName'] ) || ($debug && (strlen($ghin) == 0))) {
	get_header ();

?>
	<div id="content-container" class="entry-content">
	<div id="content" role="main">
	<h2 class="entry-title" style="text-align:center">Coronado Men’s Golf Club (CMGC) Membership Application</h2>
<p>

<?php
	if(strlen($error) != 0){
		echo '<p style="color:red;">' . PHP_EOL;
		echo $error . PHP_EOL;
		echo '</p>' . PHP_EOL;
	} else if($debug && (strlen($ghin) == 0)) {
		$lastName = "Test";
		$firstName = "Johnny";
		$birthMonth = "1";
		$birthDay = "1";
		$birthYear = "1960";
		$birthDate = $birthYear . '-' . $birthMonth . '-' . $birthDay;
		$email = "cmgc.td@gmail.com";
		$email2 = "cmgc.td@gmail.com";
		$phoneNumber = "111-222-3333";
		$sponsor1LastName = "";
		$sponsor1Ghin = "";
		$sponsor1Phone = "222-333-4444";
		$sponsor2LastName = "";
		$sponsor2Ghin = "";
		$sponsor2Phone = "333-444-5555";
		$streetAddress = "111 Main";
		$city = "San Diego";
		$state = "CA";
		$zipCode = "92101";
	} 

	/*
	echo '<p>' . PHP_EOL;
	echo 'NOTE: Normally, membership applications are processed in the order they are completed. But, during the first week ';
	echo 'that applications are being accepted, we will randomize the order of the applications. We do not want this to be a race ';
	echo 'to get your application completed and it gives us time to deal with any initial problems. So, applications completed before ';
	echo 'end of day Sunday January 8th will be assigned a random number and moved to the waiting list in random number order. ';
	echo '(Your application is not "complete" until half of the initiation fee has been paid, as described below.)' . PHP_EOL;
	echo '</p>' . PHP_EOL;
	*/

	echo '<form name="input" method="post">' . PHP_EOL;
	
	echo '<table style="border: none;margin-left:auto;margin-right:auto;">' . PHP_EOL;
	
    echo '<tr>' . PHP_EOL;
	echo '<td style="border: none;">Last Name</td>' . PHP_EOL;
	echo '<td style="border: none;"><input type="text" size="25"';
	echo '    name="LastName" value="' . $lastName . '" required></td>' . PHP_EOL;
	echo '<td style="border: none;">First Name</td>' . PHP_EOL;
	echo '<td style="border: none;"><input type="text" size="25"';
	echo '    name="FirstName" value="' . $firstName . '" required></td>' . PHP_EOL;
	echo '</tr>'  . PHP_EOL;

    echo '<tr>' . PHP_EOL;
	echo '<td style="border: none;">Street Address</td>' . PHP_EOL;
	echo '<td style="border: none;" colspan="3"><input type="text"';
	echo '    name="StreetAddress" size="70" value="' . $streetAddress . '" required></td>' . PHP_EOL;
	echo '</tr>'  . PHP_EOL;

	echo '<tr>' . PHP_EOL;
	echo '<td style="border: none;">City</td>' . PHP_EOL;
	echo '<td style="border: none;" colspan="3"><input type="text"';
	echo '    name="City" size="70" value="' . $city . '" required></td>' . PHP_EOL;
	echo '</tr>'  . PHP_EOL;

	echo '<tr>' . PHP_EOL;
	echo '<td style="border: none;">State</td>' . PHP_EOL;
	echo '<td style="border: none;" colspan="3"><input type="text"';
	// Make size 1 larger for iPhone
	echo '    name="State" size="3" value="' . $state . '" required></td>' . PHP_EOL;
	echo '</tr>'  . PHP_EOL;

	echo '<tr>' . PHP_EOL;
	echo '<td style="border: none;">Zip Code</td>' . PHP_EOL;
	echo '<td style="border: none;" colspan="3"><input type="text"';
	// Make size 1 larger for iPhone
	echo '    name="ZipCode" size="6" value="' . $zipCode . '" required></td>' . PHP_EOL;
	echo '</tr>'  . PHP_EOL;

	echo '<tr>' . PHP_EOL;
	echo '<td style="border: none;">Email Address</td>' . PHP_EOL;
	echo '<td style="border: none;" colspan="3"><input type="text" size="70"';
	echo '    name="Email" value="' . $email . '" required></td>' . PHP_EOL;
	echo '</tr>'  . PHP_EOL;

	echo '<tr>' . PHP_EOL;
	echo '<td style="border: none;">Re-enter Email</td>' . PHP_EOL;
	echo '<td style="border: none;" colspan="3"><input type="text" size="70"';
	echo '    name="Email2" value="' . $email2 . '" required></td>' . PHP_EOL;
	echo '</tr>'  . PHP_EOL;

	echo '<tr>' . PHP_EOL;
	echo '<td style="border: none;">GHIN Number</td>' . PHP_EOL;
	echo '<td style="border: none;"><input type="text" name="GHIN" size="10" pattern="[0-9]+"';
	echo '    value="' . $ghin . '"  required><br> <small>(Use zero if you don\'t have one)</small></td>' . PHP_EOL;
	echo '<td style="border: none;">Cell Number</td>' . PHP_EOL;
	echo '<td style="border: none;"><input type="text" id="phone" name="Phone"  size="25"';
	echo '	pattern="[0-9]{3}-[0-9]{3}-[0-9]{4}" value="' . $phoneNumber . '" required><br><small>(Format: 123-456-7890)</small></td>' . PHP_EOL;
	echo '</tr>' . PHP_EOL;
	
	echo '<tr>' . PHP_EOL;
	echo '<td style="border: none;">Date of Birth</td>' . PHP_EOL;
	echo '<td style="border: none;" colspan="3">';
	echo 'Month <input type="text" size = "3" name="BirthMonth" pattern="[0-9]{1,2}" value="' . $birthMonth . '" required>';
	echo ' Day <input type="text" size = "3" name="BirthDay" pattern="[0-9]{1,2}" value="' . $birthDay . '" required>';
	echo ' Year <input type="text" size = "6" name="BirthYear" pattern="[0-9]{4}" value="' . $birthYear . '" required>';
	echo ' <small>(e.g.: 5/29/1958)</small></td>' . PHP_EOL;
	echo '</tr>'  . PHP_EOL;
	
	echo '</table>' . PHP_EOL;

?>

<ol >
  <li style="margin-bottom: 15px">
The cost of membership for the Coronado Men’s Golf Club (CMGC) is an initiation fee of $400 plus one year’s annual dues, which is currently $175.
  </li>
  <li style="margin-bottom: 15px">
You must have 2 CMGC sponsors (see below).
  </li>
  <li style="margin-bottom: 15px">
To complete your application, you must pay half of the initiation fee ($200) which is non-refundable. 
When you click on the Submit Application And Pay Fee button below, you will then be given a page with a link to PayPal to pay the $200 fee. 
Your application is then put on the Pending Applications list. After your sponsors are verified by the CMGC Membership Chair, your application is moved to the Waiting List.
(Both the Pending Applications and Waiting list are visible on the club’s website, <a href="https://coronadomensgolf.org">coronadomensgolf.org</a>, under the Membership menu item.)
  </li>
  <li style="margin-bottom: 15px">
You must keep the CMGC updated for any changes to your mailing address, email address or phone number. 
  </li>
  <li style="margin-bottom: 15px">
If your application is withdrawn prior to an offer of membership, or if the applicant declined an offer of membership, 
then the entire deposit will be forfeited. You may withdraw your application prior to an offer of membership for a full refund if the initiation fee increases.
  </li>
  <li style="margin-bottom: 15px">
You will receive an invitation of membership by email (cmgcmembership1@gmail.com) when you near the top of the waiting list. 
To ensure delivery of your membership invitation by email, please add cmgcmembership1@gmail.com to your address book. When you accept the membership invitation, 
you must pay the remaining balance of the initiation fee of $200 plus first year’s dues, which is currently $175 for a total of $375 to become a CMGC member.
  </li>
  <li style="margin-bottom: 15px">
Your CMGC sponsors must be members in good standing for a minimum of 12 months. Each of your sponsors states that they have known you for at least one month 
and have played at least two rounds of golf with you. Sponsors may sponsor only 1 or 2 players each year. Sponsors will be sent an email to confirm that they are sponsoring you. 
Please provide the following information for your sponsors:
  </li>
</ol>
	
<?php

	echo '<table style="border: none; margin-left:auto;margin-right:auto;">' . PHP_EOL;
	
    echo '<tr>' . PHP_EOL;
	echo '<td style="border: none;">1: Sponsor\'s Last Name</td>' . PHP_EOL;
	echo '<td style="border: none;"><input type="text" size="20"';
	echo '    name="Sponsor1LastName" value="' . $sponsor1LastName . '" required></td>' . PHP_EOL;
	echo '<td style="border: none;">GHIN Number</td>' . PHP_EOL;
	echo '<td style="border: none;"><input type="text" size="10" pattern="[0-9]+"';
	echo '    name="Sponsor1Ghin" value="' . $sponsor1Ghin . '" required></td>' . PHP_EOL;
	echo '<td style="border: none;">Cell Number</td>' . PHP_EOL;
	echo '<td style="border: none;"><input type="text" id="sp1phone" name="Sponsor1Phone"';
	echo '	pattern="[0-9]{3}-[0-9]{3}-[0-9]{4}" value="' . $sponsor1Phone . '" required><br><small>(Format: 123-456-7890)</small></td>' . PHP_EOL;
	echo '</tr>'  . PHP_EOL;

	echo '<tr>' . PHP_EOL;
	echo '<td style="border: none;">2: Sponsor\'s Last Name</td>' . PHP_EOL;
	echo '<td style="border: none;"><input type="text" size="20"';
	echo '    name="Sponsor2LastName" value="' . $sponsor2LastName . '" required></td>' . PHP_EOL;
	echo '<td style="border: none;">GHIN Number</td>' . PHP_EOL;
	echo '<td style="border: none;"><input type="text" size="10" pattern="[0-9]+"';
	echo '    name="Sponsor2Ghin" value="' . $sponsor2Ghin . '" required></td>' . PHP_EOL;
	echo '<td style="border: none;">Cell Number</td>' . PHP_EOL;
	echo '<td style="border: none;"><input type="text" id="sp2phone" name="Sponsor2Phone"';
	echo '	pattern="[0-9]{3}-[0-9]{3}-[0-9]{4}" value="' . $sponsor2Phone . '" required><br><small>(Format: 123-456-7890)</small></td>' . PHP_EOL;
	echo '</tr>'  . PHP_EOL;

	echo '</table>' . PHP_EOL;

	echo '<input type="submit" value="Submit Application And Pay Fee"> <br> <br>' . PHP_EOL;
	echo '</form>' . PHP_EOL;
	echo '</div><!-- #content -->' . PHP_EOL;
	echo '</div><!-- #content-container -->' . PHP_EOL;

	get_footer();

} else {

	$insert_id = InsertApplication($connection, $lastName, $firstName, $mailingAddress, $email, $ghin, $phoneNumber, $birthDate,
						$sponsor1LastName, $sponsor1Ghin, $sponsor1Phone,
						$sponsor2LastName, $sponsor2Ghin, $sponsor2Phone,
						$streetAddress, $city, $state, $zipCode);

	SendEmail($doNotReplyEmailAddress, $doNotReplyEmailPassword, $membershipEmail, "New application for " . $lastName . ', ' . $firstName, "New application submitted");

	InsertSponsor($connection, $insert_id, $sponsor1Ghin, $sponsor1LastName);
	InsertSponsor($connection, $insert_id, $sponsor2Ghin, $sponsor2LastName);

	SendApplicationSponsorEmail($connection, $doNotReplyEmailAddress, $doNotReplyEmailPassword, $sponsor1Ghin, $membershipEmail);
	SendApplicationSponsorEmail($connection, $doNotReplyEmailAddress, $doNotReplyEmailPassword, $sponsor2Ghin, $membershipEmail);

	// Redirect to payment page after clearing output buffer
	ob_start();
	header("Location: pay_initiation_fee.php?application_id=" . $insert_id);
}

function InsertApplication($connection, $lastName, $firstName, $mailingAddress, $email, $ghin, $phoneNumber, $birthDate,
								$sponsor1LastName, $sponsor1Ghin, $sponsor1Phone,
								$sponsor2LastName, $sponsor2Ghin, $sponsor2Phone,
								$streetAddress, $city, $state, $zipCode) {

	$sqlCmd = "INSERT INTO `MembershipApplication` VALUES (NULL, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
	$insert = $connection->prepare ( $sqlCmd );
	
	if (! $insert) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	$active = 1;
	$dateTimeAdded = date ( 'Y-m-d H:i:s' );
	$payment = 0;
	$payerName = "";
	$paymentDateTime = "";
	
	// Record Key
	// Active
	// LastName
	// FirstName
	// Mailing Address (now obsolete)
	// Email
	// GHIN
	// Phone Number
	// Birth Date
	// Sponsor 1: last name, ghin, phone number
	// Sponsor 2: last name, ghin, phone number
	// Date Time Added
	// Payment
	// Payment Date Time
	// Payer Name
	// Street Address
	// City
	// State
	// Zip Code
	if (! $insert->bind_param ( 'issssisssissississssss', $active, $lastName, $firstName, $mailingAddress, $email, $ghin, $phoneNumber, $birthDate,
													$sponsor1LastName, $sponsor1Ghin, $sponsor1Phone,
													$sponsor2LastName, $sponsor2Ghin, $sponsor2Phone,
													$dateTimeAdded, $payment, $paymentDateTime, $payerName,
													$streetAddress, $city, $state, $zipCode )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}
	
	if (! $insert->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}

	// insert_id: Returns the ID generated by an INSERT or UPDATE query on a table with a column having the AUTO_INCREMENT attribute. 
	// In the case of a multiple-row INSERT statement, it returns the first automatically generated value that was successfully inserted.
	//echo 'insert id is: ' . $insert->insert_id . '<br>';
	return $insert->insert_id;
}

function CheckGhin($connection, $lastName, $ghin){

	$rosterEntry = GetRosterEntry ( $connection, $ghin );

	if (empty ( $rosterEntry )) {
		return 'GHIN ' . $ghin . " is not a member of the Coronado Men's Golf Club";
	} else if(!$rosterEntry->Active) {
		return 'GHIN ' . $ghin . " is not an active member of the Coronado Men's Golf Club";
	} else {
		if (strpos($rosterEntry->LastName, ' ') !== FALSE){
			// Only compare the part before the space
			$nameArray1 = explode(' ', $rosterEntry->LastName);
			$nameArray2 = explode(' ', $lastName);
			$lastNamesMatch = strcasecmp ( $nameArray1[0], $nameArray2[0] ) == 0;
		} else {
			$lastNamesMatch = strcasecmp ( $lastName, $rosterEntry->LastName ) == 0;
		}
		
		if (!$lastNamesMatch) {
			return 'Last name for GHIN ' . $ghin . ' is not ' . $lastName;
		}

		$now = new DateTime ( "now" );
		$memberAdded = new DateTime($rosterEntry->DateAdded);
		$interval = $now->diff($memberAdded);
		//return 'interval years: ' . $interval->y . ' months: ' . $interval->m;
		if($interval->y < 1){
			return $lastName . '(' . $ghin . ')' . ' has not been a member for 12 months yet';
		}

		$pastSponsorships = CountPastSponsorships($connection, $ghin);
		if($pastSponsorships >= 2){
			return 'Sponsor ' . $lastName . ' has already sponsored ' . $pastSponsorships . ' other players this calendar year and cannot sponsor any more.';
		}
		
	}

	return "";
}

function InsertSponsor($connection, $insert_id, $sponsorGhin, $sponsorLastName) {
	$sqlCmd = "INSERT INTO `MembershipSponsors` VALUES (?, ?, ?, ?, ?)";
	$insert = $connection->prepare ( $sqlCmd );
	
	if (! $insert) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}

	$confirmed = 0;
	$dateAdded = date ( 'Y-m-d' );

	if (! $insert->bind_param ( 'isisi', $insert_id, $dateAdded, $sponsorGhin, $sponsorLastName, $confirmed )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}
	
	if (! $insert->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
}

function CountPastSponsorships($connection, $ghin){

	$sqlCmd = "SELECT DateAdded FROM `MembershipSponsors` WHERE `GHIN` = ?";
	$query = $connection->prepare ( $sqlCmd );

	if (! $query->bind_param ( 'i', $ghin )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}

	if (! $query) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}

	if (! $query->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}

	$query->bind_result ($date);

	$now = new DateTime ( "now" );
	$thisYear = $now->format('Y');

	$count = 0;
	while ( $query->fetch () ) {
		$year = date('Y', strtotime($date));

		// Count how many sponsorships this year
		if(strcmp($thisYear, $year) == 0){
			$count++;
		}
	}

	return $count;
}

function CheckForExistingApplication($connection, $lastName, $firstName, $ghin){

	$sqlCmd = "SELECT LastName,FirstName,GHIN,Payment FROM `MembershipApplication` WHERE `Active` = 1 AND `LastName` = ? AND `FirstName` = ? AND `GHIN` = ?";
	$query = $connection->prepare ( $sqlCmd );

	if (! $query->bind_param ( 'ssi', $lastName, $firstName, $ghin )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}

	if (! $query) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}

	if (! $query->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}

	$query->bind_result ($lastName, $firstName, $ghin, $payment );

	$foundRecord = false;
	while ( $query->fetch () ) {
		$foundRecord = true;
	}

	return $foundRecord;
}

function SendApplicationSponsorEmail($connection, $from, $fromPassword, $sponsorGhin, $replyTo){
    try {
		$rosterEntry = GetRosterEntry ( $connection, $sponsorGhin );

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
		$mail->AddAddress($rosterEntry->Email);

        //Recipients
        $mail->setFrom($from, 'CMGC Membership Application');          //This is the email your form sends From
		$mail->addReplyTo($replyTo, 'Membership Chairman');

        //$mail->addAddress($to, ''); // Add a recipient address
        //$mail->addAddress('contact@example.com');               // Name is optional
        //$mail->addCC('cc@example.com');
        //$mail->addBCC('bcc@example.com');

        //Attachments
        //$mail->addAttachment('/var/tmp/file.tar.gz');         // Add attachments
        //$mail->addAttachment('/tmp/image.jpg', 'new.jpg');    // Optional name

        //Content
        //$mail->isHTML(false);                                  // Set email format to HTML
        $mail->Subject = "Coronado Men's Club new member sponsor";
        $mail->msgHTML("testing sponsor email<p><b>paragraph 2</b>");
        //$mail->AltBody = 'This is the body in plain text for non-HTML mail clients';

        $mail->send();
        //echo 'Message has been sent';
    } catch (Exception $e) {
        echo '<p>Message could not be sent. Mailer Error: ' . $mail->ErrorInfo . "</p>";
    }
}

function TestInput($data) {
	if(empty($data)){
		return $data;
	}
	$data = trim($data);
	$data = stripslashes($data);
	$data = htmlspecialchars($data);
	return $data;
  }

?>