<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$overrideTitle = "Local Handicap";
get_header ();

class LocalHandicapEntry {
	public $GHIN;
	public $SCGAHandicap;
	public $LocalHandicap;
	public $LowerHandicap;
	public $CalculatedHandicap;
	public $LastName;
	public $FirstName;
}

$connection = new mysqli ('p:' . $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error){
	die ( $connection->connect_error );
}

echo ' <div id="content-container" class="entry-content">';
echo '    <div id="content" role="main">';

$sqlCmd = "SELECT `LocalHandicapDate` FROM `Misc`";
$query = $connection->prepare ( $sqlCmd );

if (! $query) {
	die ( $sqlCmd . " prepare failed: " . $connection->error );
}

if (! $query->execute ()) {
	die ( $sqlCmd . " execute failed: " . $connection->error );
}

$query->bind_result ( $localHandicapDate);

$query->fetch ();

$query->close ();

//-------------------------

$sqlCmd = "SELECT `LocalHandicap`.`GHIN`, `LocalHandicap`.`SCGAHandicap`, `LocalHandicap`.`LocalHandicap`, `LocalHandicap`.`TournamentHandicap`, `Roster`.`LastName`, `Roster`.`FirstName`  FROM `LocalHandicap` INNER JOIN `Roster` ON `LocalHandicap`.`GHIN`=`Roster`.`GHIN` ORDER BY `Roster`.`LastName` ASC";
$query = $connection->prepare ( $sqlCmd );

if (! $query) {
	die ( $sqlCmd . " prepare failed: " . $connection->error );
}

if (! $query->execute ()) {
	die ( $sqlCmd . " execute failed: " . $connection->error );
}

$query->bind_result ( $GHIN, $SCGAHandicap, $LocalHandicap, $TournamentHandicap, $LastName, $FirstName );

$localHandicapEntryArray = array();
while ( $query->fetch () ) {
	$localHandicapEntry = new LocalHandicapEntry();
	$localHandicapEntry->GHIN = $GHIN;
	$localHandicapEntry->SCGAHandicap = $SCGAHandicap;
	$localHandicapEntry->LocalHandicap = $LocalHandicap;
	$localHandicapEntry->LastName = $LastName;
	$localHandicapEntry->FirstName = $FirstName;
	$localHandicapEntry->CalculatedHandicap = $TournamentHandicap;
	
	$localHandicapEntryArray [] = $localHandicapEntry;
}

$query->close ();
if (! $localHandicapEntryArray || (count ( $localHandicapEntryArray ) == 0)) {
	return;
}

echo '<h2 style="text-align:center">Local Handicap for Coronado Men\'s White (' . date ( 'M d, Y', strtotime ( $localHandicapDate ) ) . ')</h2><br>';
echo '<table style="border:none;margin-left:auto;margin-right:auto">' . PHP_EOL;
echo '<thead><tr class="header"><th>Name</th><th>USGA Index</th><th>Local Handicap</th><th>non-WHS Tournament Handicap</th></tr></thead>' . PHP_EOL;
echo '<tbody>' . PHP_EOL;

for($i = 0; $i < count ( $localHandicapEntryArray ); ++ $i) {
	if(($i % 2) == 0){
		echo '<tr class="d1">';
	}
	else {
		echo '<tr class="d0">';
	}
	echo '<td>' . $localHandicapEntryArray[$i]->LastName . ', ' . $localHandicapEntryArray[$i]->FirstName . '</td>';
	echo '<td style="text-align:center">' . $localHandicapEntryArray[$i]->SCGAHandicap . '</td>';
	echo '<td style="text-align:center">' . $localHandicapEntryArray[$i]->LocalHandicap . '</td>';
	echo '<td style="text-align:center">' . $localHandicapEntryArray[$i]->CalculatedHandicap . '</td>';
	echo '</tr>' . PHP_EOL;
}


// Finish the first column table.  Show the 2nd column table.
echo '</tbody>' . PHP_EOL;
echo '</table>' . PHP_EOL;

echo '    </div><!-- #content -->';
echo ' </div><!-- #content-container -->';

$connection->close ();

get_footer ();
?>