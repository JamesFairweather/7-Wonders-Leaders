using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace SevenWonders
{
    //connection between server and clients
    public class Connection
    {

        TcpClient tcpClient;

        //The thread that will send information to the client
        public Thread senderThread;

        public StreamReader srReceiver;
        public StreamWriter swSender;

        //This holds the owner of the connection's nickname
        public string currentUser;

        public string strResponse;

        // public int numberOFAI;

        Server host;

        public Connection(TcpClient tcpCon, Server h)
        {
            tcpClient = tcpCon;

            host = h;

            // numberOFAI = numb;
            // Check if the User has sent the right information
            senderThread = new Thread(AcceptClient);

            // The thread calls the AcceptClient() method
            senderThread.Start();
        }

        /// <summary>
        /// Reject the connection attempt and close all open sockets
        /// </summary>
        public void CloseConnection()
        {
            // Close the currently open objects
            tcpClient.Close();
         
            senderThread.Abort();
            srReceiver.Close();
            swSender.Close();
        }

        // Occurs when a new connection is established.
        // If the client has an invalid name, or the table is full, then disconnect
        // Otherwise add the user to the Server properly
        public void AcceptClient()
        {
            srReceiver = new System.IO.StreamReader(tcpClient.GetStream());
            swSender = new System.IO.StreamWriter(tcpClient.GetStream());

            // Read the account information from the client
            currentUser = srReceiver.ReadLine();

            Console.WriteLine("Accepted a new connection from user {0}", currentUser);

            // swSender.WriteLine(currentUser);
            // swSender.Flush();

            // We got a response from the client
            if (currentUser != "")
            {
                // Store the user name in the hash table
                if (host.userMap.Keys.Contains(currentUser))
                {
                    // 0 means not connected
                    swSender.WriteLine("0|Invalid Nickname.");
                    swSender.Flush();
                    CloseConnection();
                    return;
                }
                // The table is full. There are already 7 Connected Users.
                else if (host.userMap.Count + host.NPCplayers == 7)
                {
                    // 0 means not connected
                    swSender.WriteLine("0|Table is full.");
                    swSender.Flush();
                    CloseConnection();
                    return;
                }
                else if (!host.acceptClient)
                {
                    // 0 means not connected
                    swSender.WriteLine("0|You can't join the Game in Progress");
                    swSender.Flush();
                    CloseConnection();
                }

                else
                {
                    // 1 means connected successfully
                    //UC-02 R05
                    swSender.WriteLine("1");
                    swSender.Flush();

                    // Add the user to the hash tables and start listening for messages from him
                    host.AddUser(tcpClient, currentUser);
                }
            }
            //The user's name is invalid.
            else
            {
                Console.WriteLine("Invalid username attempted to connect (Connection.AcceptClient())");
                CloseConnection();

                throw new System.Exception();
            }

            // THIS LISTENS FOR USER MESSAGE
            try
            {
                // Keep waiting for a message from the user
                while ((strResponse = srReceiver.ReadLine()) != "" )
                {
                    // If it's invalid, remove the user
                    if (strResponse == null)
                    {
                        host.RemoveUser(tcpClient);
                        CloseConnection();
                    }
                    else
                    {
                        host.receiveMessageFromConnection(currentUser, strResponse);

                        // TODO: I think this line was in the original source.
                        // Console.WriteLine("Received message from user {0}: {1}", currentUser, strResponse);
                    }
                }
            }
            catch(IOException)
            {
                // If anything went wrong with this user, disconnect him
                host.RemoveUser(tcpClient);
                CloseConnection();
            }
        }
    }
}
