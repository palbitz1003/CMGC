<?php
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/tournament_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/results_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/dues_functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $wp_folder .'/wp-blog-header.php';
date_default_timezone_set ( 'America/Los_Angeles' );

get_header ();



$connection = new mysqli ('p:' . $db_hostname, $db_username, $db_password, $db_database );

if ($connection->connect_error)
	die ( $connection->connect_error );

echo ' <div id="content-container" class="entry-content">';
echo '    <div id="content" role="main">';

ShowDues($connection, $script_folder_href);

$currentTournaments = GetCurrentTournaments ( $connection );
if(isset($currentTournaments) && (count($currentTournaments) > 0)){
	$now = new DateTime ( "now" );
	echo '<h2>Current and Upcoming Tournaments:</h2>';
	echo '<table style="border:none;margin-left:30px;">' . PHP_EOL;
	
	for($i = 0; $i < count($currentTournaments); ++$i){
		
		$details = GetTournamentDetails($connection, $currentTournaments[$i]->TournamentKey);
		
		$startSignUp = new DateTime ( $currentTournaments[$i]->SignupStartDate);
		$startSignUp->add(new DateInterval ( $signup_start_time ));
		$endSignUp = new DateTime($currentTournaments[$i]->SignupEndDate);
		$endSignUp->add(new DateInterval ( $signup_end_time )); 
		$endSignUpFriendlyDate = GetUnbreakableHtmlDateString(date ( 'M d', strtotime ( $currentTournaments[$i]->SignupEndDate )));
		
		//echo 'now ' . $now->format('M d G') . '<br>';
		//echo 'end ' . $endSignUp->format('M d G') . '<br>';
		
		echo '<tr style="font-size:large;">' . PHP_EOL;
		echo '<td style="border:none">' . GetFriendlyTournamentDates($currentTournaments[$i]) . '</td>'. PHP_EOL;
		if($currentTournaments[$i]->AnnouncementOnly){
				echo '<td style="border:none;text-align:center" colspan="4"> ------ ' . $currentTournaments[$i]->Name . ' ------ </td>'. PHP_EOL;
		}
		else {
			echo '<td style="border:none; width:25%">' . GetUnbreakableDash($currentTournaments[$i]->Name) . '</td>'. PHP_EOL;
			echo '<td style="border:none"><a href="' . $script_folder_href . 'tournament_description.php?tournament='  . $currentTournaments [$i]-> TournamentKey . '">Description</a></td>'. PHP_EOL;
			
			if($now < $startSignUp){
				echo '<td style="border:none">Sign-up starts ' . 
							GetUnbreakableHtmlDateString(date ( 'M d (ga)', date_timestamp_get($startSignUp) )) . '</td>'. PHP_EOL;
				echo '<td style="border:none"></td>'. PHP_EOL;
			}
			else if($now <= $endSignUp){
				$maxSignups = "";
				/*
				if($currentTournaments [$i]-> MaxSignups != 0){
					// &nbsp; is a non-breakable space
					$maxSignups = ', max&nbsp;signup&nbsp;' . $currentTournaments [$i]-> MaxSignups;
				}
				*/
				
				echo '<td style="border:none"><a href="' . $script_folder_href . 'signup.php?tournament=' . $currentTournaments [$i]-> TournamentKey . '">Sign up</a> (<span style="font-size:small;">ends&nbsp;' . GetUnbreakableHtmlDateString($endSignUpFriendlyDate) . $maxSignups . '</span>)</td>'. PHP_EOL;
				echo '<td style="border:none"><a href="' . $script_folder_href . 'signups.php?tournament=' . $currentTournaments [$i]-> TournamentKey . '">View Signups</a></td>'. PHP_EOL;
			}
			else if ($details->TeeTimesPostedDate != TournamentDetails::EMPTYDATE) {
				$friendlyDate = GetUnbreakableHtmlDateString(date ( 'M d', strtotime ( $details->TeeTimesPostedDate )));
				echo '<td style="border:none"><a href="' . $script_folder_href . 'tee_times.php?tournament=' . $currentTournaments[$i]->TournamentKey . '">Tee Times</a> (<span style="font-size:small;">posted ' . $friendlyDate . '</span>)</td>'. PHP_EOL;
				echo '<td style="border:none"><a href="' . $script_folder_href . 'contact_chairman.php?tournament=' . $currentTournaments [$i]-> TournamentKey . '">Tournament Director</a></td>'. PHP_EOL;
			} else {
				echo '<td style="border:none">Tee Times (<span style="font-size:small;">pending</span>)</td>'. PHP_EOL;
				echo '<td style="border:none"><a href="' . $script_folder_href . 'contact_chairman.php?tournament=' . $currentTournaments [$i]-> TournamentKey . '">Tournament Director</a></td>'. PHP_EOL;
			}
			
			echo '</tr>' . PHP_EOL;
			
			if($currentTournaments[$i]->MatchPlay == 1){
				echo '<tr><td style="border:none" colspan="6">' . PHP_EOL;
				ShowMatchResults($connection, $currentTournaments[$i]->TournamentKey);
				echo '</td></tr>' . PHP_EOL;
			}
		}
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

function ShowDues($connection, $script_folder_href){

	$now = new DateTime ( "now" );
	$startDues = GetDuesStartDate();
	$endExtendedDues = GetDuesEndExtendedDate();
	
	if(($now >= $startDues) && ($now < $endExtendedDues))
	{
		$dues = GetPayPalDuesDetails($connection, 'R');
		$extendedDues = GetPayPalDuesDetails($connection, 'R_Late');
		$scgaOnly = GetPayPalDuesDetails($connection, 'L');
		
		echo '<h2>Yearly Dues Payment</h2>';
		echo '<p style="margin-left:30px;">The dues for regular members is $' . $dues->TournamentFee . ' before Oct 1. From Oct 1 through Oct 31, the dues are $' . $extendedDues->TournamentFee .'. Life members pay the annual SCGA fee of $' . $scgaOnly->TournamentFee .'. Social members should send a check to the CMGC office. After Oct 31, you will be dropped from membership automatically. ';
		echo '<p style="margin-left:30px;font-size:large;">' . PHP_EOL;
		echo '<a href="' . $script_folder_href . 'dues_payment.php">Pay Dues</a>&nbsp;&nbsp;&nbsp;&nbsp;'. PHP_EOL;
		echo '<a href="' . $script_folder_href . 'dues_not_paid.php">View Have Not Paid List</a>'. PHP_EOL;
		echo '</p>' . PHP_EOL;
	}

}

get_sidebar ();
get_footer ();
?>