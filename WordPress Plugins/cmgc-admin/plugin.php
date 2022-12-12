<?php
/**
 * Plugin Name: CMGC Admin
 * Description: Manage CMGC Website
 * Version: 1.0.0
 */


 register_activation_hook(__FILE__, 'cmgc_admin_plugin_create_options');
 add_action( 'admin_menu', 'cmgc_admin_create_menu');

 // Replaced manage_options with edit_pages so editor capability can see them
 function cmgc_admin_create_menu() {
    add_menu_page('CMGC Admin', "CMGC Admin", 
        'edit_pages', 'cmgc-admin-menu', 'cmgc_admin_show_menu',
        'dashicons-admin-home', 99);
    
    add_submenu_page('cmgc-admin-menu',  // slug name for parent menu
        'Membership Waitlist', // title of page
        'Membership Waitlist', // name of sub-menu
        'edit_pages', // minimum capability (editor)
        'cmgc-admin-membership-waitlist', // slub name for submenu
        'cmgc_admin_membership_waitlist_page'); // function to call

    add_submenu_page('cmgc-admin-menu', 'Membership Applications', 'Membership Applications',
        'edit_pages', 'cmgc-admin-membership-applications', 'cmgc_admin_membership_application_page');

 }

 function cmgc_admin_plugin_create_options()
 {
    // Only load if explicitly needed
    // Use table to pass back result of upload to waitlist page
    add_option('cmgc_admin_plugin_options', array(
        'waiting_list_upload_results' => ''
        ), 
    '', "no");
 }

 function cmgc_admin_show_menu() {
    
 }

 function cmgc_admin_membership_waitlist_page()
 {
    // After the upload completes, the browser is redirected back to this admin page.
    // Show the result of the upload and then clear the result.
    $cmgc_admin_options = get_option('cmgc_admin_plugin_options', array());
    if(!empty($cmgc_admin_options) && !empty($cmgc_admin_options['waiting_list_upload_results'])){
        if(str_contains($cmgc_admin_options['waiting_list_upload_results'], 'Error:')){
            echo '<div class="notice notice-error is-dismissible"><p>'. $cmgc_admin_options['waiting_list_upload_results'] . "</p></div>";
        }
        else {
            echo '<div class="notice notice-success is-dismissible"><p>'. $cmgc_admin_options['waiting_list_upload_results'] . "</p></div>";
        }
        
        // Clear the result
        $cmgc_admin_options['waiting_list_upload_results'] = '';
        update_option('cmgc_admin_plugin_options', $cmgc_admin_options);
    }

    ?>
    <div class="wrap">
        <?php screen_icon ( 'plugins' ); ?>

        <h2>Upload New Waiting List</h2>

        <!-- This form will post to admin.php with the action admin_action_cmgc_admin_upload_waitlist,
             which triggers calling cmgc_admin_upload_waitlist_action() below.
             Must have enctype="multipart/form-data" so _FILES variable filled in -->
        <form method="POST" enctype="multipart/form-data" action="<?php echo admin_url( 'admin.php' ); ?>">
            <input type="hidden" name="action" value="cmgc_admin_upload_waitlist">
            <table class="form-table">
                <tr>
                    <th scope="row"><label for="filename">Membership Waiting List (.csv):</label></th>
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
        <?php cmgc_admin_show_waitlist_with_payment_due(); ?>
    </div>
    <?php
 }

 // When the user clicks on the "submit" waiting list button, this function is called
 add_action( 'admin_action_cmgc_admin_upload_waitlist', 'cmgc_admin_upload_waitlist_action' );
function cmgc_admin_upload_waitlist_action()
{
    //echo plugin_dir_path(__FILE__);
    require_once plugin_dir_path(__FILE__) . 'src/upload_membership_waitlist.php';
    cmgc_admin_upload_waitlist_action2();
     
    // These 2 calls are needed to make the redirect work
    //ob_clean();
    ob_start();

    // After doing the work, redirect back to the admin page.
    // cmgc_admin_upload_waitlist_action2() filled in the result, which is displayed
    // in the notice in cmgc_admin_membership_waitlist_page()
    if(wp_redirect( $_SERVER['HTTP_REFERER'] )){
        exit();
    }
    else {
        echo "redirect failed<br>";
    }
}

// At the bottom of the waiting list page, the people approved to become members (payment due is non-zero) is shown
function cmgc_admin_show_waitlist_with_payment_due(){
    // Putting require_once at the top of this file didn't work
    require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';

    //var_dump( get_defined_vars() );

    class WaitingListEntry {
        public $Position;
        public $Name;
        public $DateAdded;
        public $PaymentDue;
        public $Payment;
        public $PaymentDateTime;
        public $PayerName;
    }
    
    $connection = new mysqli ('p:' . $db_hostname, $db_username, $db_password, $db_database );
    
    if ($connection->connect_error){
        echo 'Database connection error: ' .  $connection->connect_error . "<br>";
        return;
    }
    
    //echo ' <div id="content-container" class="entry-content">';
    //echo '    <div id="content" role="main">';
    
    $sqlCmd = "SELECT * FROM `WaitingList` WHERE `Active` = 1 and `PaymentDue` > 0 ORDER BY `Position` ASC";
    $query = $connection->prepare ( $sqlCmd );
    
    if (! $query) {
        die ( $sqlCmd . " prepare failed: " . $connection->error );
    }
    
    if (! $query->execute ()) {
        die ( $sqlCmd . " execute failed: " . $connection->error );
    }
    
    $query->bind_result ( $recordKey, $position, $name, $dateAdded, $active, $paymentDue, $payment, $paymentDateTime, $payerName );
    
    $waitingListEntriesByPosition = array();
    while ( $query->fetch () ) {
        $waitingListEntry = new WaitingListEntry();
        $waitingListEntry->Position = $position;
        $waitingListEntry->Name = $name;
        $waitingListEntry->DateAdded = $dateAdded;
        $waitingListEntry->PaymentDue = $paymentDue;
        $waitingListEntry->Payment = $payment;
        $waitingListEntry->PaymentDateTime = $paymentDateTime;
        $waitingListEntry->PayerName = $payerName;
        $waitingListEntriesByPosition [] = $waitingListEntry;
    }
    
    $query->close ();
    if (! $waitingListEntriesByPosition || (count ( $waitingListEntriesByPosition ) == 0)) {
        return;
    }

    echo '<h2>Membership Waiting Final Payment</h2>' ;

    // Table class can be widefat, fixed, or striped
    echo '<table class="fixed" >' . PHP_EOL;
    echo '<thead><tr><th>Pos</th><th>Name</th><th>Date Added</th><th>Payment Due</th><th>Payment</th><th>Payment Date</th><th>Payer Name</th></tr></thead>' . PHP_EOL;
    echo '<tbody>' . PHP_EOL;

    for($i = 0; $i < count ( $waitingListEntriesByPosition ); ++ $i) {

        echo '<tr>';
        echo '<td style="padding: 0px 10px 0px 10px;">' . $waitingListEntriesByPosition[$i]->Position . '</td>';
        echo '<td style="padding: 0px 10px 0px 10px;">' . $waitingListEntriesByPosition[$i]->Name . '</td>';
        echo '<td style="padding: 0px 10px 0px 10px;">' . date ( 'n/j/Y', strtotime ( $waitingListEntriesByPosition[$i]->DateAdded ) ) . '</td>';
        echo '<td style="text-align: center;">' . $waitingListEntriesByPosition[$i]->PaymentDue . '</td>';
        echo '<td style="text-align: center;">' . $waitingListEntriesByPosition[$i]->Payment . '</td>';
        echo '<td style="padding: 0px 10px 0px 10px;">' . $waitingListEntriesByPosition[$i]->PaymentDateTime . '</td>';
        echo '<td style="padding: 0px 10px 0px 10px;">' . $waitingListEntriesByPosition[$i]->PayerName . '</td>';
        echo '</tr>' . PHP_EOL;
    }


    // Finish the first column table.  Show the 2nd column table.
    echo '</tbody>' . PHP_EOL;
    echo '</table>' . PHP_EOL;
 }

 function cmgc_admin_membership_application_page()
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
    }
    
    $query->bind_result ( $recordKey, $active, $lastName, $firstName, $mailingAddress, $email, $ghin, $phoneNumber, $birthDate,
                        $sponsor1LastName, $sponsor1Ghin, $sponsor1PhoneNumber,
                        $sponsor2LastName, $sponsor2Ghin, $sponsor2PhoneNumber,
                        $dateTimeAdded, $payment, $paymentDateTime, $payerName );
    
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

        $membershipApplicationEntries[] = $membershipApplication;
    }
    
    $query->close ();

    echo '<h2>Pending Membership Applications</h2>' ;
    if (! $membershipApplicationEntries || (count ( $membershipApplicationEntries ) == 0)) {
        echo "none pending";
        return;
    }

    // Table class can be widefat, fixed, or striped
    echo '<table class="fixed" >' . PHP_EOL;
    echo '<thead><tr><th>Date Added</th><th>Last</th><th>First</th><th>GHIN</th><th>Mailing Address</th><th>Email Address</th><th>Phone</th><th>DOB</th>';
    echo '<th>Sp1 Last</th><th>Sp1 GHIN</th><th>Sp1 Phone</th><th>Sp2 Last</th><th>Sp2 GHIN</th><th>Sp2 Phone</th><th>Payment</th><th>Payment Date</th><th>Payer Name</th></tr></thead>' . PHP_EOL;
    echo '<tbody>' . PHP_EOL;

    for($i = 0; $i < count ( $membershipApplicationEntries ); ++ $i) {

        echo '<tr>' . PHP_EOL;
        echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationEntries[$i]->DateTimeAdded . '</td>' . PHP_EOL;
        echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationEntries[$i]->LastName . '</td>' . PHP_EOL;
        echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationEntries[$i]->FirstName . '</td>' . PHP_EOL;
        echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationEntries[$i]->GHIN . '</td>' . PHP_EOL;
        echo '<td style="padding: 0px 10px 0px 10px;">' . $membershipApplicationEntries[$i]->MailingAddress . '</td>' . PHP_EOL;
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
 }
 

 /*
 function add_custom_menu_item(){
    add_menu_page( 'Menu Item Title', 'Page Title', 'manage_options', 'page_slug', 'function', 'dashicons-icon', 1 );
}
add_action( 'admin_menu', 'add_custom_menu_item' );

function custom_menu_item_redirect() {

    $menu_redirect = isset($_GET['page']) ? $_GET['page'] : false;

    if($menu_redirect == 'page_slug' ) {
        wp_safe_redirect( home_url('/my-page') );
        exit();
    }

}
add_action( 'admin_init', 'custom_menu_item_redirect', 1 );
 */

 /*
 add_action( 'admin_init', 'cameronjonesweb_add_settings_link' );
add_filter( 'clean_url', 'cameronjonesweb_admin_menu_external_link', 10, 3 );

function cameronjonesweb_add_settings_link() {
	add_options_page(
		'My Settings Page Title',
		'My Settings Page Menu Title',
		'manage_options',
		'my-settings-page',
		'__return_false'
	);
}

function cameronjonesweb_admin_menu_external_link( $good_protocol_url, $original_url, $_context ) {
	if ( 'options-general.php?page=my-settings-page' === $good_protocol_url ) {
		$good_protocol_url = 'https://cameronjonesweb.com.au';
	}
	return $good_protocol_url;
}
 */

?>