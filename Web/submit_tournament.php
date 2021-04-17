<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$connection = new mysqli ( $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( 'db connect error: ' . $connection->connect_error );
	
	// var_dump($_POST);

login ( $_POST ['Login'], $_POST ['Password'] );

if ($_POST ['Action'] == 'Delete') {
	// Delete the tournament
	if(!isset($_POST ['TournamentKey']) || !is_numeric($_POST ['TournamentKey'])){
		die("No tournament key");
	}
	DeleteTournament($connection, $_POST ['TournamentKey']);
	
} else {
	
	// Collect the tournament data
	$t = new Tournament ();
	$t->Name = stripslashes($_POST ['Name']);
	$t->Year = $_POST ['Year'];
	// Date format is 2014-06-08
	$t->StartDate = $_POST ['StartDate'];
	$t->EndDate = $_POST ['EndDate'];
	$t->SignupStartDate = $_POST ['SignupStartDate'];
	$t->SignupEndDate = $_POST ['SignupEndDate'];
	$t->CancelEndDate = $_POST ['CancelEndDate'];
	$t->LocalHandicap = $_POST ['LocalHandicap'];
	$t->ScgaTournament = $_POST ['ScgaTournament'];
	$t->TeamSize = $_POST ['TeamSize'];
	$t->TournamentDescriptionKey = $_POST ['TournamentDescriptionKey'];
	$t->Cost = $_POST ['Cost'];
	$t->Pool = $_POST ['Pool'];
	$t->ChairmanName = $_POST ['ChairmanName'];
	$t->ChairmanEmail = $_POST ['ChairmanEmail'];
	$t->ChairmanPhone = $_POST ['ChairmanPhone'];
	$t->Stableford = $_POST ['Stableford'];
	$t->Eclectic = $_POST ['Eclectic'];
	$t->SendEmail = $_POST ['SendEmail'];
	$t->RequirePayment = $_POST ['RequirePayment'];
	$t->SCGAQualifier = $_POST ['SCGAQualifier'];
	$t->SrClubChampionship = $_POST ['SrClubChampionship'];
	if(isset($_POST['OnlineSignUp'])){
		$t->OnlineSignUp = $_POST['OnlineSignUp'];
	} else {
		$t->OnlineSignUp = 1;
	}
	if(isset($_POST['MatchPlay'])) {
		$t->MatchPlay = $_POST['MatchPlay'];
	} else {
		$t->MatchPlay = 0;
	}
	if(isset($_POST['AllowNonMemberSignup'])){
		$t->AllowNonMemberSignup = $_POST['AllowNonMemberSignup'];
	} else {
		$t->AllowNonMemberSignup = 0;
	}
	if(isset($_POST['AnnouncementOnly'])){
		$t->AnnouncementOnly = $_POST['AnnouncementOnly'];
	} else {
		$t->AnnouncementOnly = 0;
	}
	if(isset($_POST['MemberGuest'])){
		$t->MemberGuest = $_POST['MemberGuest'];
	} else {
		$t->MemberGuest = 0;
	}
	if(isset($_POST['MaxSignups'])){
		$t->MaxSignups = $_POST['MaxSignups'];
	} else {
		$t->MaxSignups = 0;
	}
	
	
	// var_dump($t);
	
	if ($_POST ['Action'] == 'Insert') {
		// Insert a new tournament
		InsertTournament ( $connection, $t );
	} else if ($_POST ['Action'] == 'Update') {
		// Update all the field of a tournament
		$t->TournamentKey = $_POST ['TournamentKey'];
		
		UpdateTournament($connection, $t->TournamentKey, 'Name', $t->Name, 's');
		UpdateTournament($connection, $t->TournamentKey, 'Year', $t->Year, 's');
		UpdateTournament($connection, $t->TournamentKey, 'StartDate', $t->StartDate, 's');
		UpdateTournament($connection, $t->TournamentKey, 'EndDate', $t->EndDate, 's');
		UpdateTournament($connection, $t->TournamentKey, 'SignupStartDate', $t->SignupStartDate, 's');
		UpdateTournament($connection, $t->TournamentKey, 'SignupEndDate', $t->SignupEndDate, 's');
		UpdateTournament($connection, $t->TournamentKey, 'CancelEndDate', $t->CancelEndDate, 's');
		UpdateTournament($connection, $t->TournamentKey, 'LocalHandicap', $t->LocalHandicap, 's');
		UpdateTournament($connection, $t->TournamentKey, 'ScgaTournament', $t->ScgaTournament, 'i');
		UpdateTournament($connection, $t->TournamentKey, 'TeamSize', $t->TeamSize, 'i');
		UpdateTournament($connection, $t->TournamentKey, 'TournamentDescriptionKey', $t->TournamentDescriptionKey, 'i');
		UpdateTournament($connection, $t->TournamentKey, 'Cost', $t->Cost, 'i');
		UpdateTournament($connection, $t->TournamentKey, 'Pool', $t->Pool, 'i');
		UpdateTournament($connection, $t->TournamentKey, 'ChairmanName', $t->ChairmanName, 's');
		UpdateTournament($connection, $t->TournamentKey, 'ChairmanEmail', $t->ChairmanEmail, 's');
		UpdateTournament($connection, $t->TournamentKey, 'ChairmanPhone', $t->ChairmanPhone, 's');
		UpdateTournament($connection, $t->TournamentKey, 'Stableford', $t->Stableford, 'i');
		UpdateTournament($connection, $t->TournamentKey, 'Eclectic', $t->Eclectic, 'i');
		UpdateTournament($connection, $t->TournamentKey, 'SendEmail', $t->SendEmail, 'i');
		UpdateTournament($connection, $t->TournamentKey, 'RequirePayment', $t->RequirePayment, 'i');
		UpdateTournament($connection, $t->TournamentKey, 'SCGAQualifier', $t->SCGAQualifier, 'i');
		UpdateTournament($connection, $t->TournamentKey, 'SrClubChampionship', $t->SrClubChampionship, 'i');
		UpdateTournament($connection, $t->TournamentKey, 'OnlineSignUp', $t->OnlineSignUp, 'i');
		UpdateTournament($connection, $t->TournamentKey, 'MatchPlay', $t->MatchPlay, 'i');
		UpdateTournament($connection, $t->TournamentKey, 'AllowNonMemberSignup', $t->AllowNonMemberSignup, 'i');
		UpdateTournament($connection, $t->TournamentKey, 'AnnouncementOnly', $t->AnnouncementOnly, 'i');
		UpdateTournament($connection, $t->TournamentKey, 'MemberGuest', $t->MemberGuest, 'i');
		UpdateTournament($connection, $t->TournamentKey, 'MaxSignups', $t->MaxSignups, 'i');
	}
}
echo "Success";
?>