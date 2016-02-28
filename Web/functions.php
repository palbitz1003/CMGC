<?php

function login($login, $password)
{
	wp_clear_auth_cookie();
	
	$creds = array ();
	$creds ['user_login'] = $login;
	$creds ['user_password'] = $password;
	$creds ['remember'] = false;
	$user = wp_signon ( $creds, false );
	if (is_wp_error ( $user )) {
		die ( "Invalid user name or password" );
	}
}

function IsDateSet($date){
	$year = date ( 'Y', strtotime ( $date ) );
	if (strcmp ( $year, '1900' ) == 0) {
		return false;
	}
	return true;
}

function clear_table($connection, $table) {
	$sqlCmd = "DELETE FROM `" . $table . "` ";
	$signups = $connection->prepare ( $sqlCmd );
	
	if (! $signups) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	if (! $signups->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
}

function ClearTableWithTournamentKey($connection, $table,  $tournamentKey) {
	$sqlCmd = "DELETE FROM `" . $table . "` WHERE `TournamentKey` = ?";
	$clear = $connection->prepare ( $sqlCmd );

	if (! $clear) {
		die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	if (! $clear->bind_param ( 'i', $tournamentKey )) {
		die ( $sqlCmd . " bind_param failed: " . $connection->error );
	}

	if (! $clear->execute ()) {
		die ( $sqlCmd . " execute failed: " . $connection->error );
	}
}

// Use this to default a value in a list
function new_list_option($value, $default_value)
{
	echo '<option';
	if($value==$default_value)
	{
		echo ' selected="selected"';
	}
	echo '>' . $value . '</option>' . PHP_EOL;
}

function insert_error_line($errorDescription, $columns) {
	if (isset ( $errorDescription )) {
		echo '<tr>';
		echo '<td style="border:none;color:red;" colspan="' . $columns . '">' . $errorDescription . '</td>';
		echo '</tr>' . PHP_EOL;
	} else {
		echo '<tr style="height:5px;">';
		echo '<td style="border:none; font-size:5px" colspan="' . $columns . '">&nbsp;</td>';
		echo '</tr>' . PHP_EOL;
	}
}

function IsPDF($fname) {
	$fh=fopen($fname,'rb');
	if ($fh) {
		$bytes5=fread($fh,5);
		fclose($fh);
		return $bytes5 === "%PDF-";
	}
	return false;
}
function IsHTML($fname) {
	$fh=fopen($fname,'rb');
	if ($fh) {
		$bytes5=fread($fh,5);
		fclose($fh);
		return (strcasecmp($bytes5,"<html") == 0);
	}
	return false;
}
function IntToBool($value){
	return ($value == 0) ? "false" : "true";
}
function FixNameCasing($name)
{
	if(empty($name)){
		return $name;
	}
	
	$name = stripslashes($name);
	$nameArray = explode(',', $name);
	if(count($nameArray) == 1){
		return ucfirst(strtolower(trim($nameArray[0])));
	}
	
	$lastName = ucfirst(strtolower(trim($nameArray[0])));
	if(strpos($lastName, ' ') !== FALSE){
		$lastName = FixCasingWithinName($lastName, ' ');
	}
	if (strpos($lastName, "'") !== FALSE){
		// Upper case first letter after apostrophe
		$lastName = FixCasingWithinName($lastName, "'");
	}
	
	$firstName = ucfirst(strtolower(trim($nameArray[1])));
	if(strpos($firstName, ' ') !== FALSE){
		$firstName = FixCasingWithinName($firstName, ' ');
	}
	if (strpos($firstName, '(') !== FALSE){
		// change (ty) back to (Ty)
		$firstName = FixCasingWithinName($firstName, '(');
	}
	
	return $lastName . ', ' . $firstName;
}

function FixCasingWithinName($name, $separator)
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
?>