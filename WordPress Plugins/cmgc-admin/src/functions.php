<?php

class cmgc_admin_RosterEntry {
	public $GHIN;
	public $LastName;
	public $FirstName;
	public $Active;
	public $Email;
	public $BirthDate;
	public $DateAdded;
	public $MembershipType;
	public $SignupPriority;
	public $Tee;
}

class cmgc_admin_Tournament {
	public $TournamentKey;
	public $Name;
	public $Year;
	public $StartDate;
	public $EndDate;
	public $SignupStartDate;
	public $SignupEndDate;
	public $CancelEndDate;
	public $LocalHandicap;
	public $ScgaTournament;
	public $TeamSize;
	public $TournamentDescriptionKey;
	public $Cost;
	public $Pool;
	public $ChairmanName;
	public $ChairmanEmail;
	public $ChairmanPhone;
	public $Stableford;
	public $Eclectic;
	public $SendEmail;
	public $RequirePayment;
	public $SCGAQualifier;
	public $ClubChampionship;
	public $SrClubChampionship;
	public $OnlineSignUp;
	public $MatchPlay;
	public $AllowNonMemberSignup;
	public $AnnouncementOnly;
	public $MemberGuest;
	public $PayAtSignup;
	
	private function IntToBool($value){
		return (!isset($value) || ($value == 0)) ? "false" : "true";
	}
}

function cmgc_admin_clear_table($connection, $table) {
	$sqlCmd = "DELETE FROM `" . $table . "` ";
	$signups = $connection->prepare ( $sqlCmd );
	
	if (! $signups) {
		wp_die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	if (! $signups->execute ()) {
		wp_die ( $sqlCmd . " execute failed: " . $connection->error );
	}
}

function cmgc_admin_FixNameCasing($name)
{
	if(empty($name)){
		return $name;
	}
	
	$name = stripslashes($name);

	// If the name already has both upper and lower case characters,
	// just use the name as-is.
	if(strtolower($name) != $name && strtoupper($name) != $name){
		return(trim($name));
	}

	$nameArray = explode(',', $name);
	if(count($nameArray) == 1){
		return ucfirst(strtolower(trim($nameArray[0])));
	}
	
	$lastName = ucfirst(strtolower(trim($nameArray[0])));
	if(strpos($lastName, ' ') !== FALSE){
		$lastName = cmgc_admin_FixCasingWithinName($lastName, ' ');
	}
	if (strpos($lastName, "'") !== FALSE){
		// Upper case first letter after apostrophe
		$lastName = cmgc_admin_FixCasingWithinName($lastName, "'");
	}
	// Fix last names that start with Mc
	if(0 === strpos($lastName, "Mc")){
		$lastName = substr($lastName, strlen("Mc"));
		// Upper case character after Mc and add Mc back in
		$lastName = "Mc" . ucfirst($lastName);
	}
	
	$firstName = ucfirst(strtolower(trim($nameArray[1])));
	if(strpos($firstName, ' ') !== FALSE){
		$firstName = cmgc_admin_FixCasingWithinName($firstName, ' ');
	}
	if (strpos($firstName, '(') !== FALSE){
		// change (ty) back to (Ty)
		$firstName = cmgc_admin_FixCasingWithinName($firstName, '(');
	}
	
	return $lastName . ', ' . $firstName;
}

function cmgc_admin_FixCasingWithinName($name, $separator)
{
	$nameArray = explode($separator, $name);
	for($i = 0; $i < count($nameArray); ++$i)
	{
		if($i === 0){
			$name = ucfirst($nameArray[$i]);
		}
		else {
			$name = $name . $separator . ucfirst($nameArray[$i]);
		}
	}
	return $name;
}

/*
 * Get a single tournament.
 */
function cmgc_admin_get_tournament($connection, $tournamentKey) {
	$sqlCmd = "SELECT * FROM `Tournaments` WHERE `TournamentKey` = ?";
	$tournament = $connection->prepare ( $sqlCmd );
	
	if (! $tournament) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}

	if (! $tournament->bind_param ( 'i', $tournamentKey )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}
	
	if (! $tournament->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
	
	$tournament->bind_result ( $key, $Name, $Year, $StartDate, $EndDate, $SignupStartDate, 
			$SignupEndDate, $CancelEndDate, $LocalHandicap, $SCGATournament, $TeamSize, $tournamentDescriptionKey,
			$cost, $pool, $chairmanName, $chairmanEmail, $chairmanPhone, $stableford,
			$eclectic, $sendEmail, $requirePayment, $scgaQualifier, $srClubChampionship, $onlineSignUp, $matchPlay,
			$allowNonMemberSignup, $announcementOnly, $memberGuest, $clubChampionship, $payAtSignup ); 

	$t = null;
	if($tournament->fetch ()){
		$t = new cmgc_admin_Tournament();
		$t->CancelEndDate = $CancelEndDate;
		$t->EndDate = $EndDate;
		$t->LocalHandicap = $LocalHandicap;
		$t->Name = $Name;
		$t->ScgaTournament = $SCGATournament;
		$t->SignupEndDate = $SignupEndDate;
		$t->SignupStartDate = $SignupStartDate;
		$t->StartDate = $StartDate;
		$t->TeamSize = $TeamSize;
		$t->TournamentKey = $tournamentKey;
		$t->TournamentDescriptionKey = $tournamentDescriptionKey;
		$t->Cost = $cost;
		$t->Pool = $pool;
		$t->ChairmanName = $chairmanName;
		$t->ChairmanEmail = $chairmanEmail;
		$t->ChairmanPhone = $chairmanPhone;
		$t->Stableford = $stableford;
		$t->Eclectic = $eclectic;
		$t->SendEmail = $sendEmail;
		$t->RequirePayment = $requirePayment;
		$t->SCGAQualifier = $scgaQualifier;
		$t->ClubChampionship = $clubChampionship;
		$t->SrClubChampionship = $srClubChampionship;
		$t->OnlineSignUp = $onlineSignUp;
		$t->MatchPlay = $matchPlay;
		$t->AllowNonMemberSignup = $allowNonMemberSignup;
		$t->AnnouncementOnly = $announcementOnly;
		$t->MemberGuest = $memberGuest;
		$t->PayAtSignup = $payAtSignup;
	}

	$tournament->close ();
	
	return $t;
}

/*
 * Get all tournaments
 */
function cmgc_admin_get_tournaments($connection) {
	
	$sqlCmd = "SELECT * FROM `Tournaments` ORDER BY `StartDate`";
	$tournament = $connection->prepare ( $sqlCmd );
	
	if (! $tournament) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	if (! $tournament->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}

	$tournament->bind_result ( $tournamentKey, $Name, $Year, $StartDate, $EndDate, $SignupStartDate, 
			$SignupEndDate, $CancelEndDate, $LocalHandicap, $SCGATournament, $TeamSize, 
			$TournamentDescriptionKey, $cost, $pool, $chairmanName, $chairmanEmail, $chairmanPhone, $stableford,
			$eclectic, $sendEmail, $requirePayment, $scgaQualifier, $srClubChampionship, $onlineSignUp, $matchPlay,
			$allowNonMemberSignup, $announcementOnly, $memberGuest, $clubChampionship, $payAtSignup );

	$tournaments = array();
	while ( $tournament->fetch () ) {
		$t = new cmgc_admin_Tournament();
		$t->CancelEndDate = $CancelEndDate;
		$t->EndDate = $EndDate;
		$t->LocalHandicap = $LocalHandicap;
		$t->Name = $Name;
		$t->ScgaTournament = $SCGATournament;
		$t->SignupEndDate = $SignupEndDate;
		$t->SignupStartDate = $SignupStartDate;
		$t->StartDate = $StartDate;
		$t->TeamSize = $TeamSize;
		$t->TournamentKey = $tournamentKey;
		$t->Year = $Year;
		$t->TournamentDescriptionKey = $TournamentDescriptionKey;
		$t->Cost = $cost;
		$t->Pool = $pool;
		$t->ChairmanName = $chairmanName;
		$t->ChairmanEmail = $chairmanEmail;
		$t->ChairmanPhone = $chairmanPhone;
		$t->Stableford = $stableford;
		$t->Eclectic = $eclectic;
		$t->SendEmail = $sendEmail;
		$t->RequirePayment = $requirePayment;
		$t->SCGAQualifier = $scgaQualifier;
		$t->ClubChampionship = $clubChampionship;
		$t->SrClubChampionship = $srClubChampionship;
		$t->OnlineSignUp = $onlineSignUp;
		$t->MatchPlay = $matchPlay;
		$t->AllowNonMemberSignup = $allowNonMemberSignup;
		$t->AnnouncementOnly = $announcementOnly;
		$t->MemberGuest = $memberGuest;
		$t->PayAtSignup = $payAtSignup;
		
		// return all tournaments
		$tournaments[] = $t;
	}

	$tournament->close ();
	
	return $tournaments;
}

function cmgc_admin_get_roster_entry($connection, $playerGHIN) {
	if (empty ( $playerGHIN )) {
		return '';
	}

	$sqlCmd = "SELECT * FROM `Roster` WHERE `GHIN` = ? ";
	$player = $connection->prepare ( $sqlCmd );

	if (! $player) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}

	if (! $player->bind_param ( 'i', $playerGHIN )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}

	if (! $player->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}

	$player->bind_result ( $GHIN, $lastName, $firstName, $active, $email, $birthDate, $dateAdded, $membershipType, $signupPriority, $tee );

	if ( $player->fetch () ) {
		$rosterEntry = new cmgc_admin_RosterEntry();
		$rosterEntry->LastName = $lastName;
		$rosterEntry->FirstName = $firstName;
		$rosterEntry->Active = $active;
		$rosterEntry->Email = $email;
		$rosterEntry->BirthDate = $birthDate;
		$rosterEntry->DateAdded = $dateAdded;
		$rosterEntry->MembershipType = $membershipType;
		$rosterEntry->SignupPriority = $signupPriority;
		$rosterEntry->Tee = $tee;
		return $rosterEntry;
	}
	else {
		return null;
	}
	
	$player->close();
}

function cmgc_admin_get_all_active_roster_entries($connection) {

	$sqlCmd = "SELECT * FROM `Roster` WHERE `Active` = 1";
	$player = $connection->prepare ( $sqlCmd );

	if (! $player) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}

	if (! $player->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}

	$player->bind_result ( $GHIN, $lastName, $firstName, $active, $email, $birthDate, $dateAdded, $membershipType, $signupPriority, $tee );

	$activeRoster = array();
	while ( $player->fetch () ) {
		$rosterEntry = new cmgc_admin_RosterEntry();
		$rosterEntry->LastName = $lastName;
		$rosterEntry->FirstName = $firstName;
		$rosterEntry->Active = $active;
		$rosterEntry->Email = $email;
		$rosterEntry->BirthDate = $birthDate;
		$rosterEntry->DateAdded = $dateAdded;
		$rosterEntry->MembershipType = $membershipType;
		$rosterEntry->SignupPriority = $signupPriority;
		$rosterEntry->Tee = $tee;
		$activeRoster[$GHIN] = $rosterEntry;
	}

	$player->close();

	return $activeRoster;
}

?>