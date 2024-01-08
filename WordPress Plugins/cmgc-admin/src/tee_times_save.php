<?php

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

class cmgc_admin_TeeTimeWaitingListClass {
	public $TournamentKey;
	public $Position;
	public $GHIN;
	public $Name;
    public $Extra;
}

class cmgc_admin_PlayerSignUpClass {
	public $SignUpKey;
	public $TournamentKey;
	public $Position;
	public $GHIN;
	public $LastName;
	public $Extra;
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

            // Check for Jr or Jr. in the last name and move to first name
            $fields = explode(' ', $lastName);
            if (count($fields) > 1)
            {
                if (strcasecmp($fields[count($fields) - 1], "jr") === 0)
                {
                    // Move the Jr. to the first name
                    $firstName = $firstName . " " + $fields[count($fields) - 1];
                    // Remove the Jr. from the last name
                    $lastName = trim(str_replace($fields[$fields.Length - 1], "", $lastName));
                }
            }

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
                // Member/Guest
                if(($extra === 'M') || ($extra === 'G')){
                    echo $extra;
                }
            }
            echo ',';
            echo $teeTimes[$i]->Players[$j]->Tee . PHP_EOL;
        }
    }
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

    $entries = cmgc_admin_get_tee_times_waitinglist($connection, $tournamentKey);

    $teeTimeComposite->WaitlistPlayers = array();
    for($i = 0; $i < count($entries); ++$i){
        
        if(intval($entries[$i]->GHIN) === 0) {
            $playerSignUp = cmgc_admin_get_player_signup_by_name($connection, $tournamentKey, $entries[$i]->Name);
        }
        else {
            $playerSignUp = cmgc_admin_get_player_signup($connection, $tournamentKey, $entries[$i]->GHIN);
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
            $player->Name = $entries[$i]->Name;
            $player->Position = $entries[$i]->Position; // waitlist position
            $player->GHIN = $entries[$i]->GHIN;
            $player->Extra = ""; // Not implemented yet
            $player->SignupKey = 0;
        }

        $player->Email = "";
        $player->Tee = "W";
        if(array_key_exists($entries[$i]->GHIN, $activeRoster)){
            $player->Email = $activeRoster[$entries[$i]->GHIN]-> Email;
            $player->Tee = $activeRoster[$entries[$i]->GHIN]->Tee;
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

function cmgc_admin_get_tee_times_waitinglist($connection, $tournamentKey){
	$sqlCmd = "SELECT * FROM `TeeTimesWaitingList` WHERE TournamentKey = ? ORDER BY `Position` ASC";
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
	
	$entries->bind_result ( $key, $position, $ghin, $name, $extra );
	
	$waitingList = array();
	while ( $entries->fetch () ) {
		$entry = new cmgc_admin_TeeTimeWaitingListClass();
		$entry->TournamentKey = $tournamentKey;
		$entry->Position = $position;
		$entry->GHIN = $ghin;
		$entry->Name = $name;
        $entry->Extra = $extra;
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

function cmgc_admin_fill_in_cancelled_layers($connection, $tournamentKey, $teeTimeComposite, $activeRoster){

    $cancelledList = cmgc_admin_get_tee_times_cancelled_list($connection, $tournamentKey);

    $teeTimeComposite->CancelledPlayers = array();
    for($i = 0; $i < count($cancelledList); ++$i){
        $player = new cmgc_admin_Player();
        $player->Name = $cancelledList[$i]->Name;
        $player->GHIN = $cancelledList[$i]->GHIN;
        $player->Position = $cancelledList[$i]->Position;
        $player->Extra = "";
        $player->SignupKey = 0; // may no longer be signed up

        $player->Tee = "W";
        $player->Email = "";
        if($player->GHIN !== 0){
            if(array_key_exists($player->GHIN, $activeRoster)){
                $player->Email = $activeRoster[$player->GHIN]-> Email;
                $player->Tee = $activeRoster[$player->GHIN]->Tee;
            }
        }

        $teeTime = new cmgc_admin_TeeTime();
        $teeTime->StartTime = '03:' . sprintf("%02d", $i) . ' PM';
        if($i >= 60){
            $teeTime->StartTime = '04:' . sprintf("%02d", $i - 60) . ' PM';
        }
        $teeTime->Players = array();
        $teeTime->Players[] = $player;

        $teeTimeComposite->CancelledPlayers[] = $teeTime;
    }
}

function cmgc_admin_get_tee_times_cancelled_list($connection, $tournamentKey){
	$sqlCmd = "SELECT * FROM `TeeTimesCancelled` WHERE TournamentKey = ? ORDER BY `Position` ASC";
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
	
	$entries->bind_result ( $key, $position, $ghin, $name );
	
	$teeTimesCancelledList = array();
	while ( $entries->fetch () ) {
		$entry = new cmgc_admin_TeeTimeCancelledPlayer();
		$entry->TournamentKey = $tournamentKey;
		$entry->Position = $position;
		$entry->GHIN = $ghin;
		$entry->Name = $name;
		$teeTimesCancelledList[] = $entry;
	}
	
	$entries->close ();
	
	return $teeTimesCancelledList;
}

?>
