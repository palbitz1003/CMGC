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
using System.Windows.Shapes;

namespace WebAdmin.View
{
    /// <summary>
    /// Interaction logic for AddGhinWindow.xaml
    /// </summary>
    public partial class AddGhinWindow : Window
    {
        public string Ghin { get; set; }

        public AddGhinWindow()
        {
            InitializeComponent();
            GhinTextBox.Focus();
        }

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(GhinTextBox.Text))
            {
                MessageBox.Show("Please fill in the GHIN number. 0 is a valid GHIN number.");
                return;
            }

            int x;
            if (!int.TryParse(GhinTextBox.Text, out x))
            {
                MessageBox.Show("Please enter a valid number");
                return;
            }

            Ghin = GhinTextBox.Text;

            DialogResult = true;
            this.Close();
        }
    }
}
