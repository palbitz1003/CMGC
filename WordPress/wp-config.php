<?php
/**
 * The base configurations of the WordPress.
 *
 * This file has the following configurations: MySQL settings, Table Prefix,
 * Secret Keys, WordPress Language, and ABSPATH. You can find more information
 * by visiting {@link http://codex.wordpress.org/Editing_wp-config.php Editing
 * wp-config.php} Codex page. You can get the MySQL settings from your web host.
 *
 * This file is used by the wp-config.php creation script during the
 * installation. You don't have to use the web site, you can just copy this file
 * to "wp-config.php" and fill in the values.
 *
 * @package WordPress
 */

// ** MySQL settings - You can get this info from your web host ** //
/** The name of the database for WordPress */
define('DB_NAME', 'paulalbitz_com');

/** MySQL database username */
define('DB_USER', 'paulalbitzcom');

/** MySQL database password */
define('DB_PASSWORD', 'KC4sv!dA');

/** MySQL hostname */
define('DB_HOST', 'mysql.paulalbitz.com');

define('FORCE_SSL', true);
define('FORCE_SSL_ADMIN',true);

/** Database Charset to use in creating database tables. */
define('DB_CHARSET', 'utf8');

/** The Database Collate type. Don't change this if in doubt. */
define('DB_COLLATE', '');

/**#@+
 * Authentication Unique Keys and Salts.
 *
 * Change these to different unique phrases!
 * You can generate these using the {@link https://api.wordpress.org/secret-key/1.1/salt/ WordPress.org secret-key service}
 * You can change these at any point in time to invalidate all existing cookies. This will force all users to have to log in again.
 *
 * @since 2.6.0
 */
define('AUTH_KEY',         '?y;(Zp~/dO*mA3ZJtJ5(L*n~4)S%;5I~3xY0~~MA2p22o5qK)K(ohcwrlBZ2LN7k');
define('SECURE_AUTH_KEY',  'C`ZI117w!ff78co^6l$IB#F|4hSfrV"Fzm6Q2Yz3npRX1#O0/n$4`FA%Bpa2f*L|');
define('LOGGED_IN_KEY',    'alCQW*VHE)9NX3IGBmb7NgdW`&1Oda3qef@8BAA/9XKC6oDcgxcW~+iVF3wHyG&E');
define('NONCE_KEY',        ';t0f$YSniylA|X`pvBRg9KO%mscJBf05*1`XI|:8:~MGEGpNsAq6e/5E?mzK2Rv:');
define('AUTH_SALT',        '$F_V:12MNL3vS":a**h8I*%q7$E`ECKgdC6a4gTmzO~$ckoE"0s0iAj$tbH//eZ^');
define('SECURE_AUTH_SALT', '`L5AviBvt6(~LA5Iliam~7XHJ"gys^~M1m%zUJ|_GlCFVcVvl4~@jn+?/!aA"C+N');
define('LOGGED_IN_SALT',   '`B^j(:B7"O@xDEs6ms(4AkzR9EQPRZy1vbjX2azvDs6xp%Glro/K+^gDH7GYelc2');
define('NONCE_SALT',       '1ov!R"7tj6%2z^+W~G`Z8Rz0zK;/ONJ8J;wG/xP#jeb:C~wIZxIV2&v%7BxmLZtH');

/**#@-*/

/**
 * WordPress Database Table prefix.
 *
 * You can have multiple installations in one database if you give each a unique
 * prefix. Only numbers, letters, and underscores please!
 */
$table_prefix  = 'wp_6ag8zs_';

/**
 * Limits total Post Revisions saved per Post/Page.
 * Change or comment this line out if you would like to increase or remove the limit.
 */
define('WP_POST_REVISIONS',  10);

/**
 * WordPress Localized Language, defaults to English.
 *
 * Change this to localize WordPress. A corresponding MO file for the chosen
 * language must be installed to wp-content/languages. For example, install
 * de_DE.mo to wp-content/languages and set WPLANG to 'de_DE' to enable German
 * language support.
 */
define('WPLANG', '');

/**
 * For developers: WordPress debugging mode.
 *
 * Change this to true to enable the display of notices during development.
 * It is strongly recommended that plugin and theme developers use WP_DEBUG
 * in their development environments.
 */
define('WP_DEBUG', false);

/* That's all, stop editing! Happy blogging. */

/** Absolute path to the WordPress directory. */
if ( !defined('ABSPATH') )
	define('ABSPATH', dirname(__FILE__) . '/');

/** Sets up WordPress vars and included files. */
require_once(ABSPATH . 'wp-settings.php');

