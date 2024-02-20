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

?>
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
 
    <h2>Roster</h2>

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
                <td>Date Adde3d:</td>
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
    <p>
        
    </p>

    </div>
<?php
}

?>