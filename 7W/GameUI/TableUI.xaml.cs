using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
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
using System.Windows.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Threading;
using System.IO;
using System.Data;

namespace SevenWonders
{
    /// <summary>
    /// Interaction logic for TableUI.xaml
    /// </summary>
    public partial class TableUI : Window
    {
        Coordinator coordinator;

        private ObservableCollection<Persona> players = new ObservableCollection<Persona>();

        /// <summary>
        /// Initialise the Table UI
        /// </summary>
        /// <param name="c"></param>
        public TableUI(Coordinator c)
        {
            InitializeComponent();

            coordinator = c;

            //get the local IP address
            yourIPAddressField.Content = coordinator.client.ipAddr.ToString();

            lvPlayers.ItemsSource = players;
        }

        public void SetPlayerInfo(NameValueCollection qscoll)
        {
            players.Clear();

            string[] strPlayerNames = qscoll["Names"].Split(',');
            string[] strPlayerIPs = qscoll["ipAddrs"].Split(',');
            string[] strAIs = qscoll["isAI"].Split(',');
            string[] strPlayerStates = qscoll["isReady"].Split(',');

            for (int i = 0; i < strPlayerNames.Count(); ++i)
            {
                Persona p = new Persona();

                p.Name = strPlayerNames[i];
                p.IPAddress = strPlayerIPs[i];
                p.isAI = strAIs[i] == "True";
                p.isReady = strPlayerStates[i] == "True";

                if (p.Name == coordinator.nickname)
                {
                    btnReady.IsEnabled = p.isReady == false;
                }

                players.Add(p);
            }

            if (btnReady.IsEnabled)
                btnReady.IsEnabled = players.Count >= 3;
        }

        /// <summary>
        /// Ready button is clicked
        /// Send the ready signal to the coordinator and load the UI
        /// Disables the Ready button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void readyButton_Click(object sender, RoutedEventArgs e)
        {
            coordinator.sendToHost("R");
        }

        /// <summary>
        /// UC-03 R01
        /// Add an AI, if possible.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addAIButton_Click(object sender, RoutedEventArgs e)
        {
            // Add "difficult" AI
            coordinator.sendToHost("aa4");
        }

        /// <summary>
        /// Remove an existing AI, if possible.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void removeAIButton_Click(object sender, RoutedEventArgs e)
        {
            coordinator.sendToHost("ar");
        }

        private void leaders_Checkbox_Click(object sender, RoutedEventArgs e)
        {
            coordinator.sendToHost(string.Format("Expansion&Leaders={0}", (bool)leaders_Checkbox.IsChecked));
        }

        private void cities_Checkbox_Click(object sender, RoutedEventArgs e)
        {
            coordinator.sendToHost(string.Format("Expansion&Cities={0}", (bool)cities_Checkbox.IsChecked));
        }
    }

    public class Persona
    {
        public string Name { get; set; }

        public bool isAI { get; set; }

        public string IPAddress { get; set; }

        public bool isReady { get; set; }
    }
}
