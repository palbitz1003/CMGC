<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

get_header ();



$connection = new mysqli ('p:' . $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( $connection->connect_error );

echo ' <div id="content-container" class="entry-content">';
echo '    <div id="content" role="main">';

$currentTournaments = GetCurrentTournaments ( $connection );
if(isset($currentTournaments) && (count($currentTournaments) > 0)){
	$now = new DateTime ( "now" );
	echo '<h2>Current and Upcoming Tournaments:</h2>';
	echo '<table style="border:none;margin-left:30px;">' . PHP_EOL;
	
	for($i = 0; $i < count($currentTournaments); ++$i){
		
		$details = GetTournamentDetails($connection, $currentTournaments[$i]->TournamentKey);
		
		$startSignUp = new DateTime ( $currentTournaments[$i]->SignupStartDate);
		$endSignUp = new DateTime($currentTournaments[$i]->SignupEndDate);
		$endSignUp->add(new DateInterval ( 'PT16H00M' )); // 4pm
		
		//echo 'now ' . $now->format('M d G') . '<br>';
		//echo 'end ' . $endSignUp->format('M d G') . '<br>';
		
		echo '<tr style="font-size:large;">';
		echo '<td style="border:none">' . GetFriendlyTournamentDates($currentTournaments[$i]) . '</td>';
		echo '<td style="border:none">' . $currentTournaments[$i]->Name . '</td>';
		echo '<td style="border:none"><a href="' . $script_folder_href . 'tournament_description.php?tournament='  . $currentTournaments [$i]-> TournamentKey . '">Description</a></td>';
		
		if ($details->TeeTimesPostedDate != TournamentDetails::EMPTYDATE) {
			echo '<td style="border:none"><a href="' . $script_folder_href . 'tee_times.php?tournament=' . $currentTournaments[$i]->TournamentKey . '">Tee Times</a></td>';
		} else {
			echo '<td style="border:none">Tee Times</td>';
		}
		
		
		if($now < $startSignUp){
			if($currentTournaments[$i]->Name == 'Member-Guest'){
				echo '<td style="border:none">Mail-In Signup</td>';
			}
			else {
				echo '<td style="border:none">Sign-up starts ' . date ( 'M d', strtotime ( $currentTournaments [$i]->SignupStartDate ) ) . '</td>';
			}
			echo '<td style="border:none"></td>';
		}
		else if($now <= $endSignUp){
			if($currentTournaments[$i]->Name == 'Member-Guest'){
				echo '<td style="border:none">Mail-In Signup</td>';
				echo '<td style="border:none"></td>';
			}
			else {
				echo '<td style="border:none"><a href="' . $script_folder_href . 'signup.php?tournament=' . $currentTournaments [$i]-> TournamentKey . '">Sign up</a></td>';
				echo '<td style="border:none"><a href="' . $script_folder_href . 'signups.php?tournament=' . $currentTournaments [$i]-> TournamentKey . '">View Signups</a></td>';
			}
		}
		else {
			echo '<td style="border:none">Sign-up closed</td>';
			echo '<td style="border:none"><a href="' . $script_folder_href . 'contact_chairman.php?tournament=' . $currentTournaments [$i]-> TournamentKey . '">Tournament Director</a></td>';
		}
		
		echo '</tr>' . PHP_EOL;
	}
	echo '</table>' . PHP_EOL;
}

ShowRecentlyCompletedTournaments($connection, $script_folder_href);

$connection->close ();

if (have_posts()) {
	while (have_posts()) {
		the_post();
		?>
		<h2 id="post-<?php the_ID(); ?>">
		<?php the_title(); ?></h2>
		<small><?php the_time('F jS, Y') ?> posted by <?php the_author() ?> </small>
		<?php 
		the_content();
	}
}

echo '    </div><!-- #content -->';
echo ' </div><!-- #content-container -->';

get_sidebar ();
get_footer ();
?>