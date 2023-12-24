<?php
require_once plugin_dir_path(__FILE__) . 'functions.php';

class cmgc_admin_DatabaseTeeTime {
	public $Key;
	public $StartTime;
	public $StartHole;
	public $Players;
}
class cmgc_admin_DatabaseTeeTimePlayer {
	public $GHIN;
	public $LastName;
	public $FirstName;
	public $Handicap;
	public $Extra;
	public $SignupKey;
}

class cmgc_admin_TeeTimeCancelledPlayer {
	public $TournamentKey;
	public $Position;
	public $GHIN;
	public $Name;
}

class cmgc_admin_TeeTimeComposite
{
    public $TeeTimes;
    public $WaitlistPlayers;
    public $CancelledPlayers;
}

class cmgc_admin_TeeTime {
    public $StartTime;
    public $Players;
}

class cmgc_admin_Player {
    public $Name;
    public $Position;
    public $GHIN;
    public $Extra;
    public $Email;
    public $SignupKey;
    public $Tee;
}

class cmgc_admin_SignUpWaitingListClass {
	public $TournamentKey;
	public $Position;
	public $GHIN1;
	public $Name1;
	public $GHIN2;
	public $Name2;
	public $GHIN3;
	public $Name3;
	public $GHIN4;
	public $Name4;
}

class cmgc_admin_PlayerSignUpClass {
	public $SignUpKey;
	public $TournamentKey;
	public $Position;
	public $GHIN;
	public $LastName;
	public $Extra;
}

// Show the waitlist page
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
    <div class="wrap">
 
        <h2>Tee Times</h2>
 
        <!-- This form will post to admin.php with the action admin_action_cmgc_save_tee_times_as_csv,
             which triggers calling cmgc_admin_save_tee_times_as_csv_action2() below.
             Must have enctype="multipart/form-data" so _FILES variable filled in -->
        <form method="POST" enctype="multipart/form-data" action="<?php echo admin_url( 'admin.php' ); ?>">
            <input type="hidden" name="action" value="cmgc_save_tee_times_as_csv">
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
                    <td>
                        <input type="submit" name="Save" value="Save Tee Times as CSV" class="button-primary">
                    </td>
                </tr>
            </table>
        </form>
    </div>
    <?php
}

 // Do the actual upload of the waitlist
 function cmgc_save_tee_times_as_csv_action2()
 {
    $cmgc_admin_options = get_option('cmgc_admin_plugin_options', array());
    $cmgc_admin_options['save_tee_times_as_csv_results'] = '';
    update_option('cmgc_admin_plugin_options', $cmgc_admin_options);

    $tournamentKey = 0;
    if(isset($_POST['Tournament'])){
        $tournamentKey = $_POST['Tournament'];
    }
    else {
        // error
        $cmgc_admin_options['save_tee_times_as_csv_results'] = 'Error: $_Post[Tournament] is empty';
        update_option('cmgc_admin_plugin_options', $cmgc_admin_options);

        // These 2 calls to clear the output buffer (ob) are needed to make the redirect work
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

    header('Content-Type: application/csv');
    header('Content-Disposition: attachment; filename=Tee Times - ' . $t->Name . '.csv');
    header('Pragma: no-cache');

    echo 'Start Time,Tee Status,Team Id,Last Name,First Name,GHIN,Flight,Email,Extra,Tee' . PHP_EOL;
    $teamId = 0;
    cmgc_admin_write_tee_times_to_csv($teeTimeComposite->TeeTimes, $teamId, 'TeeTime', $t->TeamSize);
    cmgc_admin_write_tee_times_to_csv($teeTimeComposite->WaitlistPlayers, $teamId, 'Waitlisted', $t->TeamSize);
 }

 function cmgc_admin_write_tee_times_to_csv($teeTimes, &$teamId, $teeStatus, $teamSize){
    for($i = 0; $i < count($teeTimes); ++$i){
        for($j = 0; $j < count($teeTimes[$i]->Players); ++$j){

            if(($teamSize == 4) && ($j == 0)){
                // Increment team on 1st player of foursome
                $teamId++;
            } else if(($teamSize == 2) && (($j == 0) || ($j == 2))){
                // Increment team on 1st player of pair
                $teamId++;
            } else if($teamSize == 1){
                $teamId++;
            }

            echo $teeTimes[$i]->StartTime . ',';
            echo $teeStatus . ',';
            echo $teamId . ',';

            $name = explode(',', $teeTimes[$i]->Players[$j]->Name);
            $lastName = '';
            $firstName = '';
            if(count($name) > 1){
                $lastName = trim($name[0]);
                $firstName = trim($name[1]);
            } else {
                $lastName = $name[0];
            }

            // TODO: handle name with "jr"

            echo $lastName . ',' . $firstName . ',';
            echo $teeTimes[$i]->Players[$j]->GHIN . ',';

            $extra = $teeTimes[$i]->Players[$j]->Extra;
            if(!empty($extra)){
                if(strpos($extra, 'CH') !== false){
                    echo '0';
                } else if((strpos($extra, 'F1') !== false) || (stripos($extra, 'flight1') !== false)){
                    echo '1';
                } else if((strpos($extra, 'F2') !== false) || (stripos($extra, 'flight2') !== false)){
                    echo '2';
                } else if((strpos($extra, 'F3') !== false) || (stripos($extra, 'flight3') !== false)){
                    echo '3';
                } else if((strpos($extra, 'F4') !== false) || (stripos($extra, 'flight4') !== false)){
                    echo '4';
                } else if((strpos($extra, 'F5') !== false) || (stripos($extra, 'flight5') !== false)){
                    echo '5';
                }
            }
            echo ',';
            echo $teeTimes[$i]->Players[$j]->Email . ',';

            if(!empty($extra)){
                if(($extra === 'M') || ($extra === 'G')){
                    echo $extra;
                }
            }
            echo ',';
            echo $teeTimes[$i]->Players[$j]->Tee . PHP_EOL;
        }
    }
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

function cmgc_admin_fill_in_tee_times($connection, $tournamentKey, $teeTimeComposite, $activeRoster){

    $teeTimes = cmgc_admin_get_tee_times($connection, $tournamentKey);

    $teeTimeComposite->TeeTimes = array();
    for($i = 0; $i < count($teeTimes); ++$i){
        
        $teeTime = new cmgc_admin_TeeTime();
        $teeTime->StartTime = date ( 'g:i A', strtotime ($teeTimes[$i]->StartTime));
        $teeTime->Players = array();
        
        for($j = 0; $j < count($teeTimes[$i]->Players); ++$j){
            $player = new cmgc_admin_Player();
            $player->Name =  $teeTimes[$i]->Players[$j]->LastName;
            $player->GHIN = $teeTimes[$i]->Players[$j]->GHIN;
            $player->Position = $j + 1;
            $player->Extra = $teeTimes[$i]->Players[$j]->Extra;
            $player->SignupKey = $teeTimes[$i]->Players[$j]->SignupKey;

            $player->Tee = "W";
            $player->Email = "";
            if($player->GHIN !== 0){
                if(array_key_exists($player->GHIN, $activeRoster)){
                    $rosterEntry = $activeRoster[$player->GHIN];
                    $player->Email = $rosterEntry-> Email;
                    $player->Tee = $rosterEntry->Tee;
                } else {
                    if($i < 10){
                        echo $player->GHIN . " not in roster ";
                    }
                }
            }
    
            $teeTime->Players[] = $player;
        }
    
        $teeTimeComposite->TeeTimes[] = $teeTime;
    }
}

function cmgc_admin_get_tee_times($connection, $tournamentKey) {
	$sqlCmd = "SELECT * FROM `TeeTimes` WHERE `TournamentKey` = ? ORDER BY `StartTime` ASC";
	$query = $connection->prepare ( $sqlCmd );
	
	if (! $query) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	if (! $query->bind_param ( 'i', $tournamentKey )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}
	
	if (! $query->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
	
	$query->bind_result ( $key, $tournament, $startTime, $startHole );
	
	$teeTimeArray = array();
	
	while ( $query->fetch () ) {
		$teeTime = new cmgc_admin_DatabaseTeeTime ();
		$teeTime->Key = $key;
		$teeTime->StartTime = $startTime;
		$teeTime->StartHole = $startHole;
		$teeTimeArray [] = $teeTime;
	}
	
	$query->close ();
	
	for($i = 0; $i < count ( $teeTimeArray ); ++ $i) {
		$teeTimeArray [$i]->Players = cmgc_admin_get_players_for_tee_time ( $connection, $teeTimeArray [$i]->Key );
	}
	
	return $teeTimeArray;
}

function cmgc_admin_get_players_for_tee_time($connection, $teeTimeKey) {
	$sqlCmd = "SELECT * FROM `TeeTimesPlayers` WHERE `TeeTimeKey` = ? ORDER BY `Position` ASC";
	$query = $connection->prepare ( $sqlCmd );
	
	if (! $query) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	if (! $query->bind_param ( 'i', $teeTimeKey )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}
	
	if (! $query->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
	
	$query->bind_result ( $key, $tournament, $GHIN, $Name, $Position, $extra, $signupKey );
	
	// if (! $forWeb) {
	// echo date ( 'g:i', strtotime ( $teeTime ) );
	// }
	
	$playerCount = 0;
	$playerArray = array ();
	while ( $query->fetch () ) {
		$player = new cmgc_admin_DatabaseTeeTimePlayer ();
		$player->GHIN = $GHIN;
		$player->LastName = $Name;
		$player->Extra = $extra;
		$player->SignupKey = $signupKey;
		$playerArray [] = $player;
	}
	
	$query->close ();
	
    /*
	for($i = 0; $i < count ( $playerArray ); ++ $i) {
		$playerArray [$i]->Handicap = GetPlayerHandicap ( $connection, $playerArray [$i]->GHIN );
	}
    */
	return $playerArray;
}

function cmgc_admin_fill_in_waitList_players($connection, $tournamentKey, $teeTimeComposite, $activeRoster){

    $entries = cmgc_admin_get_signup_waitinglist($connection, $tournamentKey);

    $teeTimeComposite->WaitlistPlayers = array();
    for($i = 0; $i < count($entries); ++$i){
        
        if(intval($entries[$i]->GHIN1) === 0) {
            $playerSignUp = cmgc_admin_get_player_signup_by_name($connection, $tournamentKey, $entries[$i]->Name1);
        }
        else {
            $playerSignUp = cmgc_admin_get_player_signup($connection, $tournamentKey, $entries[$i]->GHIN1);
        }

        $player = new cmgc_admin_Player();
        if($playerSignUp){
            
            $player->Name = $playerSignUp->LastName;  // is actually full name
            $player->Position = $entries[$i]->Position; // waitlist position
            $player->GHIN = $playerSignUp->GHIN;
            $player->Extra = $playerSignUp->Extra;
            $player->SignupKey = $playerSignUp->SignUpKey;
        }
        else {
            // Take what info we have. If this player makes it into the
            // tournament, the tee time submission will create a signup.
            $player->Name = $entries[$i]->Name1;
            $player->Position = $entries[$i]->Position; // waitlist position
            $player->GHIN = $entries[$i]->GHIN1;
            $player->Extra = "";
            $player->SignupKey = 0;
        }

        $player->Email = "";
        $player->Tee = "W";
        if(array_key_exists($entries[$i]->GHIN1, $activeRoster)){
            $player->Email = $activeRoster[$entries[$i]->GHIN1]-> Email;
            $player->Tee = $activeRoster[$entries[$i]->GHIN1]->Tee;
        }

        $teeTime = new cmgc_admin_TeeTime();
        $teeTime->StartTime = '01:' . sprintf("%02d", $i) . ' PM';
        if($i >= 60){
            $teeTime->StartTime = '02:' . sprintf("%02d", $i - 60) . ' PM';
        }
        $teeTime->Players = array();
        $teeTime->Players[] = $player;

        $teeTimeComposite->WaitlistPlayers[] = $teeTime;
    }
}

function cmgc_admin_get_signup_waitinglist($connection, $tournamentKey){
	$sqlCmd = "SELECT * FROM `SignUpsWaitingList` WHERE TournamentKey = ? ORDER BY `Position` ASC";
	$entries = $connection->prepare ( $sqlCmd );
	
	if (! $entries) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	if (! $entries->bind_param ( 'i', $tournamentKey )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}
	
	if (! $entries->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
	
	$entries->bind_result ( $key, $position, $ghin1, $name1, $ghin2, $name2, $ghin3, $name3, $ghin4, $name4 );
	
	$waitingList = array();
	while ( $entries->fetch () ) {
		$entry = new cmgc_admin_SignUpWaitingListClass();
		$entry->TournamentKey = $tournamentKey;
		$entry->Position = $position;
		$entry->GHIN1 = $ghin1;
		$entry->Name1 = $name1;
		$entry->GHIN2 = $ghin2;
		$entry->Name2 = $name2;
		$entry->GHIN3 = $ghin3;
		$entry->Name3 = $name3;
		$entry->GHIN4 = $ghin4;
		$entry->Name4 = $name4;
		$waitingList[] = $entry;
	}
	
	$entries->close ();
	
	return $waitingList;
}

/*
 * Get the record for a signed up player by GHIN.  There must only be 1 record.
 */
function cmgc_admin_get_player_signup($connection, $tournamentKey, $playerGHIN) {
	
	$sqlCmd = "SELECT * FROM `SignUpsPlayers` WHERE `TournamentKey` = ? AND `PlayerGHIN` = ?";
	$player = $connection->prepare ( $sqlCmd );
	
	if (! $player) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	if (! $player->bind_param ( 'ii', $tournamentKey, $playerGHIN )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}
	
	if (! $player->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
	
	$player->bind_result ( $key, $tournament, $position, $GHIN, $LastName, $extra );
	
	$playerSignUp = null;

	$count = 1;
	 while($player->fetch()) {
	 	if($count > 1){
	 		die('Player ' . $playerGHIN . ' is signed up more than once');
	 	}
	 	//echo 'found player<br>';
	 	$playerSignUp = new cmgc_admin_PlayerSignUpClass();
	 	$playerSignUp->SignUpKey = $key;
	 	$playerSignUp->TournamentKey = $tournamentKey;
	 	$playerSignUp->GHIN = $GHIN;
	 	$playerSignUp->LastName = $LastName;
	 	$playerSignUp->Extra = $extra;
		$playerSignUp->Position = $position;
	 	$count++;
	 }
	 //if(!isset($playerSignUp)) { echo 'did not find player<br>'; }
	
	$player->close ();
	
	return $playerSignUp;
}

/*
 * Get the record for a signed up player by name.  There must only be 1 record.
 */
function cmgc_admin_get_player_signup_by_name($connection, $tournamentKey, $playerName) {
	
	$sqlCmd = "SELECT * FROM `SignUpsPlayers` WHERE `TournamentKey` = ? AND `LastName` = ?";
	$player = $connection->prepare ( $sqlCmd );
	
	if (! $player) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	if (! $player->bind_param ( 'is', $tournamentKey, $playerName )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}
	
	if (! $player->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
	
	$player->bind_result ( $key, $tournament, $position, $GHIN, $LastName, $extra );
	
	$playerSignUp = null;

	$count = 1;
	 while($player->fetch()) {
	 	if($count > 1){
	 		die('Player ' . $playerName . ' is signed up more than once');
	 	}
	 	//echo 'found player<br>';
	 	$playerSignUp = new cmgc_admin_PlayerSignUpClass();
	 	$playerSignUp->SignUpKey = $key;
	 	$playerSignUp->TournamentKey = $tournamentKey;
	 	$playerSignUp->GHIN = $GHIN;
	 	$playerSignUp->LastName = $LastName;
	 	$playerSignUp->Extra = $extra;
		$playerSignUp->Position = $position;
	 	$count++;
	 }
	 //if(!isset($playerSignUp)) { echo 'did not find player<br>'; }
	
	$player->close ();
	
	return $playerSignUp;
}

?>