using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;

namespace SevenWonders
{
    public class GMCoordinator
    {
        GameManager gameManager;

        Server host;

        int numOfPlayers;
        int numOfAI;

        int numOfCountdownsFinished;
        int numOfReadyPlayers;

        string[] playerNicks = new string[7];
        char[] AIStrats = new char[6];

        int numOfPlayersThatHaveTakenTheirTurn { get; set; }

        ExpansionSet currentMode = ExpansionSet.Original;

        public bool leadersEnabled { get { return currentMode == ExpansionSet.Leaders || currentMode == ExpansionSet.Cities; } }

        public bool citiesEnabled { get { return currentMode == ExpansionSet.Cities; } }

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

        public void ResetGMCoordinator()
        {
            //keep track of information at table UI
            numOfAI = 0;
            numOfPlayers = 0;
            numOfReadyPlayers = 0;
            numOfCountdownsFinished = 0;
            numOfPlayersThatHaveTakenTheirTurn = 0;

            // default mode is no expansion packs
            currentMode = ExpansionSet.Original;

            gameManager = null;

            for (int i = 0; i < AIStrats.Length; ++i)
            {
                AIStrats[i] = '\0';
            }

            for (int i = 0; i < playerNicks.Length; ++i)
            {
                playerNicks[i] = null;
            }
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

                Console.WriteLine("Message received.  From: {0}; Message={1}", nickname, message);

                if (message.Length >= 4 && message.Substring(0, 4) == "####")
                {
                    gameManager.playClientCard(nickname, HttpUtility.ParseQueryString(message.Substring(5)));
                    return;
                }
                
                //#: Chat string.
                if (message[0] == '#')
                {
                    host.sendMessageToAll("#" + nickname + ": " + message.Substring(1));
                }
                //J: Player joins the game
                //increment the numOfPlayers
                else if (message[0] == 'J')
                {
                    //store the player's nickname and increase the number of players
                    playerNicks[numOfPlayers++] = nickname;

                    host.sendMessageToAll("#" + nickname + " has joined the table.");
                }
                //R: Player hits the Ready button
                //increment the numOfReadyPlayers
                //if all players are ready then send the Start signal
                else if (message[0] == 'R')
                {
                    //server returns an error in the chat if there are not enough players in the game
                    //Sends the signal to re-enable the Ready buttons
                    if (numOfPlayers + numOfAI < 3)
                    {
                        host.sendMessageToAll("#Not enough players at the table. Need at least " + (3 - numOfPlayers) + " more participants.");
                        host.sendMessageToAll("S0");
                    }
                    else
                    {
                        //Increase the number of ready players
                        numOfReadyPlayers++;

                        //inform all that the player is ready
                        host.sendMessageToAll("#" + nickname + " is ready.");

                        //if all players are ready, then initialise the GameManager
                        if (numOfReadyPlayers == numOfPlayers)
                        {
                            //Do not accept any more players
                            host.acceptClient = false;

                            Console.WriteLine("All players have hit Ready.  Game is starting now with {0} AI players", numOfAI);

                            gameManager = new GameManager(this, numOfPlayers, playerNicks, numOfAI, AIStrats);

                            //S[n], n = number of players in this game

                            string strCreateUIMsg = string.Format("StrtGame{0}", gameManager.player.Count);

                            foreach (Player p in gameManager.player.Values)
                            {
                                strCreateUIMsg += string.Format(",{0}", p.nickname);
                            }

                            foreach (Player p in gameManager.player.Values)
                            {
                                sendMessage(p, strCreateUIMsg);
                            }

                            //set up the game, send information on boards to players, etc.
                            gameManager.beginningOfSessionActions();

                            //set the number of countdowns finished
                            numOfCountdownsFinished = 0;
                        }
                    }
                }
                //m: game mode options
                //changed by TableUI
                else if (message[0] == 'm')
                {
                    if (message[1] == 'L')
                    {
                        host.sendMessageToAll("#Leaders expansion pack enabled.");
                        currentMode = ExpansionSet.Leaders;
                    }
                    else if (message[1] == 'V')
                    {
                        host.sendMessageToAll("#All expansion packs are disabled.");
                        currentMode = ExpansionSet.Original;
                    }
                }
                //r: all player's countdowns are 
                //tell the GameManager to update each player's game UI
                else if (message[0] == 'r')
                {
                    //increase the number of players with countdowns finished
                    numOfCountdownsFinished++;
                    //everyone's countdown is finished
                    //display the first table UI for the first turn
                    if (numOfCountdownsFinished == numOfPlayers)
                    {
                        gameManager.updateAllGameUI();
                    }
                }
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
                        //increase the number of players
                        if ((numOfPlayers + numOfAI) < 7)
                        {
                            AIStrats[numOfAI++] = message[2];
                            host.updateAIPlayer(true);
                            host.sendMessageToAll("#AI added. There are currently " + numOfAI + " AI(s).");
                        }
                        else
                        {
                            host.sendMessageToAll("#There are " + numOfPlayers + " human players and " + numOfAI + " AI(s) already at the table.");
                        }
                    }
                    //"ar": remove AI in the GameManager
                    else if (message[1] == 'r')
                    {
                        if (numOfAI > 0)
                        {
                            numOfAI--;
                            host.updateAIPlayer(false);
                            host.sendMessageToAll("#An AI has been removed. There are " + numOfAI + " AI(s) remaining.");
                        }
                        else
                        {
                            //send back to the host, telling him AI cannot be removed, since there are none
                            host.sendMessageToUser(nickname, "#No AI is currently at the table.");
                        }
                    }
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
