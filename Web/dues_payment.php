<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/signup functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/dues_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$testMode = false;
//if($_GET ['mode'] == "test"){
//	$testMode = true;
//}

$connection = new mysqli ( $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( $connection->connect_error );

$error = "";
$GHIN = "";
$LastName = "";
$FullName = "";
$playerDues = null;

$updatedDOB = "";
$updatedEmail = "";
$updatedPhone = "";

$now = new DateTime ( "now" );
$startDues = GetDuesStartDate();
$endExtendedDues = GetDuesEndExtendedDate();

if(!(($now >= $startDues) && ($now < $endExtendedDues)) && $_GET['skipdatecheck'] !== '1')
{
	get_header ();
	get_sidebar ();
	echo 'You can pay your dues only between ' . $startDues->format('M d, Y') . ' and ' . $endExtendedDues->format( 'M d, Y');
	get_footer ();
	return;
}


if (isset ( $_POST ['Player'] )) {
	
		$GHIN = trim ( $_POST ['Player'] ['GHIN'] );
		$LastName = trim ( $_POST ['Player'] ['LastName'] );
		$BirthMonth = trim ($_POST ['Player'] ['BirthMonth']);
		$BirthDay = trim ($_POST ['Player'] ['BirthDay']);
		$BirthYear = trim ($_POST ['Player'] ['BirthYear']);
		$Email = trim ($_POST ['Player'] ['Email']);
		$Phone = trim($_POST ['Player'] ['Phone']);
		
		// Remove any slashes before quotes and remove single quotes
		// as these can be used for "SQL Injection".
		$GHIN = stripslashes($GHIN);
		$GHIN = str_replace("'", "", $GHIN);
		$LastName = stripslashes ( $LastName ); 
		$LastName = str_replace("'", "", $LastName);
		$BirthMonth = stripslashes ( $BirthMonth ); 
		$BirthMonth = str_replace("'", "", $BirthMonth);
		$BirthDay = stripslashes ( $BirthDay ); 
		$BirthDay = str_replace("'", "", $BirthDay);
		$BirthYear = stripslashes ( $BirthYear ); 
		$BirthYear = str_replace("'", "", $BirthYear);
		$Email = stripslashes ( $Email ); 
		$Email = str_replace("'", "", $Email);
		$Phone = stripslashes ( $Phone ); 
		$Phone = str_replace("'", "", $Phone);
	
		// Check that both GHIN and Last Name were filled in
		if(empty($GHIN) && empty($LastName)){
			$error = 'GHIN and Last Name must be filled in';
		} else if (! empty ( $GHIN ) && empty ( $LastName )) {
			$error = 'Last Name must be filled in';
		} else if (empty ( $GHIN ) && ! empty ( $LastName )) {
			$error = 'GHIN must be filled in';
		} else if (! empty ( $GHIN ) && ! empty ( $LastName )) {
			// TODO: Check for player already paid (may also be in table, but not yet paid)
			$playerDues = GetPlayerDues($connection, $GHIN);
			if (!empty($playerDues) && ($playerDues->Payment > 0)) {
				$error = 'Player ' . $LastName . ' (' . $GHIN . ') has already payed dues';
			} else {
				// Check that last name matches GHIN database
				$rosterEntry = GetRosterEntry ( $connection, $GHIN );
				if (empty ( $rosterEntry )) {
					$error = 'GHIN ' . $GHIN . " is not a member of the Coronado Men's Golf Club";
				} else if (strcasecmp ( $LastName, $rosterEntry->LastName ) != 0) {
					$error = 'Last name for GHIN ' . $GHIN . ' is not ' . $LastName;
				} else if(!$rosterEntry->Active) {
					$error = 'GHIN ' . $GHIN . " is not an active member of the Coronado Men's Golf Club";
				} else {
					// Use the database casing for the last name
					$LastName = $rosterEntry->LastName;
					$FullName = $rosterEntry->LastName . ', ' . $rosterEntry->FirstName;
				}
			}
	}
	
	if(empty($error)){
		$now = new DateTime ( "now" );
		$year = $now->format('Y');
		
		if(empty($BirthMonth) || empty($BirthDay) || empty($BirthYear)){
			$error = "You must fill in your birthdate.";
		} else if(empty($Email)){
			$error = 'Fill in an email address. If you do not have an email address, type in "none"';
		} else if(!ctype_digit($BirthMonth) || ($BirthMonth < 1) || ($BirthMonth > 12)){
			$error = "Invalid birth month. Valid values are 1 to 12.";
		} else if(!ctype_digit($BirthDay) || ($BirthDay < 1) || ($BirthDay > 31)){
			$error = "Invalid birth day. Valid values are 1 to 31";
		} else if(!ctype_digit($BirthYear) || ($BirthYear < 1900) || ($BirthYear > $year)){
			$error = "Invalid year. Valid values are 1900 to $year";
		} else if(strtolower($Email) != "none"  && !filter_var($Email, FILTER_VALIDATE_EMAIL)){
			$error = "Invalid email address";
		} else 
		{
			$changed = null;
			$dob = $BirthYear . '-' . FormatMonthOrDay($BirthMonth)  . '-' . FormatMonthOrDay($BirthDay);
			if(empty($rosterEntry->BirthDate) || strcmp($dob, $rosterEntry->BirthDate) != 0){
				$updatedDOB = $dob;
			}
			if ((strtolower($Email) != "none") &&
				 (empty($rosterEntry->Email) ||
				 (strcasecmp($Email, $rosterEntry->Email) != 0))) {
				$updatedEmail = $Email;
			}

			// Phone number is not in the roster yet
			$updatedPhone = $Phone;
		}
	}
	
	if(empty($error)){
		$now = new DateTime ( "now" );
		$year = $now->format('Y');
		
		$endBasicDues = GetDuesEndBasicDate();
		$endExtendedDues = GetDuesEndExtendedDate();
		
		$membershipType = $rosterEntry->MembershipType;
		if(($now > $endBasicDues) && ($now < $endExtendedDues))
		{
			$membershipType = $membershipType . "_Late";
		}
		if($now > $endExtendedDues)
		{
			die("Error: Last day for payments was " . $endExtendedDues->format('Y-m-d'));
		}
		if($testMode){
			$membershipType = "Test";
		}
		
		$paypalDetails = GetPayPalDuesDetails($connection, $membershipType);
		
		if(empty($paypalDetails)){
			die("Error: Membership type " . $membershipType . " is not in the PayPal Dues table");
		} else if($paypalDetails->TournamentFee == 0){
			$error = "Membership type " . $rosterEntry->MembershipType . " does not require a yearly dues payment.";
		}
		else if(empty($paypalDetails->PayPayButton)) {
			die("Error: No PayPal button for membership type " . $membershipType);
		}
	}
}

$overrideTitle = "Pay Dues";
get_header ();

get_sidebar ();

//var_dump($_POST);

// If this page has not been filled in or there is an error, show the form
if (!empty($error) || !isset ( $_POST ['Player'] )) {

	$dues = GetPayPalDuesDetails($connection, 'R');
	$extendedDues = GetPayPalDuesDetails($connection, 'R_Late');
	$scgaOnly = GetPayPalDuesDetails($connection, 'L');
	
	echo '<div id="content-container" class="entry-content">' . PHP_EOL;
	echo '<div id="content" role="main">' . PHP_EOL;
	echo '<h2 class="entry-title" style="text-align:center">Pay Yearly Dues</h2>' . PHP_EOL;
	
	echo '<p>Dues payment is through PayPal (no checks). The dues for regular members is $' . $dues->TournamentFee . ' before Oct 1. From Oct 1 through Oct 31, the dues are $' . $extendedDues->TournamentFee .'.';
	echo ' Life members pay the annual SCGA fee of $' . $scgaOnly->TournamentFee .'.' . PHP_EOL;
	echo '<p>After Oct 31, you will be dropped from membership automatically on Dec 31.</p>' . PHP_EOL;
	echo '<p>Fill in your GHIN and last name below.</p>' . PHP_EOL;

	echo '<p>This is only step 1.  After entering your GHIN, last name, date of birth, and email address and passing the verification step, you will be shown a page with a PayPal button to pay the yearly dues. Click on the PayPal button and complete the payment.</p>' . PHP_EOL;
	echo '<form name="input" method="post">' . PHP_EOL;
	
	echo '<table style="border: none;">' . PHP_EOL;
	
	echo '<tr>' . PHP_EOL;
	echo '<td style="border: none;">GHIN:</td>' . PHP_EOL;
	echo '<td style="border: none;"><input type="text" name="Player[GHIN]"';
	echo '    value="' . $GHIN . '"></td>' . PHP_EOL;
	echo '</tr>' . PHP_EOL;
	
	echo '<tr>' . PHP_EOL;
	echo '<td style="border: none;">Last Name:</td>' . PHP_EOL;
	echo '<td style="border: none;"><input type="text"';
	echo '    name="Player[LastName]" value="' . $LastName . '"></td>' . PHP_EOL;
	echo '</tr>'  . PHP_EOL;
	
	echo '<tr>' . PHP_EOL;
	echo '<td style="border: none;">Date of Birth:</td>' . PHP_EOL;
	echo '<td style="border: none;">Month: <input type="text" size = "2" name="Player[BirthMonth]" value="' . $BirthMonth . '">';
	echo '     Day: <input type="text" size = "2" name="Player[BirthDay]" value="' . $BirthDay . '">';
	echo '     Year: <input type="text" size = "4" name="Player[BirthYear]" value="' . $BirthYear . '">';
	echo '    (e.g.: 5/29/1958)</td>' . PHP_EOL;
	echo '</tr>'  . PHP_EOL;
	
	echo '<tr>' . PHP_EOL;
	echo '<td style="border: none;">Email Address:</td>' . PHP_EOL;
	echo '<td style="border: none;"><input type="text" size=50';
	echo '    name="Player[Email]" value="' . $Email . '"></td>' . PHP_EOL;
	echo '</tr>'  . PHP_EOL;

	echo '<tr>' . PHP_EOL;
	echo '<td style="border: none;">Phone Number:</td>' . PHP_EOL;
	echo '<td style="border: none;"><input type="text" id="phone" name="Player[Phone]"';
	echo '	pattern="[0-9]{3}-[0-9]{3}-[0-9]{4}" value="' . $Phone . '" required> <small>Format: 123-456-7890</small></td>' . PHP_EOL;
	echo '</tr>'  . PHP_EOL;
	//insert_error_line($error, 2);
	
	echo '</table>' . PHP_EOL;
	
	if(isset($error)){
		echo '<p style="color:red;">' . PHP_EOL;
		echo $error . PHP_EOL;
		echo '</p>' . PHP_EOL;
	}

	echo '<input type="submit" value="Step 1: Verify input data"> <br> <br>' . PHP_EOL;
	echo '</form>' . PHP_EOL;
	echo '</div><!-- #content -->' . PHP_EOL;
	echo '</div><!-- #content-container -->' . PHP_EOL;

} else {
	

	if(empty($playerDues)){
		InsertPlayerForDues($connection, $year + 1, $GHIN, $FullName);
		InsertRosterUpdates($connection, $year + 1, $GHIN, $FullName, $updatedDOB, $updatedEmail, $updatedPhone);
		
		// for testing
		//UpdateDuesDatabase($connection, $GHIN, 150, "Player 1", "Player Email", "original string from paypal");
		//SendDuesEmail($connection, $GHIN, 150, $web_site);
	}
	else {
		// There should already be a roster update record
		UpdateRosterUpdates($connection, $year + 1, $GHIN, $FullName, $updatedDOB, $updatedEmail, $updatedPhone);
	}
	
	echo '<div id="content-container" class="entry-content">' . PHP_EOL;
	echo '<div id="content" role="main">' . PHP_EOL;

	echo '<h2 class="entry-title" style="text-align:center">Pay Yearly Dues</h2>' . PHP_EOL;
	
	echo '<p>You must pay the dues now.  You have not payed your dues until the PayPal step is complete!</p>' . PHP_EOL;
	echo "<p>The link below takes you to PayPal to make your payment.  You can pay with credit card even if you do not have a PayPal account. No credit card or account information is kept on the Coronado Men's Golf web site.</p>" . PHP_EOL;
	echo '<p style="text-align: center;"><b>Dues: $' . number_format( $paypalDetails->TournamentFee, 2) . '</b></p>' . PHP_EOL;
	
	echo '<form style="text-align:center" action="https://www.paypal.com/cgi-bin/webscr" method="post" target="_top">' . PHP_EOL;
	echo '<input type="hidden" name="cmd" value="_s-xclick">' . PHP_EOL;
	echo '<input type="hidden" name="hosted_button_id" value="' . $paypalDetails->PayPayButton . '">' . PHP_EOL;
	echo '<input type="hidden" name="item_name" value="Yearly Dues">' . PHP_EOL;
	echo '<input type="hidden" name="custom" value="Yearly Dues;' . $GHIN . ';' . $LastName . '">' . PHP_EOL;
	echo '<input type="hidden" name="on0" value="Yearly Dues">' . PHP_EOL;
	echo '<input type="hidden" name="currency_code" value="USD">' . PHP_EOL;
	echo '<input type="hidden" name="notify_url" value="https://' . $web_site . '/' . $ipn_dues_file . '">' . PHP_EOL;
	echo '<input type="hidden" name="return" value="https://' . $web_site . '/' . $script_folder_href . 'dues_payment_completed.php">' . PHP_EOL;
	echo '<input type="hidden" name="rm" value="1">' . PHP_EOL;
	echo '<input type="image" src="https://www.paypalobjects.com/en_US/i/btn/btn_paynowCC_LG.gif" border="0" name="submit" alt="PayPal - The safer, easier way to pay online!">' . PHP_EOL;
	echo '<img alt="" border="0" src="https://www.paypalobjects.com/en_US/i/scr/pixel.gif" width="1" height="1">' . PHP_EOL;
	echo '</form>' . PHP_EOL;
	
	echo '</div><!-- #content -->' . PHP_EOL;
	echo '</div><!-- #content-container -->' . PHP_EOL;
} // end of else clause

if (isset ( $connection )) {
	$connection->close ();
}

function FormatMonthOrDay($number)
{
	if($number < 10){
		if($number[0] !== '0'){
			return '0' . $number;
		}
	}
	return $number;
}

get_footer ();
?>