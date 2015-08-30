using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WebAdmin.View;

namespace WebAdmin
{
    public class Credentials
    {
        private string _loginName = string.Empty;
        public string LoginName 
        {
            get 
            {
                if (string.IsNullOrEmpty(_loginName))
                {
                    GetLoginAndPassword();
                }
                return _loginName; 
            }
        }

        private string _loginPassword = string.Empty;
        public string LoginPassword 
        {
            get
            {
                if (string.IsNullOrEmpty(_loginPassword))
                {
                    GetLoginAndPassword();
                }
                return _loginPassword; 
            }
        }

        public Credentials()
        {
            _loginName = "DbAdmin";
        }

        public void Init()
        {
            if (string.IsNullOrEmpty(_loginName) || string.IsNullOrEmpty(_loginPassword))
            {
                GetLoginAndPassword();
            }
        }

        public void GetLoginAndPassword()
        {
            CredentialsDialog cd = new CredentialsDialog();
            cd.Owner = App.Current.MainWindow;

            cd.UserNameTextBox.Text = _loginName;
            cd.PasswordTextBox.Password = _loginPassword;

            cd.ShowDialog();

            if(cd.DialogResult.HasValue && cd.DialogResult.Value)
            {
                _loginName = cd.UserNameTextBox.Text;
                _loginPassword = cd.PasswordTextBox.Password;
            }
        }

        public void CheckForInvalidPassword(string responseString)
        {
            // If the login fails, clear the password so it is asked for again
            if(!string.IsNullOrEmpty(responseString) && responseString.ToLower().Contains("invalid user name or password"))
            {
                _loginPassword = string.Empty;
            }
        }
    }
}
