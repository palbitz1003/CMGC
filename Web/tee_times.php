<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tee times functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/signup functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$overrideTitle = "Tee Times";
get_header ();
get_sidebar ();

$connection = new mysqli ('p:' . $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( $connection->connect_error );

echo ' <div id="content-container" class="entry-content">';
echo '    <div id="content" role="main">';

$tournamentKey = $_GET ['tournament'];
if (! $tournamentKey) {
	die ( "Which tournament?" );
} else {
	$tournament = GetTournament($connection, $tournamentKey);
	$teeTimeArray = GetTeeTimes($connection, $tournamentKey);
	$details = GetTournamentDetails($connection, $tournamentKey);
	$friendlyDate = date ( 'M d', strtotime ( $details->TeeTimesPostedDate ));

	if($tournament->RequirePayment){
		$unpaidSignupArray = GetSignups ( $connection, $tournamentKey, ' AND `Payment` = 0' );
	}
	else {
		$unpaidSignupArray = array();
	}

	/*
	echo "not paid:<br>";
	for($i = 0; $i < count($unpaidSignupArray); ++ $i){
		echo $unpaidSignupArray[$i]->SignUpKey . "<br>";
	}
	*/

	if ($teeTimeArray && (count ( $teeTimeArray ) > 0)) {
		echo '<h2 style="text-align:center">' . $tournament->Name . ' Tee Times' . '</h2>' . PHP_EOL;
		echo '<h4 style="text-align:center">' . "Posted " . $friendlyDate . '</h4>' . PHP_EOL;
		// A single table with 1 row. The row has 2 or 3 data elements, each a table.
		echo PHP_EOL;
		echo '<table style="border:none;margin-left:auto;margin-right:auto">' . PHP_EOL;
		echo '<tr>' . PHP_EOL;
		
		echo '<td style="width:50%;border:none;">' . PHP_EOL;
		ShowTeeTimes ($connection, $tournamentKey, $teeTimeArray, $unpaidSignupArray);
		echo '</td>' . PHP_EOL;
		
		echo '<td style="width:50%;border:none;">' . PHP_EOL;
		ShowPlayersAlphabetically ( $connection, $tournamentKey, $teeTimeArray );
		echo '</td>' . PHP_EOL;
		
		echo '</tr></table>' . PHP_EOL;
		
		ShowWaitingList($connection, $tournamentKey);
	}
}

echo '    </div><!-- #content -->';
echo ' </div><!-- #content-container -->';

function ShowTeeTimes($connection, $tournamentKey, $teeTimeArray, $unpaidSignupArray) {
	echo '<table>' . PHP_EOL;
	echo '<thead><tr class="header"><th colspan="3">By Time</th></tr></thead>' . PHP_EOL;
	echo '<tbody>' . PHP_EOL;

	for($i = 0; $i < count ( $teeTimeArray ); ++ $i) {
		for($j = 0; $j < count($teeTimeArray[$i]->Players); ++$j){
			if ((($i + 1) % 2) == 0) {
				echo '<tr class="d0">';
			} else {
				echo '<tr class="d1">';
			}
			if(count($unpaidSignupArray) > 0){
				$unpaid = false;
				for($k = 0; ($k < count($unpaidSignupArray)) && !$unpaid; ++ $k){
					if($unpaidSignupArray[$k]->SignUpKey === $teeTimeArray[$i]->Players[$j]->SignupKey){
						$unpaid = true;

						// Enable payment if not already enabled
						if(!$unpaidSignupArray[$k]->PaymentEnabled){
							UpdateSignup($connection, $teeTimeArray[$i]->Players[$j]->SignupKey, 'PaymentEnabled', 1, 'i');
						}
					}
				}
				echo '<td>';
				if($unpaid){
					echo '<a href="' . $script_folder_href . 'pay.php?tournament=' . $tournamentKey . '&signup=' . $teeTimeArray[$i]->Players[$j]->SignupKey . '">Pay</a>';
				}
				echo '</td>';
			}
			echo '<td>';
			echo date ( 'g:i', strtotime ( $teeTimeArray [$i]->StartTime ) );
			echo '</td><td>';
			echo ' ' . $teeTimeArray[$i]->Players[$j]->LastName;
			echo '</td></tr>';
			echo PHP_EOL;
		}
	}

	echo '</tbody>' . PHP_EOL;
	echo '</table>' . PHP_EOL;
}

function ShowPlayersAlphabetically($connection, $tournamentKey, $teeTimeArray) {
	$sqlCmd = "SELECT * FROM `TeeTimesPlayers` WHERE `TournamentKey` = ? ORDER BY `Name` ASC";
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

	$query->bind_result ( $key, $tournament, $GHIN, $Name, $Position, $Extra, $SignupKey );

	echo '<table>' . PHP_EOL;
	echo '<thead><tr class="header"><th colspan="2">Alphabetical</th></tr></thead>' . PHP_EOL;
	echo '<tbody>' . PHP_EOL;
	$count = 1;
	while ( $query->fetch () ) {
		if (! empty ( $Name )) {
				
			if (($count % 2) == 0) {
				echo '<tr class="d0"><td>';
			} else {
				echo '<tr class="d1"><td>';
			}
			++ $count;
				
			echo ' ' . $Name;
			echo '</td><td>';
			for($i = 0; $i < count ( $teeTimeArray ); ++ $i) {
				if ($key == $teeTimeArray [$i]->Key) {
					echo date ( 'g:i', strtotime ( $teeTimeArray [$i]->StartTime ) );
					break;
				}
			}
			echo '</td></tr>';
			echo PHP_EOL;
		}
	}
	echo '</tbody>' . PHP_EOL;
	echo '</table>' . PHP_EOL;

	$query->close ();
}
function ShowWaitingList($connection, $tournament){
	
	$waitingList = GetSignUpWaitingList($connection, $tournament);
	
	if(count($waitingList) != 0){
		echo '<p>' . PHP_EOL;
		
		echo '<table style="border:none;margin-left:auto;margin-right:auto">' . PHP_EOL;
		echo '<tr>' . PHP_EOL;
		
		echo '<td style="border:none;width=auto">' . PHP_EOL;
		echo '<table>' . PHP_EOL;
		echo '<thead><tr class="header"><th>Wait Listed</th></tr></thead>' . PHP_EOL;
		echo '<tbody>' . PHP_EOL;
		
		for($i = 0; $i < count ( $waitingList ); ++ $i) {
			if ((($i + 1) % 2) == 0) {
				echo '<tr class="d0"><td>';
			} else {
				echo '<tr class="d1"><td>';
			}
			echo $waitingList[$i]->Name1;
			if(!empty($waitingList[$i]->Name2)){
				echo ', ' . $waitingList[$i]->Name2;
				if(!empty($waitingList[$i]->Name3)){
					echo ', ' . $waitingList[$i]->Name3;
					if(!empty($waitingList[$i]->Name4)){
						echo ', ' . $waitingList[$i]->Name4;
					}
				}
			}
			echo '</td></tr>';
			echo PHP_EOL;
		}
		
		echo '</tbody>' . PHP_EOL;
		echo '</table>' . PHP_EOL;
		echo '</td>' . PHP_EOL;
		
		echo '<td style="border: none;width:300px;">' . PHP_EOL;
		echo 'This tournament is oversubscribed; These players will be placed in the spot of any cancellations in the order listed. Players not getting an assigned time in this tournament will be given priority in the next tournament entered.';
		echo '</td>' . PHP_EOL;
		echo '</tr></table>' . PHP_EOL;
	}
}

$connection->close ();
get_footer ();

?>