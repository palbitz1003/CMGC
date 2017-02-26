using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LocalHandicap
{
    public partial class MonthQuestion : Form
    {
        private DateTime _date;

        public MonthQuestion()
        {
            InitializeComponent();

            if (DateTime.Now.Day < 15)
            {
                RadioButton1st.Checked = true;
            }
            else
            {
                RadioButton15th.Checked = true;
            }

            _date = DateTime.Now;
            ThisMonthRadioButton.Text = DateTime.Now.ToString("MMMM");
            NextMonthRadioButton.Text = DateTime.Now.AddMonths(1).ToString("MMMM");
            ThisMonthRadioButton.Checked = true;
        }

        public DateTime Date { get { return _date; } private set { _date = value; } }

        private void DoneButton_Click(object sender, EventArgs e)
        {
            int day = 1;
            if (RadioButton15th.Checked)
            {
                day = 15;
            }

            int month = DateTime.Now.Month;
            if (NextMonthRadioButton.Checked)
            {
                month = DateTime.Now.AddMonths(1).Month;
            }

            Date = new DateTime(DateTime.Now.Year, month, day);

            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}