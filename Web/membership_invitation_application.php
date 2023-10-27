<?php

require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/signup functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/membership_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$testMode = false;

$waitingListRecordKey = $_GET ['waiting_list_id'];
if (! $waitingListRecordKey || !is_numeric($waitingListRecordKey)) {
	die ( "invalid waiting list entry" );
}

$connection = new mysqli ( $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error){
	die ( $connection->connect_error );
}

$waitingListEntry = GetWaitlistEntry($connection, $waitingListRecordKey);

if(empty($waitingListEntry) || empty($waitingListEntry->RecordKey)){
    cmgc_membership_invitation_error($connection, "Failed to find waiting list id " . $recordKey);
    die ();
}

if($waitingListEntry->PaymentDue == 0){
    cmgc_membership_invitation_error($connection, 'Waiting list player "' . $waitingListEntry->Name . '" has not been approved for final membership payment');
    die();
}

if($waitingListEntry->Payment > 0){
    cmgc_membership_invitation_error($connection, 'Waiting list player "' . $waitingListEntry->Name . '" has already paid');
    die();
}

$error = "";
$ghin = "";
$name = explode(",", $waitingListEntry->Name);
$lastName = trim($name[0]);
$firstName = trim($name[1]);
$birthMonth = "";
$birthDay = "";
$birthYear = "";
$birthDate = "";
$email = "";
$email2 = "";
$phoneNumber = "";
$streetAddress = "";
$city = "";
$state = "CA";
$zipCode = "";

// Remove single and double quotes?
//$LastName[$i] = str_replace("'", "", $LastName[$i]); // remove single quotes
//$LastName[$i] = str_replace('"', "", $LastName[$i]); // remove double quotes

if ($_SERVER["REQUEST_METHOD"] == "POST") {
    
	//$lastName = test_input($_POST["LastName"]);
	//$firstName = test_input($_POST["FirstName"]);
	$email = test_input($_POST["Email"]);
	$email2 = test_input($_POST["Email2"]);
	$ghin = test_input($_POST["GHIN"]);
	$phoneNumber = test_input($_POST["Phone"]);
	$birthMonth = test_input($_POST["BirthMonth"]);
	$birthDay = test_input($_POST["BirthDay"]);
	$birthYear = test_input($_POST["BirthYear"]);
	$streetAddress = test_input($_POST["StreetAddress"]);
	$city = test_input($_POST["City"]);
	$state = test_input($_POST["State"]);
	$zipCode = test_input($_POST["ZipCode"]);

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
}


$overrideTitle = "Membership Invitation Application";

//var_dump($_POST);

// If this page has not been filled in or there is an error, show the form
if (!empty($error) || !isset ( $_POST ['Email'] )) {
	get_header ();

?>
	<div id="content-container" class="entry-content">
	<div id="content" role="main">
	<h2 class="entry-title" style="text-align:center">Coronado Men’s Golf Club (CMGC) Membership Application</h2>
<p>

<?php
	if(isset($error)){
		echo '<p style="color:red;">' . PHP_EOL;
		echo $error . PHP_EOL;
		echo '</p>' . PHP_EOL;
	}

	echo '<form name="input" method="post">' . PHP_EOL;
	
	echo '<table style="border: none;margin-left:auto;margin-right:auto;">' . PHP_EOL;
	
    echo '<tr>' . PHP_EOL;
    /*
	echo '<td style="border: none;">Last Name</td>' . PHP_EOL;
	echo '<td style="border: none;"><input type="text" size="25"';
	echo '    name="LastName" value="' . $lastName . '" readonly></td>' . PHP_EOL;
	echo '<td style="border: none;">First Name</td>' . PHP_EOL;
	echo '<td style="border: none;"><input type="text" size="25"';
	echo '    name="FirstName" value="' . $firstName . '" readonly></td>' . PHP_EOL;
    */
    echo '<td style="border: none;">Name</td><td style="border: none;">' . $lastName . ', ' . $firstName . '</td><td style="border: none;"></td>' . PHP_EOL;
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
	echo '<td style="border: none;">Phone Number</td>' . PHP_EOL;
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
	
<?php

	echo '<input type="submit" value="Submit Information And Pay Fee"> <br> <br>' . PHP_EOL;
	echo '</form>' . PHP_EOL;
	echo '</div><!-- #content -->' . PHP_EOL;
	echo '</div><!-- #content-container -->' . PHP_EOL;

	get_footer();

} else {

    delete_any_existing_records($connection, $lastName, $firstName);

	$insert_id = InsertApplication($connection, $waitingListRecordKey, $lastName, $firstName, $email, $ghin, $phoneNumber, $birthDate,
						$streetAddress, $city, $state, $zipCode);

	// Redirect to payment page after clearing output buffer
	ob_start();
	header("Location: pay_membership.php?waiting_list_id=" . $waitingListRecordKey);
}

function InsertApplication($connection, $waitingListRecordKey, $lastName, $firstName, $email, $ghin, $phoneNumber, $birthDate,
								$streetAddress, $city, $state, $zipCode) {

	$sqlCmd = "INSERT INTO `MembershipInvitation` VALUES (NULL, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
	$insert = $connection->prepare ( $sqlCmd );
	
	if (! $insert) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	$active = 1;
	$dateAdded = date ( 'Y-m-d' );
	$payment = 0;
	$payerName = "";
	$paymentDateTime = "";
	
	// Record Key
	// Active
    // DateAdded
    // Waiting List Record Key
	// LastName
	// FirstName
	// Email
	// GHIN
	// Phone Number
	// Birth Date
	// Street Address
	// City
	// State
	// Zip Code
	if (! $insert->bind_param ( 'isisssissssss', $active, $dateAdded, $waitingListRecordKey, $lastName, $firstName, 
                                                    $email, $ghin, $phoneNumber, $birthDate,
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

function delete_any_existing_records($connection, $lastName, $firstName){

	$sqlCmd = "SELECT RecordKey FROM `MembershipInvitation` WHERE `Active` = 1 AND `LastName` = ? AND `FirstName` = ?";
	$query = $connection->prepare ( $sqlCmd );

	if (! $query->bind_param ( 'ss', $lastName, $firstName)) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}

	if (! $query) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}

	if (! $query->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}

	$query->bind_result ($recordKey);

	$foundRecord = false;
    $records = array();
	while ( $query->fetch () ) {
        $records[] = $recordKey;
	}

    // Delete existing records
    for($i = 0; $i < count($records); $i++){
        $sqlCmd = "DELETE FROM `MembershipInvitation` WHERE `Recordkey` = ?";

        $query = $connection->prepare ( $sqlCmd );

        //echo "deleting " . $records[$i] . '<br>';
        if (! $query->bind_param ( 'i', $records[$i])) {
            die ( $sqlCmd . " bind_param failed: " . $connection->error );
        }
    
        if (! $query) {
            die ( $sqlCmd . " prepare failed: " . $connection->error );
        }
    
        if (! $query->execute ()) {
            die ( $sqlCmd . " execute failed: " . $connection->error );
        }
    }
}

function cmgc_membership_invitation_error($connection, $error){
    get_header ();
    get_sidebar ();

    echo ' <div id="content-container" class="entry-content">';
    echo '    <div id="content" role="main">';

    if(!empty($error)){
        echo $error . "<br>";
    }
    echo '    </div><!-- #content -->';
    echo ' </div><!-- #content-container -->';

    $connection->close ();
    get_footer ();
}

function test_input($data) {
	if(empty($data)){
		return $data;
	}
	$data = trim($data);
	$data = stripslashes($data);
	$data = htmlspecialchars($data);
	return $data;
  }

?>