<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/signup functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

$overrideTitle = "Replace Players";
get_header ();

get_sidebar ();

$tournamentKey = $_GET ['tournament'];
if (! $tournamentKey) {
	die ( "Which tournament?" );
}
$signupKey = $_GET['signup'];
if(! $signupKey) {
	die ( "Which signup?" );
}

//$everything = get_defined_vars();
//ksort($everything);
//echo '<pre>';
//print_r($everything);
//echo '</pre>';

//var_dump($_POST);

$connection = new mysqli ( $db_hostname, $db_username, $db_password, $db_database );
if ($connection->connect_error)
	die ( $connection->connect_error );

$t = GetTournament($connection, $tournamentKey);
$players = GetPlayersForSignUp($connection, $signupKey);

if(count($players) == 0){
	die ("There are no players for signup code " . $_GET ['signup']);
}

$errorList = array ();
$GHIN = array ();
$LastName = array ();
$Extra = array();
$invalidAccessCodeError = null;
$hasError = false;
$emptyForm = true;

if (isset ( $_POST ['Player'] )) {
	
	for($i = 0; $i < count($_POST ['Player']); ++ $i) {
		$GHIN [$i] = trim ( $_POST ['Player'] [$i] ['GHIN'] );
		$LastName [$i] = trim ( $_POST ['Player'] [$i] ['LastName'] );
		$Extra [$i] = trim ( $_POST ['Player'] [$i] ['Extra'] );
		
		$LastName[$i] = stripslashes ( $LastName[$i] ); // remove any slashes before quotes
		$LastName[$i] = str_replace("'", "", $LastName[$i]); // remove single quotes
		if(!empty($GHIN[$i]) || !empty($LastName[$i])){
			$emptyForm = false;
		}
	}
}

if(!$emptyForm){
	
	$accessCode = trim($_POST['AccessCode']);
	
	if(empty($accessCode)){
		$invalidAccessCodeError = "Fill in the Access Code";
	}
	else {
		
		$signup = GetSignup($connection, $signupKey);
		if(empty($signup)){
			die("There is no data for signup key: " . $signupKey);
		}
		
		if($signup->AccessCode != $accessCode){
			$invalidAccessCodeError = "Invalid access code";
		}
		else {

			for($i = 0; $i < count($_POST ['Player']); ++ $i) {
				
				// If you put in a GHIN of 0, empty("0") returns true, so change
				// the GHIN number to "0000000"
				if (isset($GHIN [$i]) && $GHIN [$i] === '0'){
					$GHIN [$i] = "0000000";
				}
				
				// Check if the player is already in the group
				$playerIsInGroup = false;
				if (! empty ( $GHIN [$i] ) && ! empty ( $LastName [$i] )) {
					for($playerIndex = 0; $playerIndex < count($players); ++ $playerIndex){
						if($players[$playerIndex]->GHIN == $GHIN[$i]){
							$playerIsInGroup = true;
						}
					}
				}
				
				// These are the same checks used during signup
				if($t->MemberGuest && $players[$i]->Extra == "G" && (!empty ( $GHIN [$i] ) || !empty ( $LastName [$i]))){
					if (strpos($LastName [$i], ',') !== FALSE){
						// No checks for name matching GHIN or if player is already signed up
						// Just save the the full name.
						$FullName[$i] = $LastName [$i];
							
						if(empty ( $GHIN [$i] ))
						{
							$errorList [$i] = 'Fill in GHIN for guest';
						} else if($GHIN [$i] !== "0000000"){
							$rosterEntry = GetRosterEntry ( $connection, $GHIN [$i] );
							if (!empty ( $rosterEntry )) {
								$errorList [$i] = 'GHIN ' . $GHIN [$i] . " is a member of the Coronado Men's Golf Club<br>The guest cannot be a member.";
							}
						}
					} else {
						$errorList [$i] = 'Please fill in "last name, first name" for guests';
					}
				} else if ($t->AllowNonMemberSignup && ! empty ( $GHIN [$i] ) && !empty ( $LastName [$i]) && ($GHIN [$i] === "0000000")) {
					if (strpos($LastName [$i], ',') !== FALSE){
						// No checks for name matching GHIN or if player is already signed up
						$FullName[$i] = $LastName [$i];
					} else {
						$errorList [$i] = 'Please fill in "last name, first name" when using GHIN 0';
					}
				} else if (! empty ( $GHIN [$i] ) && empty ( $LastName [$i] )) {
					$errorList [$i] = 'Player ' . ($i + 1) . ' Last Name must be filled in';
				} else if (empty ( $GHIN [$i] ) && ! empty ( $LastName [$i] )) {
					$errorList [$i] = 'Player ' . ($i + 1) . ' GHIN must be filled in';
				} else if (! empty ( $GHIN [$i] ) && ! empty ( $LastName [$i] )) {
					// Check for player already signed up
					if (!$playerIsInGroup && IsPlayerSignedUp ( $connection, $tournamentKey, $GHIN [$i] )) {
						$errorList [$i] = 'Player ' . $GHIN [$i] . ' is already signed up';		
					} else {
						// Check that last name matches GHIN database
						$rosterEntry = GetRosterEntry ( $connection, $GHIN [$i] );
						// $errorList[$i] = 'Last name is ' . $lastName;
						if (empty ( $rosterEntry )) {
							$errorList [$i] = 'GHIN ' . $GHIN [$i] . " is not a member of the Coronado Men's Golf Club";
						} else if(!$rosterEntry->Active) {
							$errorList [$i] = 'GHIN ' . $GHIN [$i] . " is not an active member of the Coronado Men's Golf Club";
						} else {
							if (strpos($rosterEntry->LastName, ' ') !== FALSE){
								// Only compare the part before the space
								$nameArray1 = explode(' ', $rosterEntry->LastName);
								$nameArray2 = explode(' ', $LastName [$i]);
								$lastNamesMatch = strcasecmp ( $nameArray1[0], $nameArray2[0] ) == 0;
							} else {
								$lastNamesMatch = strcasecmp ( $LastName [$i], $rosterEntry->LastName ) == 0;
							}
							
							if (!$lastNamesMatch) {
								$errorList [$i] = 'Last name for GHIN ' . $GHIN [$i] . ' is not ' . $LastName [$i];
							} else {
								// Use the database casing for the last name
								$LastName [$i] = $rosterEntry->LastName;
								$FullName[$i] = $rosterEntry->LastName . ', ' . $rosterEntry->FirstName;
							}
						}
					}
				}
			}
			
			for($i = 0; $i < count($_POST ['Player']); ++ $i) {
				if (! empty ( $errorList [$i] )) {
					$hasError = true;
				}
			}
		}
	}
}

echo '<div id="content-container" class="entry-content">' . PHP_EOL;
echo '<div id="content" role="main">' . PHP_EOL;
echo '<h2 class="entry-title" style="text-align:center">Replace Players</h2>' . PHP_EOL;

if ($hasError || !empty($invalidAccessCodeError) || $emptyForm ) {
	echo '<p>Use this form to replace players in your group. Fill in the GHIN and last name of the new players for your group.</p>';
	
	echo '<form name="input" method="post">' . PHP_EOL;

	echo '<table style="border: none;">' . PHP_EOL;
	
	echo '	<tr>' . PHP_EOL;
	echo '		<td style="border: none;">Access Code:</td>' . PHP_EOL;
	echo '		<td style="border: none;"><input type="text" name="AccessCode" maxlength="4" size="4"' . PHP_EOL;
	echo '			value="' . $_POST['AccessCode'] . '"></td>' . PHP_EOL;
	echo '		<td style="border: none;"></td>' . PHP_EOL;
	echo '	</tr>' . PHP_EOL;
	insert_error_line($invalidAccessCodeError, 3);

	echo '	<tr>' . PHP_EOL;
	if($t->MemberGuest){
		echo '		<td style="border: none;">Member 1 GHIN:</td>' . PHP_EOL;
	} else {
		echo '		<td style="border: none;">Player 1 GHIN:</td>' . PHP_EOL;
	}
	
	echo '		<td style="border: none;"><input type="text" name="Player[0][GHIN]"' . PHP_EOL;
	echo '			value="' . $GHIN[0] . '"></td>' . PHP_EOL;
	echo '		<td style="border: none;"></td>' . PHP_EOL;
	echo '	</tr>' . PHP_EOL;
	echo '	<tr>' . PHP_EOL;
	if($t->MemberGuest){
		echo '		<td style="border: none;">Member 1 Last Name:</td>' . PHP_EOL;
	} else {
		echo '		<td style="border: none;">Player 1 Last Name:</td>' . PHP_EOL;
	}
	
	echo '		<td style="border: none;"><input type="text"' . PHP_EOL;
	echo '			name="Player[0][LastName]" value="' . $LastName[0] . '"></td>' . PHP_EOL;
	echo '		<td style="border: none;">Replaces ' . $players[0]->LastName . '</td>' . PHP_EOL;
	echo '	</tr>' . PHP_EOL;
	insert_error_line($errorList[0], 3);
	
	if(count($players) > 1){
		echo '	<tr>' . PHP_EOL;
		if($t->MemberGuest){
			echo '		<td style="border: none;">Guest 2 GHIN:</td>' . PHP_EOL;
		} else {
			echo '		<td style="border: none;">Player 2 GHIN:</td>' . PHP_EOL;
		}
		
		echo '		<td style="border: none;"><input type="text" name="Player[1][GHIN]"' . PHP_EOL;
		echo '			value="' . $GHIN[1] . '"></td>' . PHP_EOL;
		echo '		<td style="border: none;"></td>' . PHP_EOL;
		echo '	</tr>' . PHP_EOL;
		echo '	<tr>' . PHP_EOL;
		if($t->MemberGuest){
			echo '		<td style="border: none;">Guest 2 Name (Last, First):</td>' . PHP_EOL;
		} else {
			echo '		<td style="border: none;">Player 2 Name:</td>' . PHP_EOL;
		}
		
		echo '		<td style="border: none;"><input type="text"' . PHP_EOL;
		echo '			name="Player[1][LastName]" value="' . $LastName[1] . '"></td>' . PHP_EOL;
		echo '		<td style="border: none;">Replaces ' . $players[1]->LastName . '</td>' . PHP_EOL;
		echo '	</tr>' . PHP_EOL;
		insert_error_line($errorList[1], 3); 
		}
	if(count($players) > 2){
		echo '	<tr>' . PHP_EOL;
		if($t->MemberGuest){
			echo '		<td style="border: none;">Member 3 GHIN:</td>' . PHP_EOL;
		}else {
			echo '		<td style="border: none;">Player 3 GHIN:</td>' . PHP_EOL;
		}
		
		echo '		<td style="border: none;"><input type="text" name="Player[2][GHIN]"' . PHP_EOL;
		echo '			value="' . $GHIN[2] . '"></td>' . PHP_EOL;
		echo '		<td style="border: none;"></td>' . PHP_EOL;
		echo '	</tr>' . PHP_EOL;
		echo '	<tr>' . PHP_EOL;
		if($t->MemberGuest){
			echo '		<td style="border: none;">Member 3 Last Name:</td>' . PHP_EOL;
		} else {
			echo '		<td style="border: none;">Player 3 Last Name:</td>' . PHP_EOL;
		}
		
		echo '		<td style="border: none;"><input type="text"' . PHP_EOL;
		echo '			name="Player[2][LastName]" value="' . $LastName[2]. '"></td>' . PHP_EOL;
		echo '		<td style="border: none;">Replaces ' . $players[2]->LastName . '</td>' . PHP_EOL;
		echo '	</tr>' . PHP_EOL;
		insert_error_line($errorList[2], 3);
	}
	if(count($players) > 3){
		echo '	<tr>' . PHP_EOL;
		if($t->MemberGuest){
			echo '		<td style="border: none;">Guest 4 GHIN:</td>' . PHP_EOL;
		} else {
			echo '		<td style="border: none;">Player 4 GHIN:</td>' . PHP_EOL;
		}
		
		echo '		<td style="border: none;"><input type="text" name="Player[3][GHIN]"' . PHP_EOL;
		echo '			value="' . $GHIN[3] . '"></td>' . PHP_EOL;
		echo '		<td style="border: none;"></td>' . PHP_EOL;
		echo '	</tr>' . PHP_EOL;
		echo '	<tr>' . PHP_EOL;
		if($t->MemberGuest){
			echo '		<td style="border: none;">Guest 4 Name (Last, First):</td>' . PHP_EOL;
		} else {
			echo '		<td style="border: none;">Player 4 Last Name:</td>' . PHP_EOL;
		}
		
		echo '		<td style="border: none;"><input type="text"' . PHP_EOL;
		echo '			name="Player[3][LastName]" value="' . $LastName[3] . '"></td>' . PHP_EOL;
		echo '		<td style="border: none;">Replaces ' . $players[3]->LastName . '</td>' . PHP_EOL;
		echo '	</tr>' . PHP_EOL;
		insert_error_line($errorList[3], 3);
	}
	echo '</table>' . PHP_EOL;
	
	echo '<input type="submit" value="Replace Players"> <br> <br>' . PHP_EOL;
	echo '</form>' . PHP_EOL;
}
else {
	
	// Remove all the players in the group
	for($i = 0; $i < count($players); ++$i){
		RemoveSignedUpPlayer ( $connection, $tournamentKey, $players[$i]->GHIN, $players[$i]->LastName );
	}
	
	// Fill in the player fields that were left empty
	for($i = 0; $i < count($players); ++$i){
		if(empty($GHIN[$i])){
			$GHIN[$i] = $players[$i]->GHIN;
			$FullName[$i] = $players[$i]->LastName; // last name field filled in with full name
			$Extra[$i] = $players[$i]->Extra;
		}
		else if(!$t->SrClubChampionship){
			// carry forward flight info from old player, but not for the senior championship
			// since that flight data is based on age
			$Extra[$i] = $players[$i]->Extra;  
		}
	}
	
	// Now add the players to the signup entry
	InsertSignUpPlayers ( $connection, $tournamentKey, $signupKey, $GHIN, $FullName, $Extra );
	
	// Get the updated list of players for verification
	$players = GetPlayersForSignUp($connection, $signupKey);
	
	echo '<p>Your group has been changed to the following players:<br>' . PHP_EOL;
	for($i = 0; $i < count($players); ++$i){
		echo '&nbsp;&nbsp;&nbsp;' . $players[$i]->LastName . '<br>' . PHP_EOL;
	}
	echo '</p>' . PHP_EOL;
	
	if($t->SrClubChampionship){
		echo '<p style="color: red">Click this link to select the flight for the new player: ' . PHP_EOL;
		echo '<a href="signup_modify.php?tournament=' . $tournamentKey . '&amp;signup=' . $signupKey . '">Modify Flight</a>' . PHP_EOL;
		echo '</p>' . PHP_EOL;
	}
	
	echo '<p><a href="' . 'signups.php?tournament=' . $tournamentKey . '">View Signups</a></p>' . PHP_EOL;
}
echo '</div><!-- #content -->' . PHP_EOL;
echo '</div><!-- #content-container -->' . PHP_EOL;

if (isset ( $connection )) {
	$connection->close ();
}
get_footer ();
?>