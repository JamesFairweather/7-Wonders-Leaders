using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace SevenWonders
{
    /// <summary>
    /// Join Table UI
    /// </summary>
    public partial class JoinTableUI : Window
    {
        Coordinator coordinator;

        public JoinTableUI(Coordinator c)
        {
            InitializeComponent();
            coordinator = c;
        }

        /*
         * Parse the information in the Table UI
         * UC-02 R03
         */
        public void btnJoin_Click(object sender, RoutedEventArgs e)
        {
            IPAddress ip;

            if (!IPAddress.TryParse(ipAddressText.Text, out ip))
            {
                MessageBox.Show("Invalid server IP address.");
                return;
            }

            if (textUser.Text == string.Empty)
            {
                MessageBox.Show("You must enter a name for your player.");

                return;
            }

            // All user inputs are valid; close the dialog window
            Close();
        }

        /*
        private void buttonbtnCancel_Click2btnCancel_Click_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        */
        public string userName { get { return textUser.Text; } }

        public string ipAddressAsText{ get { return ipAddressText.Text; } }

        private void textUser_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Close();
        }
    }
}
