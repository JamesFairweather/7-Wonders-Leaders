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

        private ObservableCollection<string> players = new ObservableCollection<string>();

        /// <summary>
        /// Initialise the Table UI
        /// </summary>
        /// <param name="c"></param>
        public TableUI(Coordinator c)
        {
            InitializeComponent();

            coordinator = c;

            //get the local IP address
            yourIPAddressField.Text = coordinator.client.ipAddr.ToString();

            //empty the chatTextBox
            chatTextBox.Text = "";

            playerList.ItemsSource = players;
        }

        /// <summary>
        /// Closing the Window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Window_Closed(object sender, EventArgs e)
        {
            //Close the connection if the ready button is enabled
            //I.e. if the Ready button was not pressed.
            if (readyButton.IsEnabled == true)
            {
                coordinator.hasGame = false;

                coordinator.client.CloseConnection();
            }
        }

        /// <summary>
        /// Event handler for using the Send button on the chat
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void sendButton_Click(object sender, RoutedEventArgs e)
        {
            coordinator.sendChat();
        }
        /// <summary>
        /// Event handler for the Enter button on the Chat
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void textBox1_PreviewKeyDown(object sender, KeyEventArgs e)
        { 
            if (e.Key == Key.Return) coordinator.sendChat(); 
        }

        public void SetPlayerInfo(NameValueCollection qscoll)
        {
            players.Clear();

            foreach (string s in qscoll["Names"].Split(','))
            {
                players.Add(s);
            }
        }

        /// <summary>
        /// UI Element that displays current Players at the table
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void dataGrid1_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            // dataGrid1.ItemsSource = "User1";// Server.htUsers;
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
            coordinator.iAmReady();
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

            /*
            JDF commented out in the interests of speeding up the game start.
            //Add a Leaders AI
            if (leaders_Checkbox.IsChecked == true)
            {
                coordinator.newAIUI('L');
            }
            //Add a Vanilla AI
            else
            {
                coordinator.newAIUI('V');
            }
            */
        }

        /// <summary>
        /// Remove an existing AI, if possible.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void removeAIButton_Click(object sender, RoutedEventArgs e)
        {
            coordinator.removeAI();
        }

        private void leaders_Checkbox_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)leaders_Checkbox.IsChecked)
            {
                coordinator.expansionSet = ExpansionSet.Leaders;
                coordinator.sendToHost("mL");       // mode Leaders
            }
            else
            {
                coordinator.expansionSet = ExpansionSet.Original;
                coordinator.sendToHost("mV");       // mode Vanilla
            }
        }
    }
}
