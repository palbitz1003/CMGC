<?php
use PHPMailer\PHPMailer\PHPMailer;
use PHPMailer\PHPMailer\Exception;
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . '/login.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/functions.php';
require_once realpath($_SERVER["DOCUMENT_ROOT"]) . $script_folder . '/signup functions.php';

$m = 'test';
$m .= "\n\nDo not reply to this email.  Contact the tournament director if you have any questions.\n";

SendEmail($doNotReplyEmailAddress, $doNotReplyEmailPassword, "palbitz@san.rr.com, paul.m.albitz@outlook.com", 'Coronado Mens Golf Tournament Signup', $m);
?>
