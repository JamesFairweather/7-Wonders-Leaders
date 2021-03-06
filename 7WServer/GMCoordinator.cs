﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using NLog;

namespace SevenWonders
{
    public class PlayerInfo
    {
        public string name;
        public IPAddress ipAddress;
        public bool isAI;
        public bool isReady;
    }

    public class GMCoordinator
    {
        GameManager gameManager;

        Server host;

        List<PlayerInfo> players = new List<PlayerInfo>();

        public bool leadersEnabled = false;
        public bool citiesEnabled = false;

        private static Logger logger = LogManager.GetLogger("SevenWondersServer");

        /// <summary>
        /// Create a new server.
        /// Have the server start listening for requests.
        /// Part of UC-01 R01
        /// </summary>
        public GMCoordinator()
        {
            //create server
            host = new Server();
            host.StartServer();
            host.StatusChanged += new StatusChangedEventHandler(receiveMessage);

            ResetGMCoordinator();
        }

        public void Shutdown()
        {
            host.stopListening();
        }

        public void ResetGMCoordinator()
        {
            // default mode is no expansion packs
            leadersEnabled = false;
            citiesEnabled = false;

            gameManager = null;
        }

        public void SendUpdatedPlayers()
        {
            string strPlayerNames = "&Names=";
            string strPlayerIPs = "&ipAddrs=";
            string strAIs = "&isAI=";
            string strPlayerStates = "&isReady=";

            foreach (PlayerInfo pi in players)
            {
                strPlayerNames += pi.name + ",";
                strPlayerIPs += (pi.isAI ? "Server IP" : pi.ipAddress.ToString()) + ",";
                strAIs += pi.isAI + ",";
                strPlayerStates += pi.isReady + ",";
            }

            host.sendMessageToAll("PlyrInfo" + strPlayerNames.TrimEnd(',') + strPlayerIPs.TrimEnd(',') +
                strAIs.TrimEnd(',') + strPlayerStates.TrimEnd(','));
        }

        /////////////////////////////////////////////////////////////////////////////////////////
        /// Networking functionalities

        /// <summary>
        /// Send to Player p a message
        /// </summary>
        /// <param name="p"></param>
        /// <param name="message"></param>
        public void sendMessage(Player p, String message)
        {
            if(!p.isAI)
                host.sendMessageToUser(p.nickname, message);
        }

        public void SendMessageToAll(string msg)
        {
            host.sendMessageToAll(msg);
        }

        /// <summary>
        /// Receives a String from a user
        /// Parse the String and call the appropriate function in GameManager
        /// </summary>
        /// <param name="m"></param>
        public void receiveMessage(object sender, StatusChangedEventArgs e)
        {
            //lock makes sure that only one message is being processed at a time
            //and to ensure that data are not corrupted
            
            //an all encompassing lock statement like this hurts performance
            //therefore could use some optimisation
            lock (typeof(GMCoordinator))
            {
                //This is the nickname of the player that sent the message
                String nickname = e.nickname;

                //This is the string received from Server
                String message = e.message;

                logger.Info("Message received.  From: {0}; Message={1}", nickname, message);

                if (message.Length >= 4 && message.Substring(0, 4) == "####")
                {
                    gameManager.playClientCard(nickname, HttpUtility.ParseQueryString(message.Substring(5)));
                    return;
                }
                
                //#: Chat string.
                if (message[0] == '#')
                {
                    throw new Exception("Unexpected message received");
                    // host.sendMessageToAll("#" + nickname + ": " + message.Substring(1));
                }
                //J: Player joins the game
                //increment the numOfPlayers
                else if (message[0] == 'J')
                {
                    PlayerInfo p = new PlayerInfo();

                    p.name = nickname;
                    p.ipAddress = host.GetIPAddressOfUser(nickname);
                    p.isAI = false;
                    p.isReady = false;

                    players.Add(p);

                    SendUpdatedPlayers();
                    host.sendMessageToUser(nickname, string.Format("ChngMode&Leaders={0}", leadersEnabled));
                    host.sendMessageToUser(nickname, string.Format("ChngMode&Cities={0}", citiesEnabled));
                }
                //R: Player hits the Ready button
                //increment the numOfReadyPlayers
                //if all players are ready then send the Start signal
                else if (message[0] == 'R')
                {
                    PlayerInfo pi = players.Find(x => x.name == nickname);

                    pi.isReady = true;

                    SendUpdatedPlayers();

                    /*
                    //server returns an error in the chat if there are not enough players in the game
                    //Sends the signal to re-enable the Ready buttons
                    if (numOfPlayers + numOfAI < 3)
                    {
                        host.sendMessageToAll("#Not enough players at the table. Need at least " + (3 - numOfPlayers) + " more participants.");
                        host.sendMessageToAll("S0");
                    }
                    else
                    */
                    {

                        //Increase the number of ready players
                        // numOfReadyPlayers++;

                        //inform all that the player is ready
                        // host.sendMessageToAll("#" + nickname + " is ready.");

                        //if all players are ready, then initialise the GameManager
                        if (players.Exists(x => x.isReady == false) == false)
                        {
                            //Do not accept any more players
                            host.acceptClient = false;

                            logger.Info("All players have hit Ready.  Game is starting now with {0} AI players", players.Where(x => x.isAI == true).Count());

                            gameManager = new GameManager(this, players);

                            //S[n], n = number of players in this game

                            string strCreateUIMsg = string.Format("StrtGame&PlayerCount={0}&PlayerNames=", gameManager.player.Count);

                            foreach (Player p in gameManager.player.Values)
                            {
                                strCreateUIMsg += string.Format("{0},", p.nickname);
                            }

                            SendMessageToAll(strCreateUIMsg.TrimEnd(','));

                            //set up the game, send information on boards to players, etc.
                            gameManager.beginningOfSessionActions();

                            //set the number of countdowns finished
                            //numOfCountdownsFinished = 0;
                            gameManager.updateAllGameUI();
                        }
                    }
                }
                //m: game mode options
                //changed by TableUI
                else if (message.StartsWith("Expansion"))
                {
                    string whichExpansion = message.Substring(10);

                    if (whichExpansion.StartsWith("Leaders"))
                    {
                        leadersEnabled = whichExpansion.Substring(8) == "True";
                        host.sendMessageToAll(string.Format("ChngMode&Leaders={0}", leadersEnabled));
                    }
                    else if (whichExpansion.StartsWith("Cities"))
                    {
                        citiesEnabled = whichExpansion.Substring(7) == "True";
                        host.sendMessageToAll(string.Format("ChngMode&Cities={0}", citiesEnabled));
                    }
                }
#if FALSE
                //r: all player's countdowns are 
                //tell the GameManager to update each player's game UI
                else if (message[0] == 'r')
                {
                    //increase the number of players with countdowns finished
                    numOfCountdownsFinished++;
                    //everyone's countdown is finished
                    //display the first table UI for the first turn
                    if (numOfCountdownsFinished == 1 /*numOfPlayers*/)
                    {
                        gameManager.updateAllGameUI();
                    }
                }
#endif
                //"L" for leave a game
                else if (message[0] == 'L')
                {
                    ResetGMCoordinator();
                    //Server.sendMessageToAll("#" + nickname + " has left the table.");
                    host.sendMessageToAll("#" + nickname + " has left the table.");
                    //host.sendMessageToAll("#Game has stopped.");
                    //host.sendMessageToAll("e");

                    // TODO: reset the game state so another game can be played without having to restart the server.
                    // host.stopListening();
                }
                //"a": AI management
                else if (message[0] == 'a')
                {
                    //"aa": add AI in the GameManager
                    if (message[1] == 'a')
                    {
                        if (players.Count < 7)
                        {
                            PlayerInfo pi = new PlayerInfo();

                            pi.name = "AI" + players.Where(x => x.isAI == true).Count();
                            pi.isAI = true;
                            pi.isReady = true;

                            players.Add(pi);

                            host.NPCplayers++;

                            SendUpdatedPlayers();
                        }
                        else
                        {
                            host.sendMessageToUser(nickname, "Failed&Message=There are already 7 players at this table.  Cannot add another player.");
                        }
                    }
                    //"ar": remove AI in the GameManager
                    else if (message[1] == 'r')
                    {
                        // Remove the last AI player.  Do nothing if there are no AI players.
                        for (int i = players.Count - 1; i != 0; i--)
                        {
                            if (players[i].isAI)
                            {
                                players.Remove(players[i]);
                                host.NPCplayers--;
                                break;
                            }
                        }

                        SendUpdatedPlayers();
                    }
                }
                else if (message.Substring(0, 12) == "DebtResponse")
                {
                    NameValueCollection nvc = HttpUtility.ParseQueryString(message.Substring(13));

                    gameManager.HandleDebtResponse(nickname, int.Parse(nvc["DebtTokens"]));
                }
                else
                {
                    // shouldn't get here.
                    throw new Exception();
                }
            }
        }
    }
}
