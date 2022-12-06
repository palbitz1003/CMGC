<?php

require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/signup functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/dues_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );


$testMode = false;

$connection = new mysqli ( $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( $connection->connect_error );

$error = "";
$ghin = "";
$lastName = "";
$firstName = "";
$birthMonth = "";
$birthDay = "";
$birthYear = "";
$email = "";
$email2 = "";
$phone = "";
$mailingAddress = "";
$sponsor1LastName = "";
$sponsor1Ghin = "";
$sponsor1Phone = "";
$sponsor2LastName = "";
$sponsor2Ghin = "";
$sponsor2Phone = "";

if ($_SERVER["REQUEST_METHOD"] == "POST") {
	$lastName = test_input($_POST["LastName"]);
	$firstName = test_input($_POST["FirstName"]);
	$mailingAddress = test_input($_POST["MailingAddress"]);
	$email = test_input($_POST["Email"]);
	$email2 = test_input($_POST["Email2"]);
	$ghin = test_input($_POST["GHIN"]);
	$phone = test_input($_POST["Phone"]);
	$birthMonth = test_input($_POST["BirthMonth"]);
	$birthDay = test_input($_POST["BirthDay"]);
	$birthYear = test_input($_POST["BirthYear"]);
	$sponsor1LastName = test_input($_POST["Sponsor1LastName"]);
	$sponsor1Ghin = test_input($_POST["Sponsor1Ghin"]);
	$sponsor1Phone = test_input($_POST["Sponsor1Phone"]);
	$sponsor2LastName = test_input($_POST["Sponsor2LastName"]);
	$sponsor2Ghin = test_input($_POST["Sponsor2Ghin"]);
	$sponsor2Phone = test_input($_POST["Sponsor1Phone"]);

	$error = "none yet";
	if(strcasecmp($email, $email2) != 0){
		$error = "Email addresses are different: " . $email . ", " . $email2;
	}
	else if(!checkdate($birthMonth, $birthDay, $birthYear))
	{
		$error = "Date of birth is not a valid date: " . $birthMonth . "/" . $birthDay . "/" . $birthYear;
	}
	
}


$overrideTitle = "Membership Application";

get_header ();

get_sidebar ();

//var_dump($_POST);

// If this page has not been filled in or there is an error, show the form
if (!empty($error) || !isset ( $_POST ['LastName'] )) {

?>
	<div id="content-container" class="entry-content">
	<div id="content" role="main">
	<h2 class="entry-title" style="text-align:center">Coronado Men’s Golf Club (CMGC) Membership Application</h2>
<p>

<?php
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
	echo '<td style="border: none;">Mailing Address</td>' . PHP_EOL;
	echo '<td style="border: none;" colspan="3"><input type="text"';
	echo '    name="MailingAddress" size="70" value="' . $mailingAddress . '" required></td>' . PHP_EOL;
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
	echo '<td style="border: none;">Phone Number</td>' . PHP_EOL;
	echo '<td style="border: none;"><input type="text" id="phone" name="Phone"  size="25"';
	echo '	pattern="[0-9]{3}-[0-9]{3}-[0-9]{4}" value="' . $phone . '" required><br><small>(Format: 123-456-7890)</small></td>' . PHP_EOL;
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
The cost of membership for the Coronado Men’s Golf Club (CMGC) is an initiation fee of $300 plus one year’s annual dues, which is currently $160.00.
  </li>
  <li style="margin-bottom: 15px">
Once your application is complete, you must pay half of the initiation fee ($150) which is non-refundable. 
When you have paid your fees with the link provided you can find the waiting list on the club’s website, <a href="https://coronadomensgolf.org">coronadomensgolf.org</a>. 
Your sponsors will be verified by the CMGC Membership Chair. 
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
To ensure delivery of your membership invitation by email, please add (cmgcmembership1@gmail.com) to your address book. When you accept the membership invitation, 
you must pay the remaining balance of the initiation fee of $150 plus first year’s dues, which is currently $160 for a total of $310 to become a CMGC member.
  </li>
  <li style="margin-bottom: 15px">
Your CMGC sponsors must be members in good standing for a minimum of 12 months. Each of your sponsor’s states that they have known you for at least one month 
and have played at least two rounds of golf with you. The Membership Chair will verify your sponsors. Please provide the following information for your sponsors:
  </li>
</ol>
</p>
	
<?php

	echo '<table style="border: none; margin-left:auto;margin-right:auto;">' . PHP_EOL;
	
    echo '<tr>' . PHP_EOL;
	echo '<td style="border: none;">a: Sponsor\'s Last Name</td>' . PHP_EOL;
	echo '<td style="border: none;"><input type="text" size="20"';
	echo '    name="Sponsor1LastName" value="' . $sponsor1LastName . '" required></td>' . PHP_EOL;
	echo '<td style="border: none;">GHIN Number</td>' . PHP_EOL;
	echo '<td style="border: none;"><input type="text" size="10" pattern="[0-9]+"';
	echo '    name="Sponsor1Ghin" value="' . $sponsor1Ghin . '" required></td>' . PHP_EOL;
	echo '<td style="border: none;">Phone Number</td>' . PHP_EOL;
	echo '<td style="border: none;"><input type="text" id="phone" name="Sponsor1Phone"';
	echo '	pattern="[0-9]{3}-[0-9]{3}-[0-9]{4}" value="' . $sponsor1Phone . '" required><br><small>(Format: 123-456-7890)</small></td>' . PHP_EOL;
	echo '</tr>'  . PHP_EOL;

	echo '<tr>' . PHP_EOL;
	echo '<td style="border: none;">b: Sponsor\'s Last Name</td>' . PHP_EOL;
	echo '<td style="border: none;"><input type="text" size="20"';
	echo '    name="Sponsor2LastName" value="' . $sponsor2LastName . '" required></td>' . PHP_EOL;
	echo '<td style="border: none;">GHIN Number</td>' . PHP_EOL;
	echo '<td style="border: none;"><input type="text" size="10" pattern="[0-9]+"';
	echo '    name="Sponsor2Ghin" value="' . $sponsor2Ghin . '" required></td>' . PHP_EOL;
	echo '<td style="border: none;">Phone Number</td>' . PHP_EOL;
	echo '<td style="border: none;"><input type="text" id="phone" name="Sponsor2Phone"';
	echo '	pattern="[0-9]{3}-[0-9]{3}-[0-9]{4}" value="' . $sponsor2Phone . '" required><br><small>(Format: 123-456-7890)</small></td>' . PHP_EOL;
	echo '</tr>'  . PHP_EOL;

	echo '</table>' . PHP_EOL;
	
	if(isset($error)){
		echo '<p style="color:red;">' . PHP_EOL;
		echo $error . PHP_EOL;
		echo '</p>' . PHP_EOL;
	}

	echo '<input type="submit" value="Step 1: Submit Application"> <br> <br>' . PHP_EOL;
	echo '</form>' . PHP_EOL;
	echo '</div><!-- #content -->' . PHP_EOL;
	echo '</div><!-- #content-container -->' . PHP_EOL;

} else {
}

function test_input($data) {
	$data = trim($data);
	$data = stripslashes($data);
	$data = htmlspecialchars($data);
	return $data;
  }

?>