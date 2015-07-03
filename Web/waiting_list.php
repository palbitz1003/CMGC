<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$overrideTitle = "Waiting List";
get_header ();

class WaitingListEntry {
	public $Position;
	public $Name;
	public $DateAdded;
}

$connection = new mysqli ('p:' . $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( $connection->connect_error );

echo ' <div id="content-container" class="entry-content">';
echo '    <div id="content" role="main">';

$sqlCmd = "SELECT * FROM `WaitingList` ORDER BY `Position` ASC";
$query = $connection->prepare ( $sqlCmd );

if (! $query) {
	die ( $sqlCmd . " prepare failed: " . $connection->error );
}

if (! $query->execute ()) {
	die ( $sqlCmd . " execute failed: " . $connection->error );
}

$query->bind_result ( $position, $name, $dateAdded );

while ( $query->fetch () ) {
	$waitingListEntry = new WaitingListEntry();
	$waitingListEntry->Position = $position;
	$waitingListEntry->Name = strtoupper($name);  // force upper case, for now
	$waitingListEntry->DateAdded = $dateAdded;
	$waitingListEntriesByPosition [] = $waitingListEntry;
}

$query->close ();
if (! $waitingListEntriesByPosition || (count ( $waitingListEntriesByPosition ) == 0)) {
	return;
}

$sqlCmd = "SELECT * FROM `WaitingList` ORDER BY `Name` ASC";
$query = $connection->prepare ( $sqlCmd );

if (! $query) {
	die ( $sqlCmd . " prepare failed: " . $connection->error );
}

if (! $query->execute ()) {
	die ( $sqlCmd . " execute failed: " . $connection->error );
}

$query->bind_result ( $position, $name, $dateAdded );

while ( $query->fetch () ) {
	$waitingListEntry = new WaitingListEntry();
	$waitingListEntry->Position = $position;
	$waitingListEntry->Name = strtoupper($name);  // force upper case, for now
	$waitingListEntry->DateAdded = $dateAdded;
	$waitingListEntriesByName [] = $waitingListEntry;
}

if (! $waitingListEntriesByName || (count ( $waitingListEntriesByName ) == 0)) {
	$query->close ();
	return;
}


// A single table with 1 row.  The row has 2 data elements, each a table.
echo PHP_EOL;
echo '<table style="border:none;margin-left:auto;margin-right:auto"><tbody>' . PHP_EOL;
echo '<tr>' . PHP_EOL;
echo '<td style="width:50%;border:none;">' . PHP_EOL;
echo '<table>' . PHP_EOL;
echo '<thead><tr class="header"><th>Position  </th><th>Name</th><th>Date Added</th></tr></thead>' . PHP_EOL;
echo '<tbody>' . PHP_EOL;

for($i = 0; $i < count ( $waitingListEntriesByPosition ); ++ $i) {
	if(($i % 2) == 0){
		echo '<tr class="d1">';
	}
	else {
		echo '<tr class=d0>';
	}
	echo '<td>' . $waitingListEntriesByPosition[$i]->Position . '</td>';
	echo '<td>' . $waitingListEntriesByPosition[$i]->Name . '</td>';
	echo '<td>' . date ( 'n/j/Y', strtotime ( $waitingListEntriesByPosition[$i]->DateAdded ) ) . '</td>';
	echo '</tr>' . PHP_EOL;
}


// Finish the first column table.  Show the 2nd column table.
echo '</tbody>' . PHP_EOL;
echo '</table>' . PHP_EOL;
echo '</td>' . PHP_EOL;
echo '<td style="width:50%;border:none;">' . PHP_EOL;

echo '<table>' . PHP_EOL;
echo '<thead><tr class="header"><th>Position  </th><th>Name - Alphabetical</th></tr></thead>' . PHP_EOL;
echo '<tbody>' . PHP_EOL;
for($i = 0; $i < count ( $waitingListEntriesByName ); ++ $i) {
	if(($i % 2) == 0){
		echo '<tr class="d1">';
	}
	else {
		echo '<tr class="d0">';
	}
	echo '<td>' . $waitingListEntriesByName[$i]->Position . '</td>';
	echo '<td>' . $waitingListEntriesByName[$i]->Name . '</td>';
	echo '</tr>' . PHP_EOL;
}
echo '</tbody>' . PHP_EOL;
echo '</table>' . PHP_EOL;

echo '</td></tr></tbody></table>' . PHP_EOL;

echo '    </div><!-- #content -->';
echo ' </div><!-- #content-container -->';

$query->close ();

$connection->close ();

get_footer ();
?>