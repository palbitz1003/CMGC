using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WebAdmin.View
{
    /// <summary>
    /// Interaction logic for TouranmentDescriptionTab.xaml
    /// </summary>
    public partial class TournamentDescriptionTab : UserControl
    {
        private string _pre = "<!DOCTYPE html><html><body>";
        private string _post = "</body></html>";

        public TournamentDescriptionTab()
        {
            InitializeComponent();
            HTMLDescriptionToggleButton.Content = "Show HTML Example";
        }

        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            ShowDescription();
            HTMLDescriptionToggleButton.Content = "Show HTML Example";
        }

        private void HTMLDescriptionToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.Compare(HTMLDescriptionToggleButton.Content.ToString(), "Show HTML Example", true) == 0)
            {
                HTMLDescriptionToggleButton.Content = "Show Description";

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("<h3>Unordered List:</h3>");
                sb.AppendLine("&lt;ul&gt;<br>");
                sb.AppendLine("&lt;li&gt;Line 1&lt;/li&gt;<br>");
                sb.AppendLine("&lt;li&gt;Line 2&lt;/li&gt;<br>");
                sb.AppendLine("&lt;/ul&gt;<br>");

                sb.Append("<h3>Here's what it looks like:</h3>");
                sb.AppendLine("<ul>");
                sb.AppendLine("<li>Line 1</li>");
                sb.AppendLine("<li>Line 2</li>");
                sb.AppendLine("</ul>");

                sb.AppendLine("<hr>");

                sb.AppendLine("<h3>Ordered List:</h3>");
                sb.AppendLine("&lt;ol&gt;<br>");
                sb.AppendLine("&lt;li&gt;Line 1&lt;/li&gt;<br>");
                sb.AppendLine("&lt;li&gt;Line 2&lt;/li&gt;<br>");
                sb.AppendLine("&lt;/ol&gt;<br>");

                sb.Append("<h3>Here's what it looks like:</h3>");
                sb.AppendLine("<ol>");
                sb.AppendLine("<li>Line 1</li>");
                sb.AppendLine("<li>Line 2</li>");
                sb.AppendLine("</ol>");

                sb.AppendLine("<hr>");

                sb.Append("<h3>Misc formatting:</h3>");
                sb.Append("&lt;b>Bold&lt;/b&gt; and &lt;u&gt;underline&lt;/u&gt; superscript 2&lt;sup&gt;nd&lt;/sup&gt;");

                sb.Append("<h3>Here's what it looks like:</h3>");
                sb.Append("<b>Bold</b> and <u>underline</u> superscript 2<sup>nd</sup>");

                WebBrowser.NavigateToString(_pre + sb.ToString() + _post);
            }
            else
            {
                HTMLDescriptionToggleButton.Content = "Show HTML Example";
                ShowDescription();

            }
        }

        private void ShowDescription()
        {
            if (!string.IsNullOrEmpty(DescriptionTextBox.Text))
            {
                WebBrowser.NavigateToString(_pre + DescriptionTextBox.Text + _post);
            }
            else
            {
                WebBrowser.NavigateToString(_pre + _post);
            }
        }
    }
}
