<?php
require_once plugin_dir_path(__FILE__) . 'functions.php';

function cmgc_admin_upload_waitlist_action2()
{
    class WaitingListEntry {
        public $RecordKey;
        public $Position;
        public $Name;
        public $DateAdded;
        public $PaymentDue;
    }

    /*
    The global predefined variable $_FILES is an associative array containing items uploaded via HTTP POST method. 
    Uploading a file requires HTTP POST method form with enctype attribute set to multipart/form-data.

    The _FILES array contains following properties −

    $_FILES['file']['name'] - The original name of the file to be uploaded.

    $_FILES['file']['type'] - The mime type of the file.

    $_FILES['file']['size'] - The size, in bytes, of the uploaded file.

    $_FILES['file']['tmp_name'] - The temporary filename of the file in which the uploaded file was stored on the server.

    $_FILES['file']['error'] - The error code associated with this file upload.
    */

    //print_r($_POST); 
    //print_r($_FILES); echo '<br>';

    $cmgc_admin_options = get_option('cmgc_admin_plugin_options', array());
    $cmgc_admin_options['waiting_list_upload_results'] = '';
    update_option('cmgc_admin_plugin_options', $cmgc_admin_options);

    $error = false;
    $waitingList = array();
    if($_POST["action"] === "cmgc_admin_upload_waitlist"){

        if(empty($_FILES["filename"]["name"])){
            $error = true;
            $cmgc_admin_options['waiting_list_upload_results'] = 'Error: No file chosen';
            update_option('cmgc_admin_plugin_options', $cmgc_admin_options);
        }
        else {
            //echo "Name is: " . $_FILES["filename"]["name"] . "<br>";
            $filename=$_FILES["filename"]["tmp_name"];    
            if($_FILES["filename"]["type"] === 'text/csv'){
                if($_FILES["filename"]["size"] > 0){
                    $file = fopen($filename, "r");
                    $lineNumber = 1;
                    
                    while (($error == false) && (($getData = fgetcsv($file, 10000, ",")) !== FALSE)){
                        //echo $getData[0] . ", " . $getData[1] . ", " . $getData[2] . '<br>';

                        // Get rid of any leading & trailing whitespace
                        if(!empty($getData[0])){
                            $getData[0] = trim($getData[0]);
                        }
                        if(!empty($getData[1])){
                            $getData[1] = trim($getData[1]);
                        }
                        if(!empty($getData[2])){
                            $getData[2] = trim($getData[2]);
                        }
                        if(!empty($getData[3])){
                            $getData[3] = trim($getData[3]);
                        }

                        if($lineNumber === 1){
                            if(empty($getData[0]) || strcasecmp($getData[0], 'position') != 0){
                                $error = true;
                                $cmgc_admin_options['waiting_list_upload_results'] = 'Error: Waiting list file line 1 does not have "POSITION" in field 1';
                                update_option('cmgc_admin_plugin_options', $cmgc_admin_options);
                            }
                            else if(empty($getData[1]) || strcasecmp($getData[1], 'name') != 0){
                                $error = true;
                                $cmgc_admin_options['waiting_list_upload_results'] = 'Error: Waiting list file line 1 does not have "NAME" in field 2';
                                update_option('cmgc_admin_plugin_options', $cmgc_admin_options);
                            }
                            else if(empty($getData[2]) || strcasecmp($getData[2], 'date added') != 0){
                                $error = true;
                                $cmgc_admin_options['waiting_list_upload_results'] = 'Error: Waiting list file line 1 does not have "DATE ADDED" in field 3';
                                update_option('cmgc_admin_plugin_options', $cmgc_admin_options);
                            }
                            else if(empty($getData[3]) || strcasecmp($getData[3], 'payment') != 0){
                                $error = true;
                                $cmgc_admin_options['waiting_list_upload_results'] = 'Error: Waiting list file line 1 does not have "PAYMENT" in field 4';
                                update_option('cmgc_admin_plugin_options', $cmgc_admin_options);
                            }
                        }
                        else {
                            // Skip empty lines. If the position field is filled in, but the rest is not, then skip it.
                            // The payment field ($getData[3]) can be empty though.
                            if(!(empty($getData[1]) && empty($getData[2]) && empty($getData[3]))){
                                if(empty($getData[0]) || !ctype_digit($getData[0])){
                                    $error = true;
                                    $cmgc_admin_options['waiting_list_upload_results'] = 'Line ' . $lineNumber . ' Error: position field is not an integer: ' . $getData[0];
                                    update_option('cmgc_admin_plugin_options', $cmgc_admin_options);
                                }
                                else if(!cmgc_admin_validateDate($getData[2])){
                                    $error = true;
                                    $cmgc_admin_options['waiting_list_upload_results'] = 'Line ' . $lineNumber . ' Error: date added field is not a valid date (m/d/Y): ' . $getData[2];
                                    update_option('cmgc_admin_plugin_options', $cmgc_admin_options);
                                }
                                else if(!empty($getData[3]) && !ctype_digit($getData[3])){
                                    $error = true;
                                    $cmgc_admin_options['waiting_list_upload_results'] = 'Line ' . $lineNumber . ' Error: payment field is not an integer: ' . $getData[3];
                                    update_option('cmgc_admin_plugin_options', $cmgc_admin_options);
                                }

                                $entry = new WaitingListEntry();
                                $entry->Position = $getData[0];
                                $entry->Name = $getData[1];
                                $entry->DateAdded = $getData[2];
                                if(empty($getData[3])){
                                    $entry->PaymentDue = 0;
                                }
                                else {
                                    $entry->PaymentDue = $getData[3];
                                }
                                
                                $waitingList[] = $entry;
                            }
                        }
                        $lineNumber++;
                    }
                }
                else {
                    $error = true;
                    $cmgc_admin_options['waiting_list_upload_results'] = 'Error: file size is 0';
                    update_option('cmgc_admin_plugin_options', $cmgc_admin_options);
                }
            }
            else {
                $error = true;
                $cmgc_admin_options['waiting_list_upload_results'] = 'Error: file is not .csv';
                update_option('cmgc_admin_plugin_options', $cmgc_admin_options);
            }
        }
    }
    else {
        $error = true;
        echo 'Error: $_POST["action"] is not cmgc_admin_upload_waitlist. Here are $_POST variables:<br>';
        print_r($_POST); echo '<br>';
    }

    if(!$error){
        //for($i = 0; $i < count($waitingList); ++$i){
            //echo $waitingList[$i]->Position . ', ' . $waitingList[$i]->Name . ', ' . $waitingList[$i]->DateAdded . '<br>';
        //}

        // Putting require_once at the top of this file didn't work
        require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';

        $connection = new mysqli ( $db_hostname, $db_username, $db_password, $db_database );

        if ($connection->connect_error)
            wp_die ( $connection->connect_error );

        //cmgc_admin_clear_table($connection, 'WaitingList');
        cmgc_admin_waitlist_set_inactive($connection);

        $allEntries = cmgc_admin_waitlist_get_all($connection);

        
        for($i = 0; $i < count ( $waitingList ); ++ $i) {
            
            /*
            * Record Key (int) primary, unique, auto increment
            * Position (int)
            * Name (varchar 50)
            * Date Added (date)
            * Active (tiny int)
            * Payment Due (int)
            * Payment (int)
            * Payment DateTime (datetime)
            * Payer Name (varchar 50)
            */

            // SQL wants YYYY-mm-dd instead of mm/dd/yyyy
            $sqlDate = date("Y-m-d", strtotime($waitingList[$i]->DateAdded));

            $key = $waitingList[$i]->Name . " " . $sqlDate;
            if(empty($allEntries[$key])){

                //echo "Adding to waitlist: " . $key . "<br>";

                $sqlCmd = "INSERT INTO `WaitingList` VALUES (NULL, ?, ?, ?, ?, ?, ?, NULL, ?)";
                $insert = $connection->prepare ( $sqlCmd );
                
                if (! $insert) {
                    wp_die ( $sqlCmd . " prepare failed: " . $connection->error );
                }
                
                

                $active = 1;
                $payment = 0;
                $payerName = "";
                
                if (! $insert->bind_param ( 'issiiis', $waitingList[$i]->Position, $waitingList[$i]->Name, $sqlDate, $active, $waitingList[$i]->PaymentDue, $payment, $payerName )) {
                    wp_die ( $sqlCmd . " bind_param failed: " . $connection->error );
                }
                
                if (! $insert->execute ()) {
                    wp_die ( $sqlCmd . " execute failed: " . $connection->error );
                }

                $insert->close();
            }
            else {
                //echo "Updating waitlist: " . $key . "<br>";

                $sqlCmd = "UPDATE `WaitingList` SET `Position`= ?, `PaymentDue` = ?, `Active` = 1 WHERE `RecordKey` = ?";
			    $update = $connection->prepare ( $sqlCmd );
			
                if (! $update) {
                    die ( $sqlCmd . " prepare failed: " . $connection->error );
                }
			
                if (! $update->bind_param ( 'iii', $waitingList[$i]->Position, $waitingList[$i]->PaymentDue, $allEntries[$key]->RecordKey)) {
                    die ( $sqlCmd . " bind_param failed: " . $connection->error );
                }
                
                if (! $update->execute ()) {
                    die ( $sqlCmd . " execute failed: " . $connection->error );
                }
                $update->close ();
            }
        }
            
        $connection->close ();

        $cmgc_admin_options['waiting_list_upload_results'] = 'Upload complete: ' . $_FILES['filename']['name'];
        update_option('cmgc_admin_plugin_options', $cmgc_admin_options);
    }
}

function cmgc_admin_waitlist_set_inactive($connection){
    // Mark all the players as inactive
		$sqlCmd = "UPDATE `WaitingList` SET `Active` = 0";
		$setAllInactive = $connection->prepare ( $sqlCmd );
		if (! $setAllInactive) {
			die ( $sqlCmd . " prepare failed: " . $connection->error );
		}
		if (! $setAllInactive->execute ()) {
			die ( $sqlCmd . " execute failed: " . $connection->error );
		}
		$setAllInactive->close ();
}

function cmgc_admin_waitlist_get_all($connection){
    $sqlCmd = "SELECT * FROM `WaitingList`";
    $query = $connection->prepare ( $sqlCmd );

    if (! $query) {
        die ( $sqlCmd . " prepare failed: " . $connection->error );
    }

    if (! $query->execute ()) {
        die ( $sqlCmd . " execute failed: " . $connection->error );
    }

    $query->bind_result ( $recordKey, $position, $name, $dateAdded, $active, $paymentDue, $payment, $paymentDateTime, $PayerName );

    $waitingListEntries = array();

    while ( $query->fetch () ) {
        $waitingListEntry = new WaitingListEntry();
        $waitingListEntry->RecordKey = $recordKey;
        $waitingListEntry->Position = $position;
        $waitingListEntry->Name = $name; 
        $waitingListEntry->DateAdded = $dateAdded;
        $waitingListEntry->PaymentDue = $paymentDue;

        // Add to array by combination of name and date added, since we don't have GHIN
        $key = $waitingListEntry->Name . " " . $dateAdded;
        if(!empty($waitingListEntries [$key])){
            wp_die("Duplicate entry for " . $key);
        }
        $waitingListEntries [$key] = $waitingListEntry;
    }

    $query->close ();

    //print_r(array_keys($waitingListEntries));

    return $waitingListEntries;
}

function cmgc_admin_validateDate($date)
{
    $tempDate = explode('/', $date);
    
    if(!ctype_digit($tempDate[0]) || !ctype_digit($tempDate[1]) || !ctype_digit($tempDate[2])){
        return false;
    }

    // checkdate(month, day, year)
    return checkdate($tempDate[0], $tempDate[1], $tempDate[2]);
}
?>