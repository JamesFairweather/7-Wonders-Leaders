using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Windows.Controls;
using System.ComponentModel;
using System.Data;
using System.Windows;

namespace SevenWonders
{
    public class Client
    {
        //Keep the User's nickname
        public string nickname;
        public StreamWriter swSender;
        public StreamReader srReceiver;

        public TcpClient tcpUser;

        public Coordinator c;

        public Thread thrMessaging;
        public IPAddress ipAddr;
        public bool Connected;

        public Client(Coordinator c, String user)
        {
            this.c = c;
            nickname = user;
        }

        /// <summary>
        /// Initialize a connection with the given Nickname
        /// Use Port 1989 (random number)
        /// </summary>
        /// <param name="nick"></param>
        /// <param name="ipAddr"></param>
        public void InitializeConnection(IPAddress ipAddr)
        {
            // Start a new TCP connection to the Server at the given IP address
            tcpUser = new TcpClient();

            try
            {
                tcpUser.Connect(ipAddr, 1989);
                swSender = new StreamWriter(tcpUser.GetStream());
                srReceiver = new StreamReader(tcpUser.GetStream());
            }
            catch (Exception)
            {
                MessageBox.Show(string.Format("Fatal error: a connection to the server on {0}:{1} could not be established.  Quitting", ipAddr, 1989), "7 Wonders");
                return;
            }

            //Send the nickname. Await for permission.
            swSender.WriteLine(nickname);
            swSender.Flush();


            //see if the Connection has accepted us.
            string connectionStatus = srReceiver.ReadLine();

            //Output the reason why the Server has rejected us
            if (connectionStatus[0] == '0')
            {
                MessageBox.Show("Could not join game: " + connectionStatus.Substring(2));
                return;
            }

            // Helps us track whether we're connected or not
            Connected = true;
            this.ipAddr = ipAddr;

            // Server has accepted us
            // Start the thread for receiving messages and further communication
            thrMessaging = new Thread(new ThreadStart(ReceiveMessages));
            thrMessaging.Start();
        }

        /// <summary>
        /// Keep listening for received messages from the Server
        /// Pass this message to Coordinator
        /// </summary>
        public void ReceiveMessages()
        {
            string messageReceived;

            // While we are successfully connected, read incoming lines from the server and pass it to coordinator
            while (Connected) 
            {
                try
                {
                    messageReceived = srReceiver.ReadLine();
                    if (Connected == false)
                    {
                        break;
                    }
                    //objWriter.WriteLine(messageReceived);
                    if (Connected == false)
                    {
                        break;
                    }
                    c.receiveMessage(messageReceived);
                    if (Connected == false)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    Connected = false;
                }
            }
        }

        /// <summary>
        /// Send a message to the Server, which is in the form 
        /// nickname_(the message)
        /// </summary>
        /// <param name="message"></param>
        public void SendMessageToServer(String message)
        {
            swSender.WriteLine(message);
            swSender.Flush();
        }

        // Closes a current connection
        public void CloseConnection()
        {
            if (Connected)
            {
                Connected = false;
                thrMessaging.Abort();
                swSender.Close();
                srReceiver.Close();
                tcpUser.Close();
            }
        }
    }
}
