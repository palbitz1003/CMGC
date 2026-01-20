<?php

function cmgc_admin_membership_application_page2()
 {
    // Putting require_once at the top of this file didn't work
    require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';

    $connection = new mysqli ('p:' . $db_hostname, $db_username, $db_password, $db_database );
    
    if ($connection->connect_error){
        echo 'Database connection error: ' .  $connection->connect_error . "<br>";
        return;
    }

    // Read the application setup details (open/count/start date)
	$sqlCmd = "SELECT * FROM `MembershipDetails`";
	$query = $connection->prepare ( $sqlCmd );

	if (! $query) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}

	if (! $query->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}

	$query->bind_result ($allow, $max, $start);

    $open = 0;
	$maxApplications = 0;
	$startDate = "2000-01-01";
	if($query->fetch ()) {
        $open  = $allow;
        $maxApplications = $max;
        $startDate = $start;
    }
    $query->close ();

    
    // Read the application table
    $sqlCmd = "SELECT * FROM `MembershipApplication` WHERE `Active` = 1 ORDER BY `DateTimeAdded` ASC";
    $query = $connection->prepare ( $sqlCmd );
    
    if (! $query) {
        die ( $sqlCmd . " prepare failed: " . $connection->error );
    }
    
    if (! $query->execute ()) {
        die ( $sqlCmd . " execute failed: " . $connection->error );
    }

    class MembershipApplication {
        public $RecordKey;
        public $Active;
        public $LastName;
        public $FirstName;
        public $MailingAddress;
        public $Email;
        public $GHIN;
        public $PhoneNumber;
        public $BirthDate;
        public $Sponsor1LastName;
        public $Sponsor1Ghin;
        public $Sponsor1PhoneNumber;
        public $Sponsor2LastName;
        public $Sponsor2Ghin;
        public $Sponsor2PhoneNumber;
        public $DateTimeAdded;
        public $Payment;
        public $PaymentDateTime;
        public $PayerName;
        public $StreetAddress;
        public $City;
        public $State;
        public $ZipCode;
    }
    
    $query->bind_result ( $recordKey, $active, $lastName, $firstName, $mailingAddress, $email, $ghin, $phoneNumber, $birthDate,
                        $sponsor1LastName, $sponsor1Ghin, $sponsor1PhoneNumber,
                        $sponsor2LastName, $sponsor2Ghin, $sponsor2PhoneNumber,
                        $dateTimeAdded, $payment, $paymentDateTime, $payerName,
                        $streetAddress, $city, $state, $ZipCode );
    
    $membershipApplicationEntries = array();
    while ( $query->fetch () ) {
        $membershipApplication = new MembershipApplication();
        $membershipApplication->RecordKey = $recordKey;
        $membershipApplication->Active = $active;
        $membershipApplication->LastName = $lastName;
        $membershipApplication->FirstName = $firstName;
        $membershipApplication->MailingAddress = $mailingAddress;
        $membershipApplication->Email = $email;
        $membershipApplication->GHIN = $ghin;
        $membershipApplication->PhoneNumber = $phoneNumber;
        $membershipApplication->BirthDate = $birthDate;
        $membershipApplication->Sponsor1LastName = $sponsor1LastName;
        $membershipApplication->Sponsor1Ghin = $sponsor1Ghin;
        $membershipApplication->Sponsor1PhoneNumber = $sponsor1PhoneNumber;
        $membershipApplication->Sponsor2LastName = $sponsor2LastName;
        $membershipApplication->Sponsor2Ghin = $sponsor2Ghin;
        $membershipApplication->Sponsor2PhoneNumber = $sponsor2PhoneNumber;
        $membershipApplication->DateTimeAdded = $dateTimeAdded;
        $membershipApplication->Payment = $payment;
        $membershipApplication->PaymentDateTime = $paymentDateTime;
        $membershipApplication->PayerName = $payerName;
        $membershipApplication->StreetAddress = $streetAddress;
        $membershipApplication->City = $city;
        $membershipApplication->State = $state;
        $membershipApplication->ZipCode = $ZipCode;

        $membershipApplicationEntries[] = $membershipApplication;
    }

    $query->close ();

    // Read the sponsor table 
    $sqlCmd = "SELECT * FROM `MembershipSponsors`";
    $query = $connection->prepare ( $sqlCmd );
    
    if (! $query) {
        die ( $sqlCmd . " prepare failed: " . $connection->error );
    }
    
    if (! $query->execute ()) {
        die ( $sqlCmd . " execute failed: " . $connection->error );
    }

    class MembershipApplicationSponsor {
        public $ApplicationRecordKey;
        public $DateAdded;
        public $SponsorGhin;
        public $SponsorLastName;
        public $Confirmed;
    }

    $query->bind_result ( $applicationRecordKey, $dateAdded, $sponsorGhin, $sponsorLastName, $confirmed );

    $membershipApplicationSponsorEntries = array();
    while ( $query->fetch () ) {
        $membershipApplicationSponsor = new MembershipApplicationSponsor();
        $membershipApplicationSponsor->ApplicationRecordKey = $applicationRecordKey;
        $membershipApplicationSponsor->DateAdded = $dateAdded;
        $membershipApplicationSponsor->SponsorGhin = $sponsorGhin;
        $membershipApplicationSponsor->SponsorLastName = $sponsorLastName;
        $membershipApplicationSponsor->Confirmed = $confirmed;

        $membershipApplicationSponsorEntries[] = $membershipApplicationSponsor;
    }
    
    $query->close ();


    $adminUrl = admin_url( 'admin.php' );

    echo '<br><br><form method="POST" enctype="multipart/form-data" action="' . $adminUrl . '">' . PHP_EOL;
    echo '<input type="hidden" name="action" value="cmgc_admin_update_application_details">' . PHP_EOL;
    echo 'Accept appications on start date: <input name="AcceptApplications" type="checkbox" value="1" ';
    if($open){
        echo 'checked';
    }
    echo '>' . PHP_EOL;
    echo '&nbsp;&nbsp;&nbsp;max applications: <input name="MaxApplications" type="number" min="0" max="200" value="' . $maxApplications . '">' . PHP_EOL;
    echo '&nbsp;&nbsp;&nbsp;start date: <input name="ApplicationStartDate" type="date" value="' . $startDate .  '">' . PHP_EOL;
    echo '&nbsp;&nbsp;&nbsp;<input type="submit" name="Update" value="Update Details" class="button-primary">' . PHP_EOL;
    echo '</form>' . PHP_EOL;

    echo '<h2>Pending Membership Applications</h2>' ;
    if (! $membershipApplicationEntries || (count ( $membershipApplicationEntries ) == 0)) {
        echo "none pending";
        return;
    }

    
    echo '<form method="POST" enctype="multipart/form-data" action="' . $adminUrl . '">' . PHP_EOL;
    echo '       <input type="hidden" name="action" value="cmgc_admin_clear_applications">' . PHP_EOL;

    // Table class can be widefat, fixed, or striped
    echo '<table class="fixed" >' . PHP_EOL;
    echo '<thead><tr><th>Clear</th><th>Date Added</th><th>Name</th><th>GHIN</th><th>Email Address</th><th>Street Address</th><th>City</th><th>State</th><th>Zip</th><th>DOB</th><th>Phone</th>';
    echo '<th>Sp1 Last</th><th>Sp1 GHIN</th><th>Sp1 Phone</th><th>Sp2 Last</th><th>Sp2 GHIN</th><th>Sp2 Phone</th><th>Payment</th><th>Payment Date</th><th>Payer Name</th></tr></thead>' . PHP_EOL;
    echo '<tbody>' . PHP_EOL;

    
    for($i = 0; $i < count ( $membershipApplicationEntries ); ++ $i) {

        echo '<tr>' . PHP_EOL;
        echo '<td style="padding: 0px 10px 0px 10px;"><input type="checkbox" name="RecordKey[]" value="' . $membershipApplicationEntries[$i]->RecordKey . '"></td>' . PHP_EOL;
        echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationEntries[$i]->DateTimeAdded . '</td>' . PHP_EOL;
        echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationEntries[$i]->LastName . ', ' .  $membershipApplicationEntries[$i]->FirstName .'</td>' . PHP_EOL;
        echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationEntries[$i]->GHIN . '</td>' . PHP_EOL;
        echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationEntries[$i]->Email . '</td>' . PHP_EOL;
        echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationEntries[$i]->StreetAddress . '</td>' . PHP_EOL;
        echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationEntries[$i]->City . '</td>' . PHP_EOL;
        echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationEntries[$i]->State . '</td>' . PHP_EOL;
        echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationEntries[$i]->ZipCode . '</td>' . PHP_EOL;
        echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationEntries[$i]->BirthDate . '</td>' . PHP_EOL;
        echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationEntries[$i]->PhoneNumber . '</td>' . PHP_EOL;
        echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationEntries[$i]->Sponsor1LastName . '</td>' . PHP_EOL;
        echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationEntries[$i]->Sponsor1Ghin . '</td>' . PHP_EOL;
        echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationEntries[$i]->Sponsor1PhoneNumber . '</td>' . PHP_EOL;
        echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationEntries[$i]->Sponsor2LastName . '</td>' . PHP_EOL;
        echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationEntries[$i]->Sponsor2Ghin . '</td>' . PHP_EOL;
        echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationEntries[$i]->Sponsor2PhoneNumber . '</td>' . PHP_EOL;
        echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationEntries[$i]->Payment . '</td>' . PHP_EOL;
        echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationEntries[$i]->PaymentDateTime . '</td>' . PHP_EOL;
        echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationEntries[$i]->PayerName . '</td>' . PHP_EOL;
        echo '</tr>' . PHP_EOL;
    }
    
    echo '</tbody>' . PHP_EOL;
    echo '</table>' . PHP_EOL;

    echo '<br><br><input type="submit" name="Clear" value="Clear Checked Applications" class="button-primary">' . PHP_EOL;
    echo '</form>' . PHP_EOL;

    // Show the sponsors after the applications
    echo '<br><br><table class="fixed" >' . PHP_EOL;
    echo '<thead><tr><th>Applicant</th><th>Sponsor</th><th>Sponsor GHIN</th><th style="padding: 0px 10px 0px 10px;">Confirmed</th><th></th></tr></thead>' . PHP_EOL;
    echo '<tbody>' . PHP_EOL;

    for($i = 0; $i < count ( $membershipApplicationEntries ); ++ $i) {
        for($j = 0; $j < count($membershipApplicationSponsorEntries); ++ $j){
            if($membershipApplicationEntries[$i]->RecordKey == $membershipApplicationSponsorEntries[$j]->ApplicationRecordKey){
                echo '<tr>' . PHP_EOL;
                echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationEntries[$i]->LastName . ', ' .  $membershipApplicationEntries[$i]->FirstName .'</td>' . PHP_EOL;
                echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationSponsorEntries[$j]->SponsorLastName . '</td>' . PHP_EOL;
                echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationSponsorEntries[$j]->SponsorGhin . '</td>' . PHP_EOL;
                $sponsorConfirmed = "no";
                if($membershipApplicationSponsorEntries[$j]->Confirmed != 0){
                    $sponsorConfirmed = "yes";
                }
                echo '<td style="padding: 0px 10px 0px 10px;">' . $sponsorConfirmed . '</td>' . PHP_EOL;

                echo '<td style="padding: 0px 10px 0px 10px;">' . PHP_EOL;
                if($membershipApplicationSponsorEntries[$j]->Confirmed == 0){
                    // Show confirm button when sponsor has not been confirmed yet
                    echo '<form method="POST" enctype="multipart/form-data" action="' . $adminUrl . '">' . PHP_EOL;
                    echo '  <input type="hidden" name="action" value="cmgc_admin_confirm_sponsor">' . PHP_EOL;
                    echo '  <input type="hidden" name="RecordKey" value="' . $membershipApplicationSponsorEntries[$j]->ApplicationRecordKey . '">' . PHP_EOL;
                    echo '  <input type="hidden" name="SponsorGhin" value="' . $membershipApplicationSponsorEntries[$j]->SponsorGhin . '">' . PHP_EOL;
                    echo '  <input type="submit" name="Confirm" value="Confirm" class="button-primary">' . PHP_EOL;
                echo '</form>' . PHP_EOL;
                }
                echo '</td>' . PHP_EOL;

                echo '</tr>' . PHP_EOL;
            }
        }
    }

    echo '</tbody>' . PHP_EOL;
    echo '</table>' . PHP_EOL;
 }

 function cmgc_admin_update_application_details_action2()
 {
    // Putting require_once at the top of this file didn't work
    require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';

    if($_POST["action"] === "cmgc_admin_update_application_details"){
        
        $connection = new mysqli ('p:' . $db_hostname, $db_username, $db_password, $db_database );
        
        if ($connection->connect_error){
            echo 'Database connection error: ' .  $connection->connect_error . "<br>";
            return false;
        }

        $acceptApplications = 0;
        $maxApplications = 0;
        $applicationStartDate="3000-01-01";
        // If checkbox is not set, then no $_POST variable
        // If checkbox is set, value is 1
        if(isset($_POST['AcceptApplications'])){
            $acceptApplications = 1;
        }
        
        // Always set
        if(isset($_POST['MaxApplications'])){
            $maxApplications = $_POST['MaxApplications'];
        }
        
        // Always set
        if(isset($_POST['ApplicationStartDate'])){
            $applicationStartDate = $_POST['ApplicationStartDate'];
        }
        
        $sqlCmd = "UPDATE `MembershipDetails` SET  `AllowApplications` = ?, `MaxApplications` = ?, `StartDate` = ?";
		$update = $connection->prepare ( $sqlCmd );
			
        if (! $update) {
            die ( $sqlCmd . " prepare failed: " . $connection->error );
        }
    
        if (! $update->bind_param ( 'iis', $acceptApplications, $maxApplications, $applicationStartDate)) {
            die ( $sqlCmd . " bind_param failed: " . $connection->error );
        }
        
        if (! $update->execute ()) {
            die ( $sqlCmd . " execute failed: " . $connection->error );
        }
        $update->close ();

        //echo "open is " . $acceptApplications . "<br>";
        //echo "max applications is " . $maxApplications . "<br>";
        //echo "application start is " . $applicationStartDate . "<br>";
    }

    return true;
 }


 function cmgc_admin_clear_applications_action2()
 {
    // Putting require_once at the top of this file didn't work
    require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';

    if($_POST["action"] === "cmgc_admin_clear_applications"){
        
        $connection = new mysqli ('p:' . $db_hostname, $db_username, $db_password, $db_database );
        
        if ($connection->connect_error){
            echo 'Database connection error: ' .  $connection->connect_error . "<br>";
            return false;
        }

        if(isset($_POST['RecordKey'])){
            foreach($_POST['RecordKey'] as $recordKey){
                //echo 'clear ' . $recordKey . "<br>";

                $sqlCmd = "UPDATE `MembershipApplication` SET  `Active` = 0 WHERE `RecordKey` = ?";
			    $update = $connection->prepare ( $sqlCmd );
			
                if (! $update) {
                    die ( $sqlCmd . " prepare failed: " . $connection->error );
                }
			
                if (! $update->bind_param ( 'i', $recordKey)) {
                    die ( $sqlCmd . " bind_param failed: " . $connection->error );
                }
                
                if (! $update->execute ()) {
                    die ( $sqlCmd . " execute failed: " . $connection->error );
                }
                $update->close ();
            }
        }
    }

    return true;
}

function cmgc_admin_confirm_sponsor_action2()
{
   // Putting require_once at the top of this file didn't work
   require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';

   if($_POST["action"] === "cmgc_admin_confirm_sponsor"){
       
        //echo "Record Key is " . $_POST['RecordKey'] . " and sponsor ghin is " . $_POST['SponsorGhin'];

        if(isset($_POST['RecordKey']) && isset($_POST['SponsorGhin'])){
        
            $connection = new mysqli ('p:' . $db_hostname, $db_username, $db_password, $db_database );
        
            if ($connection->connect_error){
                echo 'Database connection error: ' .  $connection->connect_error . "<br>";
                return false;
            }

            $sqlCmd = "UPDATE `MembershipSponsors` SET  `Confirmed` = 1 WHERE `ApplicationRecordKey` = ? AND `GHIN` = ?";
            $update = $connection->prepare ( $sqlCmd );
        
            if (! $update) {
                die ( $sqlCmd . " prepare failed: " . $connection->error );
            }
        
            if (! $update->bind_param ( 'ii', $_POST['RecordKey'], $_POST['SponsorGhin'])) {
                die ( $sqlCmd . " bind_param failed: " . $connection->error );
            }
            
            if (! $update->execute ()) {
                die ( $sqlCmd . " execute failed: " . $connection->error );
            }
            $update->close ();

        } else {
            echo "Missing parameter: Record Key is " . $_POST['RecordKey'] . " and sponsor ghin is " . $_POST['SponsorGhin'];
            return false;
        }
    }

   return true;
}

?>