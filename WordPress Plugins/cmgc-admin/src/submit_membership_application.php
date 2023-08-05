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

    echo '<h2>Pending Membership Applications</h2>' ;
    if (! $membershipApplicationEntries || (count ( $membershipApplicationEntries ) == 0)) {
        echo "none pending";
        return;
    }

    $adminUrl = admin_url( 'admin.php' );
    echo '<form method="POST" enctype="multipart/form-data" action="' . $adminUrl . '">' . PHP_EOL;
    echo '       <input type="hidden" name="action" value="cmgc_admin_clear_applications">' . PHP_EOL;

    // Table class can be widefat, fixed, or striped
    echo '<table class="fixed" >' . PHP_EOL;
    echo '<thead><tr><th>Clear</th><th>Date Added</th><th>Last</th><th>First</th><th>GHIN</th><th>Street Address</th><th>City</th><th>State</th><th>Zip</th><th>Email Address</th><th>Phone</th><th>DOB</th>';
    echo '<th>Sp1 Last</th><th>Sp1 GHIN</th><th>Sp1 Phone</th><th>Sp2 Last</th><th>Sp2 GHIN</th><th>Sp2 Phone</th><th>Payment</th><th>Payment Date</th><th>Payer Name</th></tr></thead>' . PHP_EOL;
    echo '<tbody>' . PHP_EOL;

    
    for($i = 0; $i < count ( $membershipApplicationEntries ); ++ $i) {

        echo '<tr>' . PHP_EOL;
        echo '<td style="padding: 0px 10px 0px 10px;"><input type="checkbox" name="RecordKey[]" value="' . $membershipApplicationEntries[$i]->RecordKey . '"></td>' . PHP_EOL;
        echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationEntries[$i]->DateTimeAdded . '</td>' . PHP_EOL;
        echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationEntries[$i]->LastName . '</td>' . PHP_EOL;
        echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationEntries[$i]->FirstName . '</td>' . PHP_EOL;
        echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationEntries[$i]->GHIN . '</td>' . PHP_EOL;

        //echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationEntries[$i]->MailingAddress . '</td>' . PHP_EOL;
        echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationEntries[$i]->StreetAddress . '</td>' . PHP_EOL;
        echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationEntries[$i]->City . '</td>' . PHP_EOL;
        echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationEntries[$i]->State . '</td>' . PHP_EOL;
        echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationEntries[$i]->ZipCode . '</td>' . PHP_EOL;

        echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationEntries[$i]->Email . '</td>' . PHP_EOL;
        echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationEntries[$i]->PhoneNumber . '</td>' . PHP_EOL;
        echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationEntries[$i]->BirthDate . '</td>' . PHP_EOL;
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
    
    // Finish the first column table.  Show the 2nd column table.
    echo '</tbody>' . PHP_EOL;
    echo '</table>' . PHP_EOL;

    echo '<br><br><input type="submit" name="Clear" value="Clear Checked Applications" class="button-primary">' . PHP_EOL;
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

 ?>