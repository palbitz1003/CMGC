<?php
require_once plugin_dir_path(__FILE__) . 'functions.php';

function cmgc_admin_roster_page2()
{
    // Putting require_once at the top of this file didn't work
    require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';

    $connection = new mysqli ('p:' . $db_hostname, $db_username, $db_password, $db_database );
    
    if ($connection->connect_error){
        echo 'Database connection error: ' .  $connection->connect_error . "<br>";
        return;
    }

    $activeRoster = cmgc_admin_get_all_active_roster_entries_alphabetically($connection);

    // After the upload completes, the browser is redirected back to this admin page.
   // Show the result of the upload and then clear the result.
   $cmgc_admin_options = get_option('cmgc_admin_plugin_options', array());
   if(!empty($cmgc_admin_options) && !empty($cmgc_admin_options['roster_upload_results'])){
       if(str_contains($cmgc_admin_options['roster_upload_results'], 'Error:')){
           echo '<div class="notice notice-error is-dismissible"><p>'. $cmgc_admin_options['roster_upload_results'] . "</p></div>";
       }
       else {
           echo '<div class="notice notice-success is-dismissible"><p>'. $cmgc_admin_options['roster_upload_results'] . "</p></div>";
       }
       
       // Clear the result
       $cmgc_admin_options['roster_upload_results'] = '';
       update_option('cmgc_admin_plugin_options', $cmgc_admin_options);
   }

   ?>
   <div class="wrap">

       <h2>Upload Roster</h2>

       <!-- This form will post to admin.php with the action admin_action_cmgc_admin_upload_roster,
            which triggers calling cmgc_admin_upload_roster() below.
            Must have enctype="multipart/form-data" so _FILES variable filled in -->
       <form method="POST" enctype="multipart/form-data" action="<?php echo admin_url( 'admin.php' ); ?>">
           <input type="hidden" name="action" value="cmgc_admin_upload_roster">
           <table class="form-table">
               <tr>
                   <th scope="row"><label for="filename">Roster (.csv):</label></th>
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

    <script>
        var roster = <?php echo json_encode($activeRoster); ?>;

        function autocompleteMatch(input) {
            if (input == '') {
                return null;
            }
            let lowerCaseName = input.toLowerCase();
            for(let i = 0; i < roster.length; i++){
                if(roster[i].FullName.toLowerCase().startsWith(lowerCaseName)){
                    return roster[i];
                }
            }
            return null;
        }

        function showResults(form) {
            //var inputValue = form.PartialName.value;
            //res = document.getElementById("result");
            //res.innerHTML = '';
            let player = autocompleteMatch(form.PartialName.value);
            //for (i=0; i<terms.length; i++) {
            //    list += '<li>' + terms[i] + '</li>';
            //}
            if(player == null){
                form.FullName.value = '';
                form.GHIN.value = '';
                form.Email.value = '';
                form.BirthDate.value = '';
                form.DateAdded.value = '';
                form.MembershipType.value = '';
                form.SignupPriority.value = '';
                form.Tee.value = '';
            } else {
                form.FullName.value = player.FullName;
                form.GHIN.value = player.GHIN;
                form.Email.value = player.Email;
                form.BirthDate.value = player.BirthDate;
                form.DateAdded.value = player.DateAdded;
                switch(player.MembershipType){
                    case 'R':
                        form.MembershipType.value = 'Regular';
                        break;
                    case 'L':
                        form.MembershipType.value = 'Life';
                        break;
                    case 'H':
                        form.MembershipType.value = 'Honorary';
                        break;
                    default:
                        form.MembershipType.value = player.MembershipType;
                        break;
                }
                if(player.SignupPriority === "B"){
                    form.SignupPriority.value = "Board Member";
                } else {
                    form.SignupPriority.value = "not a Board Member";
                }
                switch(player.Tee){
                    case 'G':
                        form.Tee.value = 'Green';
                        break;
                    case 'S':
                        form.Tee.value = 'Silver';
                        break;
                    default:
                        form.Tee.value = 'White';
                        break;
                }
            }
        }
    </script>

    <div class="wrap">
 
    <h2>Search Roster</h2>

    <form autocomplete="off" method="POST" enctype="multipart/form-data" action="<?php echo admin_url( 'admin.php' ); ?>">
        <table class="fixed">
            <tr>
                <td>Enter Name:</td>
                <td><input type="text" name="PartialName" size="50" onKeyUp="showResults(this.form)" /></td>
            </tr>
            <tr>
                <td>Full Name:</td>
                <td><input type="text" name="FullName" size="50" readonly /></td>
            </tr>
            <tr>
                <td>GHIN:</td>
                <td><input type="text" name="GHIN" size="50" readonly /></td>
            </tr>
            <tr>
                <td>Email:</td>
                <td><input type="text" name="Email" size="50" readonly /></td>
            </tr>
            <tr>
                <td>Birth Date:</td>
                <td><input type="text" name="BirthDate" size="50" readonly /></td>
            </tr>
            <tr>
                <td>Date Added:</td>
                <td><input type="text" name="DateAdded" size="50" readonly /></td>
            </tr>
            <tr>
                <td>Membership Type:</td>
                <td><input type="text" name="MembershipType" size="50" readonly /></td>
            </tr>
            <tr>
                <td>Signup Priority:</td>
                <td><input type="text" name="SignupPriority" size="50" readonly /></td>
            </tr>
            <tr>
                <td>Tee:</td>
                <td><input type="text" name="Tee" size="50" readonly /></td>
            </tr>
        </table>
    </form>

    </div>
<?php
}

 // Do the actual upload of the roster
 function cmgc_admin_upload_roster_action2()
 {
    $cmgc_admin_options = get_option('cmgc_admin_plugin_options', array());
    $cmgc_admin_options['roster_upload_results'] = 'Not Implemented Yet';
    update_option('cmgc_admin_plugin_options', $cmgc_admin_options);
 }

?>