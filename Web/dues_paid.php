<?php
require_once realpath ( $_SERVER ["DOCUMENT_ROOT"] ) . '/login.php';
require_once realpath ( $_SERVER ["DOCUMENT_ROOT"] ) . $script_folder . '/dues_functions.php';
require_once realpath ( $_SERVER ["DOCUMENT_ROOT"] ) . $wp_folder . '/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$overrideTitle = "Paid Yearly Dues";
get_header ();

get_sidebar ();

$connection = new mysqli ( 'p:' . $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( $connection->connect_error );

echo ' <div id="content-container" class="entry-content">';
echo '    <div id="content" role="main">';

echo '<h2 class="entry-title" style="text-align:center">Paid Yearly Dues</h2>' . PHP_EOL;

echo '<p>Once you pay via PayPal you will automatically be listed on the paid list. ';
echo 'Paying by check is not an option anymore. You must pay through PayPal.';

$players = GetPlayersDuesPaid($connection);

echo '<p>Players that have paid: ' . count($players);
echo '<table style="border: none;margin-left:auto;margin-right:auto;width: 96%">' . PHP_EOL;
echo '<tbody><tr>' . PHP_EOL;

$players = GetPlayersDuesPaid($connection);

// Determine the players per table
$playersPerTable = (int)(count($players) / 4);

// Create an array of counts per table
$tableCounts = array();
for($table = 0; $table < 4; ++$table){
	$tableCounts[] = $playersPerTable;
}

// distribute the extra players to each table
$extra = count($players) - (4 * $playersPerTable);
for($table = 0; $table < 4; ++$table){
	if($extra > 0)
	{
		++$tableCounts[$table];
		--$extra;
	}
}

$currentTable = 0;
$playersInTable = 0;
for($i = 0; $i < count($players); ++$i)
{
	if($i == 0){
		ShowTableHeader();
	}
	if($playersInTable == $tableCounts[$currentTable])
	{
		echo '</tbody></table></td>' . PHP_EOL;
		if($currentTable < 3){
			ShowTableHeader();
		}
		++ $currentTable;
		$playersInTable = 0;
	}
	
	ShowPlayer($i, $players [$i]->Name);
	++$playersInTable;
}

if($playersInTable != 0){
	echo '</tbody></table></td>' . PHP_EOL;
}
echo '</tr></tbody></table>' . PHP_EOL;

echo '    </div><!-- #content -->';
echo ' </div><!-- #content-container -->';

function ShowTableHeader()
{
	echo '<td style="width:25%;border:none;"><table style="width:100%;">' . PHP_EOL;
	echo '<thead><tr class="header"><th>Name</th></tr></thead>' . PHP_EOL;
	echo '<tbody>' . PHP_EOL;
}

function ShowPlayer($i, $name){
	if (($i % 2) == 0) {
		echo '<tr class="d1">';
	} else {
		echo '<tr class="d0">';
	}
	echo '<td>' . $name . '</td>';
		
	echo '</tr>' . PHP_EOL;
}

$connection->close ();
get_footer ();
?>