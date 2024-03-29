<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/functions.php';

class Tournament {
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
	
	// I didn't figure out how to convert 0/1
	// to true/false on the receiving end ...
	public function ConvertToBool()
	{
		$this->LocalHandicap = IntToBool($this->LocalHandicap);
		$this->ScgaTournament = IntToBool($this->ScgaTournament);
		$this->SendEmail = IntToBool($this->SendEmail);
		$this->RequirePayment = IntToBool($this->RequirePayment);
		$this->Eclectic = IntToBool($this->Eclectic);
		$this->Stableford = IntToBool($this->Stableford);
		$this->SCGAQualifier = IntToBool($this->SCGAQualifier);
		$this->ClubChampionship = IntToBool($this->ClubChampionship);
		$this->SrClubChampionship = IntToBool($this->SrClubChampionship);
		$this->OnlineSignUp = IntToBool($this->OnlineSignUp);
		$this->MatchPlay = IntToBool($this->MatchPlay);
		$this->AllowNonMemberSignup = IntToBool($this->AllowNonMemberSignup);
		$this->AnnouncementOnly = IntToBool($this->AnnouncementOnly);
		$this->MemberGuest = IntToBool($this->MemberGuest);
		$this->PayAtSignup = IntToBool($this->PayAtSignup);
	}
}
class TournamentDetails {
	public $TournamentKey;
	public $TeeTimesPostedDate;
	public $ScoresPostedDate;
	public $ChitsPostedDate;
	public $PoolPostedDate;
	public $ClosestToThePinPostedDate;
	public $ResultsAreUnofficial;
	public $ScoresFile;
	public $ChitsFile;
	public $PoolFile;
	public $GolfGeniusResults;
	
	const EMPTYDATE = "1900-01-01";
	
	function TournamentDetails(){
		$this->TeeTimesPostedDate = self::EMPTYDATE;
		$this->ScoresPostedDate = self::EMPTYDATE;
		$this->ChitsPostedDate = self::EMPTYDATE;
		$this->PoolPostedDate = self::EMPTYDATE;
		$this->ClosestToThePinPostedDate = self::EMPTYDATE;
		$this->ResultsAreUnofficial = false;
		$this->GolfGeniusResults = NULL;
	}
}
class PayPalDetails {
	public $PayPayButton;
	public $TournamentFee;
	public $ProcessingFee;
	public $Players1;  // PayPal index into combobox for 1 player
	public $Players2;  // PayPal index into combobox for 2 players
	public $Players3;  // PayPal index into combobox for 3 players
	public $Players4;  // PayPal index into combobox for 4 players
}

function GetFriendlyNonHtmlTournamentDates($tournament){
    if (strcmp ( $tournament->StartDate, $tournament->EndDate ) == 0) {
        return date ( 'M d', strtotime ( $tournament->StartDate ) );
	} else {
        $startMonth = date ( 'M', strtotime ( $tournament->StartDate ) );
        $endMonth = date ( 'M', strtotime ( $tournament->EndDate ) );
        if (strcmp ( $startMonth, $endMonth ) == 0) {
            // same month
            return date ( 'M d-', strtotime ( $tournament->StartDate ) ) . date ( 'd', strtotime ( $tournament->EndDate ) );
        } else {
            // different months
            return date ( 'M d-', strtotime ( $tournament->StartDate ) ) . date ( 'M d', strtotime ( $tournament->EndDate ) );
        }
    }
}

function GetFriendlyTournamentDates($tournament){
	if (strcmp ( $tournament->StartDate, $tournament->EndDate ) == 0) {
		
		return GetUnbreakableHtmlDateString(date ( 'M d', strtotime ( $tournament->StartDate ) ));
	} else {
		$startMonth = date ( 'M', strtotime ( $tournament->StartDate ) );
		$endMonth = date ( 'M', strtotime ( $tournament->EndDate ) );
		if (strcmp ( $startMonth, $endMonth ) == 0) {
			// same month
			return GetUnbreakableHtmlDateString(date ( 'M d-', strtotime ( $tournament->StartDate ) ) . date ( 'd', strtotime ( $tournament->EndDate ) ));
		} else {
			// different months
			// Allow line break on dash
			return GetUnbreakableHtmlDateString(date ( 'M d', strtotime ( $tournament->StartDate ) )) . 
				'-' . 
				GetUnbreakableHtmlDateString(date ( 'M d', strtotime ( $tournament->EndDate ) ));
		}
	}
}

// Create a string that cannot be broken across lines
function GetUnbreakableHtmlDateString($date)
{
	// Since it is easy to confuse 12pm with midnight, make it
	// clear that it is noon
	$date = str_replace("12pm", "noon", $date);

	// nbsp is non-breaking space
	$date = str_replace(" ", "&nbsp;", $date);
	// #8209 is non-breaking dash
	$date = GetUnbreakableDash($date);
	return $date;
}

// Replace dashes with dash that cannot be broken across lines
function GetUnbreakableDash($name)
{
	return str_replace("-", "&#8209;", $name);
}

function DisplayTournamentDetails($t){
	echo '<h4>The Usual Details</h4>' . PHP_EOL;
	echo '<ul>' . PHP_EOL;
	echo '<li>Entry fee: $' . $t->Cost . ' per person</li>' . PHP_EOL;
	if(isset($t->ChairmanName) && (strlen($t->ChairmanName) > 0)){
		echo '<li><a href="mailto:' . $t->ChairmanEmail . '">Contact tournament director</a></li>' . PHP_EOL;
	}
	echo '</ul>' . PHP_EOL;
}
/*
 * Get all the tournaments for a single year.
 * Return all tournaments if the year is left empty.
 */
function GetTournaments($connection, $year) {
	
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
		$t = new Tournament();
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
		
		if(empty($year)){
			// return all tournaments
			$tournaments[] = $t;
		}
		else {
			// return tournaments only for a single year
			$d = strtotime($StartDate);
			$tournamentYear = date("Y", $d);
			if(strcmp($tournamentYear, $year) == 0){
				$tournaments[] = $t;
			}
		}
	}

	$tournament->close ();
	
	return $tournaments;
}
/*
 * Get a single tournament.
 */
function GetTournament($connection, $tournamentKey) {
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
		$t = new Tournament();
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
 * Get details about the tournament -- mostly data
 * about the tournament results.
 */
function GetTournamentDetails($connection, $tournamentKey){
	$sqlCmd = "SELECT * FROM `TournamentDetails` WHERE `TournamentKey` = ?";
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

	$tournament->bind_result ( $key, $teeTimesPostedDate, $scoresPostedDate, $chitsPostedDate, $poolPostedDate,
								$closestToThePinPostedDate, $resultsAreUnofficial, $scoresFile, $chitsFile, $poolFile, $golfGeniusResults );
	
	$details = new TournamentDetails();
	$details->TournamentKey = $tournamentKey;
	$details->TeeTimesPostedDate = TournamentDetails::EMPTYDATE;
	$details->ScoresPostedDate = TournamentDetails::EMPTYDATE;
	$details->ChitsPostedDate = TournamentDetails::EMPTYDATE;
	$details->PoolPostedDate = TournamentDetails::EMPTYDATE;
	$details->ClosestToThePinPostedDate = TournamentDetails::EMPTYDATE;
	if($tournament->fetch ()){
		$details->TeeTimesPostedDate = $teeTimesPostedDate;
		$details->ScoresPostedDate = $scoresPostedDate;
		$details->ChitsPostedDate = $chitsPostedDate;
		$details->PoolPostedDate = $poolPostedDate;
		$details->ClosestToThePinPostedDate = $closestToThePinPostedDate;
		$details->ResultsAreUnofficial = $resultsAreUnofficial;
		$details->ScoresFile = $scoresFile;
		$details->ChitsFile = $chitsFile;
		$details->PoolFile = $poolFile;
		$details->GolfGeniusResults = $golfGeniusResults;
	}
	
	$tournament->close ();
	
	return $details;
}
function UpdateTournamentDetails($connection, $tournamentKey, $field, $value){
	
	// Create the record if it does not exist
	CreateTournamentDetails($connection, $tournamentKey);
	
	switch($field){
		case 'ScoresFile':
		case 'ScoresPostedDate':
		case 'ChitsFile':
		case 'ChitsPostedDate':
		case 'PoolFile':
		case 'PoolPostedDate':
		case 'ClosestToThePinPostedDate':
		case 'GolfGeniusResultsLink':
			$sqlCmd = "UPDATE `TournamentDetails` SET `" . $field . "` = ? WHERE `TournamentKey` = ?";
			$paramType = 's';
			break;
		default:
			die("tournament_functions.php: Did not provide valid field parameter to UpdateTournamentDetails: " . $field);
	}
	
	$update = $connection->prepare ( $sqlCmd );
	
	if (! $update) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	if (! $update->bind_param ( $paramType . 'i',  $value, $tournamentKey)) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}
	
	if (! $update->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
	
	$update->close ();
}
/*
 * Get the tournaments that are active or signups are upcoming 
 * in the next 2 weeks.
 */
function GetCurrentTournaments($connection) {
	
	$tournaments = GetTournaments($connection, '');
		
	$now = new DateTime ( "now" );
	$currentTournaments = array();
	$nonAnnouncementTournaments = 0;

	for($i = 0; $i < count($tournaments); ++$i){
		// Get the start date
		$signupStartMinus2Weeks = new DateTime ( $tournaments[$i]->SignupStartDate );
		// Give 2 weeks notice
		$signupStartMinus2Weeks->sub(new DateInterval ( 'P14D' ));
		
		$end = new DateTime($tournaments[$i]->EndDate);
		$end->add(new DateInterval ( 'PT23H59M' ));
		
		//echo 'signup start minus 2 weeks ' . date ( 'M d Y', $signupStartMinus2Weeks->getTimestamp() ) . ', end ' . date ( 'M d Y', $end->getTimestamp() ) . ', now ' . date ( 'M d Y', $now->getTimestamp() ) . '<br>';

		// Include tournaments within 2 weeks of signup starting
		// Always show the next (non announcement) tournament
		if((($signupStartMinus2Weeks <= $now) || ($nonAnnouncementTournaments == 0)) && ($end >= $now)){
			
			$currentTournaments[] = $tournaments[$i];
			if(!$tournaments[$i]->AnnouncementOnly){
				$nonAnnouncementTournaments++;
			}
		}
	}
	
	return $currentTournaments;
}
/*
 * Get all the tournaments that have completed in
 * the last week
 */
function GetRecentlyCompletedTournaments($connection) {

	$tournaments = GetTournaments($connection, '');

	$now = new DateTime ( "now" );
	$currentTournaments = Array();

	for($i = 0; $i < count($tournaments); ++$i){
		// Get the start date
		$start = new DateTime ( $tournaments[$i]->StartDate );
		// this is just for testing
		//$start->sub(new DateInterval ( 'P28D' ));

		$endPlusWeek = new DateTime($tournaments[$i]->EndDate);
		// Show the details for a week
		$endPlusWeek->add(new DateInterval ( 'P8D' ));

		//echo 'start ' . date ( 'M d', $start->getTimestamp() ) . ', end ' . date ( 'M d', $end->getTimestamp() ) . '<br>';

		if(($start <= $now) && ($endPlusWeek >= $now)){
			$details = GetTournamentDetails($connection, $tournaments[$i]->TournamentKey);

			if (($details->ChitsPostedDate != TournamentDetails::EMPTYDATE) ||
				($details->ClosestToThePinPostedDate != TournamentDetails::EMPTYDATE) ||
				($details->PoolPostedDate != TournamentDetails::EMPTYDATE) ||
				($details->ScoresPostedDate != TournamentDetails::EMPTYDATE) ||
				(!empty($details->GolfGeniusResults)) ||
				($tournaments[$i]->MatchPlay == 1)) {
				$currentTournaments[] = $tournaments[$i];
			}
		}
	}

	return $currentTournaments;
}

/*
 * Show the tournaments that have completed in the last week
 */
function ShowRecentlyCompletedTournaments($connection, $script_folder_href){
	$currentTournaments = GetRecentlyCompletedTournaments ( $connection );
	if(isset($currentTournaments) && (count($currentTournaments) > 0)){
		echo '<h2>Recent Tournaments:</h2>';
		echo '<table style="border:none;margin-left:30px;"><tbody>' . PHP_EOL;
	
		for($i = 0; $i < count($currentTournaments); ++$i){
			echo '<tr style="background-color:white;font-size:large;">';
			ShowTournamentResults($connection, $currentTournaments [$i], 'style="border:none"', true, $script_folder_href, false);
			echo '</tr>' . PHP_EOL;
		}
		echo '</tbody></table>' . PHP_EOL;
	}
}

function ShowTournamentResults($connection, $tournament, $style, $abbreviated, $script_folder_href, $showAnnouncements)
{
	echo '<td ' . $style . '>' . GetFriendlyTournamentDates($tournament) . '</td>';
	
	if($tournament->AnnouncementOnly){
		if($showAnnouncements){
			echo '<td colspan="6" style="text-align:center" ' . $style . '> ------ ' . $tournament->Name . ' ------ </td>';
		}
		return;
	}
	
	echo '<td ' . $style . '>' . $tournament->Name . '</td>';
	if(!$abbreviated){
		if($tournament->TournamentDescriptionKey > 0){
			echo '<td ' . $style . '><a href="' . $script_folder_href . 'tournament_description.php?tournament=' . $tournament->TournamentKey . '">Description</a></td>';
		}
		else {
			echo '<td ' . $style . '> Description </td>';
		}
	}
	
	ShowTournamentResultsLinks($connection, $tournament, $style, '', $script_folder_href);
}

function ShowTournamentResultsLinks($connection, $tournament, $style, $skipThisResult, $script_folder_href) {
	$details = GetTournamentDetails ( $connection, $tournament->TournamentKey );
	
	if ($skipThisResult != 'Scores') {
		if (($details->ScoresPostedDate != TournamentDetails::EMPTYDATE) || $tournament->MatchPlay) {
			echo '<td ' . $style . '><a href="' . $script_folder_href . 'results.php?tournament=' . $tournament->TournamentKey . '&amp;result=scores">Scores</a></td>';
		} else {
			echo '<td ' . $style . '>Scores</td>';
		}
	}
	
	if ($skipThisResult != 'Chits') {
		if ($details->ChitsPostedDate != TournamentDetails::EMPTYDATE) {
			echo '<td ' . $style . '><a href="' . $script_folder_href . 'results.php?tournament=' . $tournament->TournamentKey . '&amp;result=chits">Chits</a></td>';
		} else {
			echo '<td ' . $style . '>Chits</td>';
		}
	}

	if ($skipThisResult != 'GolfGeniusResults') {
		if (!empty($details->GolfGeniusResults)) {
			// Open results in a new tab
			echo '<td ' . $style . '><a href="' . $details->GolfGeniusResults . '" target="_blank">Golf Genius Results</a></td>';
		} else {
			echo '<td ' . $style . '>Golf Genius Results</td>';
		}
	}
}

/*
 * Insert a tournament into the database
 */
function InsertTournament($connection, $tournament) {
	// This has become too long ...
	$sqlCmd = "INSERT INTO `Tournaments` VALUES (NULL, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
	$insert = $connection->prepare ( $sqlCmd );
	
	if (! $insert) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	// This has become too long ...
	if (! $insert->bind_param ( 'sssssssiiiiiisssiiiiiiiiiiiii', $tournament->Name, $tournament->Year, $tournament->StartDate, $tournament->EndDate, 
			$tournament->SignupStartDate, $tournament->SignupEndDate, $tournament->CancelEndDate, $tournament->LocalHandicap, 
			$tournament->ScgaTournament, $tournament->TeamSize, $tournament->TournamentDescriptionKey,
			$tournament->Cost, $tournament->Pool, $tournament->ChairmanName, $tournament->ChairmanEmail, 
			$tournament->ChairmanPhone, $tournament->Stableford, $tournament->Eclectic, $tournament->SendEmail,
			$tournament->RequirePayment, $tournament->SCGAQualifier, $tournament->SrClubChampionship,
			$tournament->OnlineSignUp, $tournament->MatchPlay, $tournament->AllowNonMemberSignup,
			$tournament->AnnouncementOnly, $tournament->MemberGuest, $tournament->ClubChampionship, $tournament->PayAtSignup )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}
	
	if (! $insert->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
	
	// echo 'insert id is: ' . $insert->insert_id . '<br>';
	return $insert->insert_id;
}
function DeleteTournament($connection, $tournamentKey){
	$t = GetTournament($connection, $tournamentKey);
	if(is_null($t)){
		die('Tournament with key ' . $tournamentKey . ' does not exist');
	}
	
	// Clear all the tables that have tournament related data
	ClearTableWithTournamentKey($connection, 'Chits', $tournamentKey);
	ClearTableWithTournamentKey($connection, 'ClosestToThePin', $tournamentKey);
	ClearTableWithTournamentKey($connection, 'Pool', $tournamentKey);
	ClearTableWithTournamentKey($connection, 'Scores', $tournamentKey);
	ClearTableWithTournamentKey($connection, 'SignUps', $tournamentKey);
	ClearTableWithTournamentKey($connection, 'SignUpsPlayers', $tournamentKey);
	ClearTableWithTournamentKey($connection, 'TeeTimes', $tournamentKey);
	ClearTableWithTournamentKey($connection, 'TeeTimesPlayers', $tournamentKey);
	ClearTableWithTournamentKey($connection, 'TournamentDetails', $tournamentKey);
	ClearTableWithTournamentKey($connection, 'Tournaments', $tournamentKey);
}
function UpdateTournament($connection, $tournamentKey, $field, $value, $paramType){

	$sqlCmd = "UPDATE `Tournaments` SET `" . $field . "` = ? WHERE `TournamentKey` = ?";

	$update = $connection->prepare ( $sqlCmd );
	
	if (! $update) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	if (! $update->bind_param ( $paramType . 'i',  $value, $tournamentKey)) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}
	
	if (! $update->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
	
	$update->close ();
}

function IsPastSignupPeriod($tournament)
{
	$now = new DateTime ( "now" );
	$endSignUp = new DateTime($tournament->SignupEndDate);
	// Allow changes 6 full days after signup ends.
	// 7 days from 12AM of the 20th is 12AM of 27th.
	$endSignUp->add(new DateInterval ( 'P7D'  )); 
	if($now > $endSignUp) {
		return true;
	}
	return false;
}

function GetPrevious2DayTournamentKey($connection, $tournamentKey){

	// Get tournaments ordered by start date
	$tournaments = GetTournaments($connection, '');

	$currentIndex = -1;
	for($i = 0; ($i < count($tournaments)) && ($currentIndex == -1); ++$i){
		if($tournamentKey == $tournaments[$i]->TournamentKey){
			$currentIndex = $i;
		}
	}

	if($currentIndex == -1){
		return -1;
	}

	// Only find previous tournament if this is a 2 day tournament
	if($tournaments[$currentIndex]->StartDate == $tournaments[$currentIndex]->EndDate){
		//echo "This is a 1 day tournament<br>";
		return -1;
	}

	for($i = $currentIndex - 1; $i >= 0; --$i){
		// Skip announcements
		if(!$tournaments[$i]->AnnouncementOnly){
			// Find previous 2 day tournament
			if($tournaments[$i]->StartDate != $tournaments[$i]->EndDate){
				//echo "found previous 2 day tournament: " . $tournaments[$i]->Name . "<br>";
				return $tournaments[$i]->TournamentKey;
			}
			//echo "previous is a 1 day tournament: " . $tournaments[$i]->Name . "<br>";
		}
	}

	return -1;
}

?>