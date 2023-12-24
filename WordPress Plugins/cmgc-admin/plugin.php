<?php
/**
 * Plugin Name: CMGC Admin
 * Description: Manage CMGC Website
 * Version: 1.0.0
 */

 // Documentation: 
 //    https://developer.wordpress.org/plugins/

 // When this plugin is activated, call cmgc_admin_plugin_create_options.
 // This plugin uses the activation to create variables used for communication in the WP options table
 register_activation_hook(__FILE__, 'cmgc_admin_plugin_create_options');

 // See https://blog.idrsolutions.com/wordpress-plugin-part-1/
 //     https://developer.wordpress.org/plugins/administration-menus/top-level-menus/
 add_action( 'admin_menu', 'cmgc_admin_create_menu');

 // Create the menu on the WP admin page.
 // The first option ‘CMGC Admin’ is the title of our options page
 // The second parameter ‘CMGC Admin’ is the label for our admin panel
 // The third parameter determines which users can see the option by limiting access to certain users with certain capabilities
 // parameter 3: replaced "manage_options" with "edit_pages" so users with editor permissions can see them
 // ‘cmgc-admin-menu’ is the slug which is used to identify the menu
 // The final parameter ‘cmgc_admin_show_menu’ is the name of the function we want to call when the option is selected
 function cmgc_admin_create_menu() {
    add_menu_page('CMGC Admin', "CMGC Admin", 
        'edit_pages', 'cmgc-admin-menu', 'cmgc_admin_show_menu',
        'dashicons-admin-home', 99);
    
    add_submenu_page('cmgc-admin-menu',  // slug name for parent menu
        'Membership Waitlist', // title of page
        'Membership Waitlist', // name of sub-menu
        'edit_pages', // minimum capability (editor)
        'cmgc-admin-membership-waitlist', // slug name for submenu
        'cmgc_admin_membership_waitlist_page'); // function to call

    add_submenu_page('cmgc-admin-menu', // slug name for parent menu
        'Membership Applications', // title of page
        'Membership Applications', // name of sub-menu
        'edit_pages', // minimum capability (editor)
        'cmgc-admin-membership-applications', // slug name for submenu
        'cmgc_admin_membership_application_page'); // function to call

    add_submenu_page('cmgc-admin-menu', // slug name for parent menu
        'Tee Times', // title of page
        'Tee Times', // name of sub-menu
        'edit_pages', // minimum capability (editor)
        'cmgc-admin-tee_times', // slug name for submenu
        'cmgc_admin_tee_times_page'); // function to call

 }

 // Options are database entries in the WP database
 // waiting_list_upload_results is a return value passed between web pages, because I couldn't find a direct way to return a value
 function cmgc_admin_plugin_create_options()
 {
    // Only load these options if explicitly needed
    // Use WP options table to pass back result of upload to waitlist page
    add_option('cmgc_admin_plugin_options', // Name of the option to add
        array('waiting_list_upload_results' => ''), // Option value
        '', // deprecated
        "no"); // Whether to load the option when WordPress starts up

    add_option('cmgc_admin_plugin_options', // Name of the option to add
        array('save_tee_times_as_csv_results' => ''), // Option value
        '', // deprecated
        "no"); // Whether to load the option when WordPress starts up
 }

 function cmgc_admin_show_menu() {
    
 }

 // When the user clicks on the Membership Waitlist page, this function is called
 function cmgc_admin_membership_waitlist_page()
 {
    require_once plugin_dir_path(__FILE__) . 'src/upload_membership_waitlist.php';
    cmgc_admin_membership_waitlist_page2();

 }

 // When the user clicks on the "submit" waiting list button, this function is called
 add_action( 'admin_action_cmgc_admin_upload_waitlist', 'cmgc_admin_upload_waitlist_action' );
function cmgc_admin_upload_waitlist_action()
{
    //echo plugin_dir_path(__FILE__);
    require_once plugin_dir_path(__FILE__) . 'src/upload_membership_waitlist.php';
    cmgc_admin_upload_waitlist_action2();
     
    // These 2 calls to clear the output buffer (ob) are needed to make the redirect work
    //ob_clean();
    ob_start();

    // After doing the work, redirect back to the admin page.
    // cmgc_admin_upload_waitlist_action2() filled in the result, which is displayed
    // in the notice in cmgc_admin_membership_waitlist_page()
    if(wp_redirect( $_SERVER['HTTP_REFERER'] )){
        exit();
    }
    else {
        echo "redirect failed<br>";
    }
}

// When the user clicks on the Membership Applications page, this function is called
 function cmgc_admin_membership_application_page()
 {
    require_once plugin_dir_path(__FILE__) . 'src/submit_membership_application.php';
    cmgc_admin_membership_application_page2();

 }

  // When the user clicks on the "Clear Checked Applications" waiting list button, this function is called
  add_action( 'admin_action_cmgc_admin_clear_applications', 'cmgc_admin_clear_applications_action' );
  function cmgc_admin_clear_applications_action()
  {
    require_once plugin_dir_path(__FILE__) . 'src/submit_membership_application.php';
    if(cmgc_admin_clear_applications_action2())
    {
        // These 2 calls to clear the output buffer (ob) are needed to make the redirect work
        //ob_clean();
        ob_start();

        // After doing the work, redirect back to the admin page.
        // cmgc_admin_upload_waitlist_action2() filled in the result, which is displayed
        // in the notice in cmgc_admin_membership_waitlist_page()
        if(wp_redirect( $_SERVER['HTTP_REFERER'] )){
            exit();
        }
        else {
            echo "redirect failed<br>";
        }
    }
  }

// When the user clicks on the Membership Waitlist page, this function is called
 function cmgc_admin_tee_times_page()
 {
    require_once plugin_dir_path(__FILE__) . 'src/tee_times.php';
    cmgc_admin_tee_times_page2();

 }

 // When the user clicks on the "submit" waiting list button, this function is called
 add_action( 'admin_action_cmgc_save_tee_times_as_csv', 'cmgc_save_tee_times_as_csv_action' );
function cmgc_save_tee_times_as_csv_action()
{
    require_once plugin_dir_path(__FILE__) . 'src/tee_times.php';
    cmgc_save_tee_times_as_csv_action2();
}

 

 /*
 function add_custom_menu_item(){
    add_menu_page( 'Menu Item Title', 'Page Title', 'manage_options', 'page_slug', 'function', 'dashicons-icon', 1 );
}
add_action( 'admin_menu', 'add_custom_menu_item' );

function custom_menu_item_redirect() {

    $menu_redirect = isset($_GET['page']) ? $_GET['page'] : false;

    if($menu_redirect == 'page_slug' ) {
        wp_safe_redirect( home_url('/my-page') );
        exit();
    }

}
add_action( 'admin_init', 'custom_menu_item_redirect', 1 );
 */

 /*
 add_action( 'admin_init', 'cameronjonesweb_add_settings_link' );
add_filter( 'clean_url', 'cameronjonesweb_admin_menu_external_link', 10, 3 );

function cameronjonesweb_add_settings_link() {
	add_options_page(
		'My Settings Page Title',
		'My Settings Page Menu Title',
		'manage_options',
		'my-settings-page',
		'__return_false'
	);
}

function cameronjonesweb_admin_menu_external_link( $good_protocol_url, $original_url, $_context ) {
	if ( 'options-general.php?page=my-settings-page' === $good_protocol_url ) {
		$good_protocol_url = 'https://cameronjonesweb.com.au';
	}
	return $good_protocol_url;
}
 */

?>