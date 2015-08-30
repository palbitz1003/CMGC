using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAdmin
{
    public class WebAddresses
    {
        public static string BaseAddress { get; set; }
        public static string ScriptFolder { get; set; }
        public const string SubmitTournament = "/submit_tournament.php";
        public const string SubmitWaitingList = "/submit_waiting_list.php";
        public const string SubmitRoster = "/submit_roster.php";
        public const string SubmitGHIN = "/submit_ghin.php";
        public const string SubmitTeeTimes = "/submit_tee_times.php";
        public const string SubmitLocalHandicap = "/submit_local_handicap.php";
        public const string SubmitClosestToThePin = "/submit_closest_to_the_pin.php";
        public const string SubmitResultsHtml = "/submit_results_html.php";
        public const string SubmitResultsCsv = "/submit_results_csv.php";
        public const string SubmitTournamentDescription = "/submit_tournament_description.php";
        public const string SubmitSignUpsWaitingList = "/submit_signups_waiting_list.php";
        public const string SubmitSignUps = "/submit_signups.php";
        public const string SubmitDues = "/submit_dues_payment.php";

        public const string GetSignups = "/get_signups.php";
        public const string GetTeeTimes = "/get_tee_times.php";
        public const string GetTournamentNames = "/get_tournament_names.php";
        public const string GetTournament = "/get_tournament.php";
        public const string GetClosestToThePin = "/get_closest_to_the_pin.php";
        public const string GetTournamentDescriptions = "/get_tournament_descriptions.php";
        public const string GetSignUpsWaitingList = "/get_signups_waiting_list.php";
        public const string GetDues = "/get_dues_paid_json.php";
    }
}
