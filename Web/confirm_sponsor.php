<?php

require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';

date_default_timezone_set ( 'America/Los_Angeles' );

$overrideTitle = "Confirm Sponsor";
get_header ();
get_sidebar ();

$connection = new mysqli ('p:' . $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( $connection->connect_error );

echo ' <div id="content-container" class="entry-content">';
echo '    <div id="content" role="main">';

if(!empty($_GET ['application'])){
    $application = $_GET ['application'];
    if (!is_numeric($application)) {
        die ( "Bad application ID" );
    } 
} else {
    die ( "Missing application ID" );
}

if(!empty($_GET ['sponsor'])){
    $sponsor = $_GET ['sponsor'];
    if (!is_numeric($sponsor)) {
        die ( "Bad sponsor ID" );
    } 
} else {
    die ( "Missing sponsor" );
}

$sponsorFound = CheckSponsorship($connection, $application, $sponsor);
if($sponsorFound){
    ConfirmSponsorship($connection, $application, $sponsor);
    echo '<h2 style="text-align:center">Sponsorship confirmed. Thank you!</ht>';
} else {
    echo "Application " . $application . " does not have sponsor " . $sponsor;
}

echo '    </div><!-- #content -->';
echo ' </div><!-- #content-container -->';

$connection->close ();
get_footer ();

function CheckSponsorship($connection, $applicationID, $ghin){

	$sqlCmd = "SELECT Confirmed FROM `MembershipSponsors` WHERE `ApplicationRecordKey` = ? AND `GHIN` = ?";
	$query = $connection->prepare ( $sqlCmd );

	if (! $query->bind_param ( 'ii', $applicationID, $ghin )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}

	if (! $query) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}

	if (! $query->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}

	$query->bind_result ($confirmed);

	$count = 0;
	while ( $query->fetch () ) {
        $count++;
	}

    if($count == 1){
        return true;
    } else if($count > 1){
        die ( "Found " . $count . " records for application " . $applicationID . " and sponsor " . $ghin );
    }

	return false;
}

function ConfirmSponsorship($connection, $applicationID, $ghin){

    $sqlCmd = "UPDATE `MembershipSponsors` SET `Confirmed`= ? WHERE `ApplicationRecordKey` = ? AND `GHIN` = ?";
	$query = $connection->prepare ( $sqlCmd );

    $confirmed = 1;
	if (! $query->bind_param ( 'iii', $confirmed, $applicationID, $ghin )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}

	if (! $query) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}

	if (! $query->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
}

?>