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
if (! $tournamentKey || !is_numeric($tournamentKey)) {
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
		echo '<tr><td colspan="2" style="border:none;width:75px">';
		echo 'Email the <a href="mailto:' . $tournament->ChairmanEmail . '">tournament director</a> if you want to cancel. If flight information is shown after your name, ';
		echo 'see the <a href="tournament_description.php?tournament='  . $tournament-> TournamentKey . '">tournament description</a> for an explanation.'  . PHP_EOL;
		echo '</td></tr>' . PHP_EOL;
		echo '<tr>' . PHP_EOL;
		
		echo '<td style="width:50%;border:none;">' . PHP_EOL;
		ShowTeeTimes ($connection, $tournamentKey, $teeTimeArray, $unpaidSignupArray);
		echo '</td>' . PHP_EOL;
		
		echo '<td style="width:50%;border:none;">' . PHP_EOL;
		ShowPlayersAlphabetically ( $connection, $tournamentKey, $teeTimeArray );
		echo '</td>' . PHP_EOL;
		
		echo '</tr></table>' . PHP_EOL;
		
		ShowWaitingList($connection, $tournamentKey);

		ShowTeeTimesCancelledList($connection, $tournamentKey);
	}
}

echo '    </div><!-- #content -->';
echo ' </div><!-- #content-container -->';

function ShowTeeTimes($connection, $tournamentKey, $teeTimeArray, $unpaidSignupArray) {
	echo '<table>' . PHP_EOL;
	echo '<thead><tr class="header"><th colspan="3">By Time</th></tr></thead>' . PHP_EOL;
	echo '<tbody>' . PHP_EOL;

	$paymentSignupKeyShown = array();
	for($i = 0; $i < count ( $teeTimeArray ); ++ $i) {
		for($j = 0; $j < count($teeTimeArray[$i]->Players); ++$j){
			if ((($i + 1) % 2) == 0) {
				echo '<tr class="d0">';
			} else {
				echo '<tr class="d1">';
			}
			if(!empty($unpaidSignupArray) && (count($unpaidSignupArray) > 0)){
				$unpaid = false;
				$signupKey = $teeTimeArray[$i]->Players[$j]->SignupKey;

				for($k = 0; ($k < count($unpaidSignupArray)) && !$unpaid; ++ $k){
					if($unpaidSignupArray[$k]->SignUpKey === $signupKey){
						$unpaid = true;

						// Enable payment if not already enabled
						if(!$unpaidSignupArray[$k]->PaymentEnabled){
							UpdateSignup($connection, $signupKey, 'PaymentEnabled', 1, 'i');
						}
					}
				}
				echo '<td>';
				if($unpaid){
					// Only show the payment link once for each signup key
					if(array_search($signupKey, $paymentSignupKeyShown, true) === false){
						$dbSignups = GetPlayersForSignUp($connection, $signupKey);
						$link = "Pay for Signup Group";
						if(!empty($dbSignups) && count ($dbSignups) == 1)
						{
							$link = "Pay for Single";
						}

						echo '<a href="' . $script_folder_href . 'pay.php?tournament=' . $tournamentKey . '&signup=' . $signupKey . '">' . $link . '</a>';
						// Add to list of signup keys shown for payment
						$paymentSignupKeyShown[] = $signupKey;
					}
				}
				echo '</td>';
			}
			echo '<td>';
			echo date ( 'g:i', strtotime ( $teeTimeArray [$i]->StartTime ) );
			echo '</td><td>';
			echo ' ' . $teeTimeArray[$i]->Players[$j]->LastName;
			
			// Show extra data
			if(!empty($teeTimeArray[$i]->Players[$j]->Extra)){
				echo ' (' . $teeTimeArray[$i]->Players[$j]->Extra . ')';
			}
			
			echo '</td></tr>';
			echo PHP_EOL;
		}
		// Show empty slots if tee time is not full
		// Skip over completely empty tee times
		if(count($teeTimeArray[$i]->Players) != 0){
			for($j = count($teeTimeArray[$i]->Players); $j < 4; ++$j){
				if ((($i + 1) % 2) == 0) {
					echo '<tr class="d0">';
				} else {
					echo '<tr class="d1">';
				}
				if(!empty($unpaidSignupArray) && (count($unpaidSignupArray) > 0)){
					echo '<td></td>';
				}
				echo '<td>';
				echo date ( 'g:i', strtotime ( $teeTimeArray [$i]->StartTime ) );
				echo '</td><td>';
				// No name to display
				echo '</td></tr>';
				echo PHP_EOL;
			}
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
function ShowWaitingList($connection, $tournamentKey){
	
	$waitingList = GetSignUpWaitingList($connection, $tournamentKey);
	
	if(count($waitingList) != 0){
		// Sigh. I couldn't get a paragraph to center properly without putting it in a table
		echo '<table style="width:500px;border:none;margin-left:auto;margin-right:auto">' . PHP_EOL;
		echo '<tbody>' . PHP_EOL;
		echo '<tr><td style="border:none">' . PHP_EOL;
		echo 'This tournament is oversubscribed; These players will be placed in the spot of any cancellations in the order listed. ' . PHP_EOL;
		echo 'Players not getting an assigned time in this tournament will be given priority in the next tournament entered.' . PHP_EOL;
		echo '</td></tr>' . PHP_EOL;
		echo '</tbody>' . PHP_EOL;
		echo '</table>' . PHP_EOL;

		ShowWaitingListTable($waitingList);
	}
}

function ShowTeeTimesCancelledList($connection, $tournamentKey){
	
	$teeTimesCancelledList = GetTeeTimesCancelledList($connection, $tournamentKey);
	
	if(count($teeTimesCancelledList) != 0){

		echo '<table style="min-width:500px;margin-left:auto;margin-right:auto">' . PHP_EOL;
		echo '<thead><tr class="header"><th  colspan="4">Cancellations</th></tr></thead>' . PHP_EOL;
		echo '<tbody>' . PHP_EOL;
		
		for($i = 0; $i < count ( $teeTimesCancelledList );) {
			echo '<tr>' . PHP_EOL;
			$cols = 0;
			for(; ($cols < 4) && ($i < count($teeTimesCancelledList)); ++$cols, ++$i){
				if($cols == 0){
					echo '<td style="width:25%;">' . $teeTimesCancelledList[$i]->Name . '</td>' . PHP_EOL;
				}
				else {
					// would be better for a style to provide the border ...
					echo '<td style="border-left: 1px solid #ccc;width:25%;">' . $teeTimesCancelledList[$i]->Name . '</td>' . PHP_EOL;
				}
			}
			// Finish the column data to add in all the border lines
			for(;$cols < 4; ++$cols){
				echo '<td style="border-left: 1px solid #ccc;width:25%;"></td>' . PHP_EOL;
			}
			echo '</tr>' . PHP_EOL;
		}
		echo '</tbody>' . PHP_EOL;
		echo '</table>' . PHP_EOL;

	}
}

$connection->close ();
get_footer ();

?>