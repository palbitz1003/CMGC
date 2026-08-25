<?php
require_once plugin_dir_path(__FILE__) . 'functions.php';

// Show the yearly dues page
function cmgc_admin_yearly_dues_page2()
{
   // After the upload completes, the browser is redirected back to this admin page.
   // Show the result of the upload and then clear the result.
   $cmgc_admin_options = get_option('cmgc_admin_plugin_options', array());
   if(!empty($cmgc_admin_options) && !empty($cmgc_admin_options['yearly_dues_upload_results'])){
       if(str_contains($cmgc_admin_options['yearly_dues_upload_results'], 'Error:')){
           echo '<div class="notice notice-error is-dismissible"><p>'. $cmgc_admin_options['yearly_dues_upload_results'] . "</p></div>";
       }
       else {
           echo '<div class="notice notice-success is-dismissible"><p>'. $cmgc_admin_options['yearly_dues_upload_results'] . "</p></div>";
       }
       
       // Clear the result
       $cmgc_admin_options['yearly_dues_upload_results'] = '';
       update_option('cmgc_admin_plugin_options', $cmgc_admin_options);
   }

   ?>
   <div class="wrap">

       <h2>Yearly Dues</h2>

       <!-- This form will post to admin.php with the action admin_action_cmgc_admin_upload_yearly_dues,
            which triggers calling cmgc_admin_upload_yearly_dues_action() below.
            Must have enctype="multipart/form-data" so _FILES variable filled in -->
       <form method="POST" enctype="multipart/form-data" action="<?php echo admin_url( 'admin.php' ); ?>">
           <input type="hidden" name="action" value="cmgc_admin_upload_yearly_dues">
           <table class="form-table">
               <tr>
                   <th scope="row"><label for="filename">Yearly Dues (.csv):</label></th>
                   <td><input type="file" id="filename" name="filename" accept=".csv" required></td>
               </tr>
               <tr>
                   <td>
                       <input type="submit" name="Import" value="Upload" class="button-primary">
                   </td>
                   <td></td>
               </tr>
           </table>
       </form>
   </div>
   <?php
}

function cmgc_admin_upload_yearly_dues_action2()
{
    class DuesEntry {
        public $Name;
        public $GHIN;
        public $Rollover;
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

    require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';

    $cmgc_admin_options = get_option('cmgc_admin_plugin_options', array());
    $cmgc_admin_options['yearly_dues_upload_results'] = '';
    update_option('cmgc_admin_plugin_options', $cmgc_admin_options);

    $error = false;
    $waitingList = array();
    if($_POST["action"] === "cmgc_admin_upload_yearly_dues"){

        if(empty($_FILES["filename"]["name"])){
            $error = true;
            $cmgc_admin_options['yearly_dues_upload_results'] = 'Error: No file chosen';
            update_option('cmgc_admin_plugin_options', $cmgc_admin_options);
        }
        else {
            //echo "Name is: " . $_FILES["filename"]["name"] . "<br>";
            $filename=$_FILES["filename"]["tmp_name"];    
            if($_FILES["filename"]["type"] === 'text/csv'){
                if($_FILES["filename"]["size"] > 0){
                    $file = fopen($filename, "r");
                    $lineNumber = 1;
                    $nameIndex = -1;
                    $ghinNumberIndex = -1;
                    $ghinStatusIndex = -1;
                    $subscriptionIndex = -1;
                    $subscriptionEndDateIndex = -1;
                    $rolloverIndex = -1;

                    $paidCount = 0;
                    $rolloverCount = 0;
                    $currentYear = date("Y");
                    $nextYear = $currentYear + 1;

                    $duesEntries = array();
                    $entryDateTime = date("Y-m-d H:i:s");
                    
                    while (($error == false) && (($getData = fgetcsv($file, 10000, ",")) !== FALSE)){

                        if($lineNumber === 1){
                            for($i = 0; $i < count($getData); ++$i){
                                $header = trim($getData[$i]);
                                $header = strtolower($header);
                                if(empty($header)){
                                    // skip empty headers
                                } else if(strcasecmp($header, 'name') == 0){
                                    $nameIndex = $i;
                                    //echo 'Name index: ' . $nameIndex . '<br>';
                                } else if((strpos($header, 'ghin') !== false) && strpos($header, '#') !== false){
                                    $ghinNumberIndex = $i;
                                    //echo 'GHIN Number index: ' . $ghinNumberIndex . '<br>';
                                } else if(strcasecmp($header, 'ghin status') == 0){
                                    $ghinStatusIndex = $i;
                                    //echo 'GHIN Status index: ' . $ghinStatusIndex . '<br>';
                                } else if(strcasecmp($header, 'subscription') == 0){
                                    $subscriptionIndex = $i;
                                    //echo 'Subscription index: ' . $subscriptionIndex . '<br>';
                                } else if((strpos($header, 'subscription') !== false) && strpos($header, 'end date') !== false){
                                    $subscriptionEndDateIndex = $i;
                                    //echo 'Subscription End Date index: ' . $subscriptionEndDateIndex . '<br>';
                                } else if(strcasecmp($header, 'rollover') == 0){
                                    $rolloverIndex = $i;
                                    //echo 'Rollover index: ' . $rolloverIndex . '<br>';
                                }
                            }

                            if($nameIndex === -1){
                                $error = true;
                                $cmgc_admin_options['yearly_dues_upload_results'] = 'Error: Failed to find column with header "Name"';
                                update_option('cmgc_admin_plugin_options', $cmgc_admin_options);
                            }
                            if(!$error && ($ghinNumberIndex === -1)){
                                $error = true;
                                $cmgc_admin_options['yearly_dues_upload_results'] = 'Error: Failed to find column with header "GHIN #"';
                                update_option('cmgc_admin_plugin_options', $cmgc_admin_options);
                            }
                            if(!$error && ($ghinStatusIndex === -1)){
                                $error = true;
                                $cmgc_admin_options['yearly_dues_upload_results'] = 'Error: Failed to find column with header "GHIN Status"';
                                update_option('cmgc_admin_plugin_options', $cmgc_admin_options);
                            }
                            if(!$error && ($subscriptionIndex === -1)){
                                $error = true;
                                $cmgc_admin_options['yearly_dues_upload_results'] = 'Error: Failed to find column with header "Subscription"';
                                update_option('cmgc_admin_plugin_options', $cmgc_admin_options);
                            }
                            if(!$error && ($subscriptionEndDateIndex === -1)){
                                $error = true;
                                $cmgc_admin_options['yearly_dues_upload_results'] = 'Error: Failed to find column with header "SubscriptionEnd Date"';
                                update_option('cmgc_admin_plugin_options', $cmgc_admin_options);
                            }
                            if(!$error && ($rolloverIndex === -1)){
                                $error = true;
                                $cmgc_admin_options['yearly_dues_upload_results'] = 'Error: Failed to find column with header "Rollover"';
                                update_option('cmgc_admin_plugin_options', $cmgc_admin_options);
                            }

                        }
                        else {
                            
                            $paidEntry = false;
                            $rolloverEntry = false;
                            $subscriptionEndDate = trim($getData[$subscriptionEndDateIndex]);
                            $dateFields = explode("/", $subscriptionEndDate);

                            if(count($dateFields) == 3){
                                if($dateFields[2] > $currentYear){
                                    $paidCount++;
                                    $paidEntry = true;
                                }
                            }
                            if(strcasecmp("yes", trim($getData[$rolloverIndex])) == 0){
                                $rolloverCount++;
                                $rolloverEntry = true;
                            }

                            if($paidEntry || $rolloverEntry){
                                $newEntry = new DuesEntry();
                                // Limit name to 50 characters, since that is what the database supports
                                $newEntry->Name = substr($getData[$nameIndex], 0, 50);
                                $newEntry->GHIN = $getData[$ghinNumberIndex];
                                $newEntry->Rollover = $rolloverEntry;

                                $duesEntries[] = $newEntry;
                            }

                        }
                        $lineNumber++;
                    }

                    $connection = new mysqli ('p:' . $db_hostname, $db_username, $db_password, $db_database );
    
                    if ($connection->connect_error){
                        $error = true;
                        $cmgc_admin_options['yearly_dues_upload_results'] = 'Error: SQL connection error: ' . $connection->error;
                        update_option('cmgc_admin_plugin_options', $cmgc_admin_options);
                        return;
                    }

                    $sqlCmd = "DELETE FROM `Dues` WHERE `Year` = " . $nextYear;
                    $query = $connection->prepare ( $sqlCmd );
                    
                    if (! $query) {
                        $error = true;
                        $cmgc_admin_options['yearly_dues_upload_results'] = 'Error: SQL prepare failed (delete from Dues table): ' . $connection->error;
                        update_option('cmgc_admin_plugin_options', $cmgc_admin_options);
                        return;
                    }
                    
                    if (! $query->execute ()) {
                        $error = true;
                        $cmgc_admin_options['yearly_dues_upload_results'] = 'Error: SQL execute failed (delete from Dues table): ' . $connection->error;
                        update_option('cmgc_admin_plugin_options', $cmgc_admin_options);
                        return;
                    }

                    // Add the entries, but limit how many are added with each SQL call
                    for($currentEntry = 0; $currentEntry < count($duesEntries);){
                        $sqlCmd = "INSERT INTO `Dues` VALUES ";
                        for($i = 0; ($i < 50) && ($currentEntry < count($duesEntries)); ++$i){
                            // Must have comma separator between each entry after the first one
                            if($i > 0){
                                $sqlCmd = $sqlCmd . ", ";
                            }
                            // Add "Rollover" to those created because rollover was set to "yes"
                            $scga = "SCGA";
                            if($duesEntries[$currentEntry]->Rollover){
                                $scga = $scga . " Rollover";
                            }
                            $entry = " (" . $nextYear . ", " . $duesEntries[$currentEntry]->GHIN . ", '" . $duesEntries[$currentEntry]->Name . "', 1, '" . $entryDateTime . "', '" . $scga . "', '', 0)";
                            $sqlCmd = $sqlCmd . $entry;
                            $currentEntry++;
                        }
                        //echo $sqlCmd . "<br>";

                        $insert = $connection->prepare ( $sqlCmd );
                    
                        if (! $insert) {
                            $error = true;
                            $cmgc_admin_options['yearly_dues_upload_results'] = 'Error: SQL prepare failed (insert into Dues table): ' . $connection->error;
                            update_option('cmgc_admin_plugin_options', $cmgc_admin_options);
                            return;
                        }

                        if (! $insert->execute ()) {
                            $error = true;
                            $cmgc_admin_options['yearly_dues_upload_results'] = 'Error: SQL execute failed (insert into Dues table): ' . $connection->error;
                            update_option('cmgc_admin_plugin_options', $cmgc_admin_options);
                            return;
                        }
                    }

                    if(!$error){
                        $cmgc_admin_options['yearly_dues_upload_results'] = 'Success: ' . $paidCount . ' paid, ' . $rolloverCount . ' rollover set to "yes"';
                        update_option('cmgc_admin_plugin_options', $cmgc_admin_options);
                    }
                }
                else {
                    $error = true;
                    $cmgc_admin_options['yearly_dues_upload_results'] = 'Error: file size is 0';
                    update_option('cmgc_admin_plugin_options', $cmgc_admin_options);
                }
            }
            else {
                $error = true;
                $cmgc_admin_options['yearly_dues_upload_results'] = 'Error: file is not .csv';
                update_option('cmgc_admin_plugin_options', $cmgc_admin_options);
            }
        }
    }
    else {
        $error = true;
        echo 'Error: $_POST["action"] is not cmgc_admin_upload_yearly_dues. Here are $_POST variables:<br>';
        print_r($_POST); echo '<br>';
    }
}

?>