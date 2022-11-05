<?php

function cmgc_admin_clear_table($connection, $table) {
	$sqlCmd = "DELETE FROM `" . $table . "` ";
	$signups = $connection->prepare ( $sqlCmd );
	
	if (! $signups) {
		wp_die ( $sqlCmd . " prepare failed: " . $connection->error );
	}
	
	if (! $signups->execute ()) {
		wp_die ( $sqlCmd . " execute failed: " . $connection->error );
	}
}

function cmgc_admin_FixNameCasing($name)
{
	if(empty($name)){
		return $name;
	}
	
	$name = stripslashes($name);

	// If the name already has both upper and lower case characters,
	// just use the name as-is.
	if(strtolower($name) != $name && strtoupper($name) != $name){
		return(trim($name));
	}

	$nameArray = explode(',', $name);
	if(count($nameArray) == 1){
		return ucfirst(strtolower(trim($nameArray[0])));
	}
	
	$lastName = ucfirst(strtolower(trim($nameArray[0])));
	if(strpos($lastName, ' ') !== FALSE){
		$lastName = cmgc_admin_FixCasingWithinName($lastName, ' ');
	}
	if (strpos($lastName, "'") !== FALSE){
		// Upper case first letter after apostrophe
		$lastName = cmgc_admin_FixCasingWithinName($lastName, "'");
	}
	// Fix last names that start with Mc
	if(0 === strpos($lastName, "Mc")){
		$lastName = substr($lastName, strlen("Mc"));
		// Upper case character after Mc and add Mc back in
		$lastName = "Mc" . ucfirst($lastName);
	}
	
	$firstName = ucfirst(strtolower(trim($nameArray[1])));
	if(strpos($firstName, ' ') !== FALSE){
		$firstName = cmgc_admin_FixCasingWithinName($firstName, ' ');
	}
	if (strpos($firstName, '(') !== FALSE){
		// change (ty) back to (Ty)
		$firstName = cmgc_admin_FixCasingWithinName($firstName, '(');
	}
	
	return $lastName . ', ' . $firstName;
}

function cmgc_admin_FixCasingWithinName($name, $separator)
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