<?php
require_once plugin_dir_path(__FILE__) . 'functions.php';

class cmgc_admin_TeeTimeComposite
{
    public $TeeTimes;
    public $WaitlistPlayers;
    public $CancelledPlayers;
}

// Show the tee times page
function cmgc_admin_tee_times_page2()
{
    // Putting require_once at the top of this file didn't work
    require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';

    $connection = new mysqli ('p:' . $db_hostname, $db_username, $db_password, $db_database );
    
    if ($connection->connect_error){
        echo 'Database connection error: ' .  $connection->connect_error . "<br>";
        return;
    }

    $currentTournaments = cmgc_admin_get_current_tournaments($connection);
    $activeRoster = cmgc_admin_get_all_active_roster_entries_alphabetically($connection);

    // After the upload completes, the browser is redirected back to this admin page.
   // Show the result of the upload and then clear the result.
   $cmgc_admin_options = get_option('cmgc_admin_plugin_options', array());
   if(!empty($cmgc_admin_options) && !empty($cmgc_admin_options['save_tee_times_as_csv_results'])){
       if(str_contains($cmgc_admin_options['save_tee_times_as_csv_results'], 'Error:')){
           echo '<div class="notice notice-error is-dismissible"><p>'. $cmgc_admin_options['save_tee_times_as_csv_results'] . "</p></div>";
       }
       else {
           echo '<div class="notice notice-success is-dismissible"><p>'. $cmgc_admin_options['save_tee_times_as_csv_results'] . "</p></div>";
       }
       
       // Clear the result
       $cmgc_admin_options['save_tee_times_as_csv_results'] = '';
       update_option('cmgc_admin_plugin_options', $cmgc_admin_options);
   }



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
            } else {
                form.FullName.value = player.FullName;
                form.GHIN.value = player.GHIN;
            }
        }
    </script>

    <div class="wrap">
 
        <h2>Tee Times</h2>
 
        <!-- This form will post to admin.php with the action admin_action_cmgc_admin_tee_times_form,
             Must have enctype="multipart/form-data" so _FILES variable filled in 
             Must have autocomplete off so javascript can fill in form elements -->
        <form autocomplete="off" method="POST" enctype="multipart/form-data" action="<?php echo admin_url( 'admin.php' ); ?>">
            <input type="hidden" name="action" value="cmgc_admin_tee_times_form">
            <table class="fixed">
                <tr>
                    <td style="padding: 0px 10px 0px 10px;">Tournament: 
                    <select name="Tournament" id="Tournament">
                        <?php
                            for($i = 0; $i < count($currentTournaments); ++$i){
                                if($i == (count($currentTournaments) - 1)){
                                    echo '<option selected value="';
                                }
                                else {
                                    echo '<option value="';
                                }
                                echo $currentTournaments[$i]->TournamentKey . '">' . $currentTournaments[$i]->Name . '</option>' . PHP_EOL;
                            }
                        ?>
                        </select>
                    </td>

                </tr>
                <tr>
                    <td>
                        <!-- Selected tournament is passed to cmgc_admin_save_tee_times_as_csv as $_POST['Tournament'] -->
                        <input type="submit" name="SaveTeeTimesAsCSV_button" value="Save Tee Times As CSV" class="button-primary">
                        (There is no feedback when the save succeeds. Just look in your download folder for a file.)
                    </td>
                </tr>
                <tr>
                    <td>
                        Add player: <input type="text" name="PartialName" onKeyUp="showResults(this.form)" />
                        <input type="text" name="FullName" readonly />
                        <input type="text" name="GHIN" readonly />
                        Flight (if needed): <input type="text" name="Flight" />
                        <input type="submit" name="AddPlayerToWaitingList_button" id="AddPlayer" value="Add Player to Waiting List" class="button-primary">
                    </td>
                </tr>
            </table>
        </form>
        <p>
            
        </p>
        <!--
        <form   method="POST" enctype="multipart/form-data" action="<?php echo admin_url( 'admin.php' ); ?>">
            <input type="hidden" name="action" value="cmgc_admin_add_player_to_waitlist">
            
        </form>
                        -->
    </div>
    <?php
}

function cmgc_admin_tee_times_error($error)
{
    $cmgc_admin_options = get_option('cmgc_admin_plugin_options', array());
    $cmgc_admin_options['save_tee_times_as_csv_results'] = $error;
    update_option('cmgc_admin_plugin_options', $cmgc_admin_options);

    // These 2 calls to clear the output buffer (ob) are needed to make the redirect work
    //ob_clean();
    ob_start();

    // After doing the work, redirect back to the admin page.
    // The result is displayed in the notice in cmgc_admin_tee_times_page2()
    if(wp_redirect( $_SERVER['HTTP_REFERER'] )){
        exit();
    }
    else {
        echo "redirect failed<br>";
    }
}

 // Do the saving of the csv
 function cmgc_admin_save_tee_times_as_csv_action2()
 {
    require_once plugin_dir_path(__FILE__) . 'tee_times_save.php';

    $cmgc_admin_options = get_option('cmgc_admin_plugin_options', array());
    $cmgc_admin_options['save_tee_times_as_csv_results'] = '';
    update_option('cmgc_admin_plugin_options', $cmgc_admin_options);

    if(!isset($_POST['Tournament']) || empty($_POST['Tournament'])){
        cmgc_admin_tee_times_error('Error: $_POST[Tournament] is empty');
    }

    $tournamentKey = $_POST['Tournament'];

    // Putting require_once at the top of this file didn't work
    require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';

    $connection = new mysqli ('p:' . $db_hostname, $db_username, $db_password, $db_database );
    
    if ($connection->connect_error){
        echo 'Database connection error: ' .  $connection->connect_error . "<br>";
        return;
    }

    $t = cmgc_admin_get_tournament($connection, $tournamentKey);

    $activeRoster = cmgc_admin_get_all_active_roster_entries($connection);

    $teeTimeComposite = new cmgc_admin_TeeTimeComposite();

    cmgc_admin_fill_in_tee_times($connection, $tournamentKey, $teeTimeComposite, $activeRoster);
    cmgc_admin_fill_in_waitList_players($connection, $tournamentKey, $teeTimeComposite, $activeRoster);
    cmgc_admin_fill_in_cancelled_layers($connection, $tournamentKey, $teeTimeComposite, $activeRoster);

    header('Content-Type: application/csv');
    header('Content-Disposition: attachment; filename=Tee Times - ' . $t->Name . '.csv');
    header('Pragma: no-cache');

    echo 'Start Time,Tee Status,Team Id,Last Name,First Name,GHIN,Flight,Email,Extra,Tee' . PHP_EOL;
    $teamId = 0;
    cmgc_admin_write_tee_times_to_csv($teeTimeComposite->TeeTimes, $teamId, 'TeeTime', $t->TeamSize);
    cmgc_admin_write_tee_times_to_csv($teeTimeComposite->WaitlistPlayers, $teamId, 'Waitlisted', $t->TeamSize);
    cmgc_admin_write_tee_times_to_csv($teeTimeComposite->CancelledPlayers, $teamId, 'Cancelled', $t->TeamSize);
 }

 function cmgc_admin_get_current_tournaments($connection) {

	$tournaments = cmgc_admin_get_tournaments($connection);
		
	$now = new DateTime ( "now" );
	
    // Find the 1st tournament after "now"
    $last = 0;
	for($i = 0; ($i < count($tournaments)) && ($last == 0); ++$i){
		// Get the start date
		$signupStart = new DateTime ( $tournaments[$i]->SignupStartDate );

        // Find the 1st tournament that has not started signups
		if($signupStart >= $now){
			$last = $i - 1;
		}
	}
    if($last == 0){
        $last = count($tournaments);
    }
    
    $start = $last - 5;
    if($start < 0) { $start = 0;}
    if($last < 0) { $last = 0;}

    // Grab the last tournaments that are not announcements
    $currentTournaments = array();
    for($i = $start; $i <= $last; ++$i){
        if(!$tournaments[$i]->AnnouncementOnly){
            $currentTournaments[] = $tournaments[$i];
        }
    }
	
	return $currentTournaments;
}

function cmgc_admin_add_player_to_waitlist()
 {
    if(!isset($_POST['Tournament']) || empty($_POST['Tournament'])){
        cmgc_admin_tee_times_error('Error: $_POST[Tournament] is empty');
    }

    $tournamentKey = $_POST['Tournament'];
    
    if(!isset($_POST['GHIN']) || empty($_POST['GHIN'])){
        cmgc_admin_tee_times_error('Error: Add Player to Waiting List: no player selected');
    }

    print_r($_POST);
 }

 

?>