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

        public Server host;

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

                bool MessageHandled = false;
                NameValueCollection qscoll;

                if (message.Length >= 8)
                {
                    switch (message.Substring(0, 8))
                    {
                        case "BldStrct":
                            qscoll = HttpUtility.ParseQueryString(message.Substring(9));
                            gameManager.buildStructureFromHand(nickname, qscoll["Structure"], qscoll["BuildWonderStage"], qscoll["FreeBuild"], qscoll["leftCoins"], qscoll["rightCoins"], qscoll["Bilkis"]);
                            MessageHandled = true;
                            break;

                        case "Discards":
                            qscoll = HttpUtility.ParseQueryString(message.Substring(9));
                            gameManager.discardCardForThreeCoins(nickname, qscoll["Structure"]);
                            MessageHandled = true;
                            break;

                        case "SendComm":
                            qscoll = HttpUtility.ParseQueryString(message.Substring(9));
                            gameManager.updateCommercePanel(nickname, qscoll["Structure"], qscoll["BuildWonderStage"]);
                            MessageHandled = true;
                            break;
                    }
                }

                if (MessageHandled)
                    return;

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

#if FALSE
                            if (currentMode == ExpansionSet.Leaders)
                            {
                                throw new NotImplementedException();
                                /*
                                //tell the GameManager to start on the beginning of session operations
                                gameManager = new LeadersGameManager(this, numOfPlayers, playerNicks, numOfAI, AIStrats);
                                //have to upcast to LeadersGameManager because polymorphism doesn't work in C#
                                gameManager = (LeadersGameManager)gameManager;
                                */
                            }
                            else if (currentMode == ExpansionSet.Original)
                            {
                                gameManager = new GameManager(this, numOfPlayers, playerNicks, numOfAI, AIStrats);
                            }
                            else throw new NotImplementedException();
#endif
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
                else if (message[0] == 'U')
                {
                    // main UI on client-side is ready; send board information to each one and initial actions
                    gameManager.sendBoardNames();
                }
                //m: game mode options
                //changed by TableUI
                else if (message[0] == 'm')
                {
                    //leaders mode checkbox enabled by host
                    //clear all the AIs
                    numOfAI = 0;

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

                    host.sendMessageToAll("#All AIs from previous mode cleared.");
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
                //O: player hits the Olympia power button
                else if (message[0] == 'O')
                {
                    throw new Exception();
                    //handle the Olympia button
                    //prepare to send Olympia UI information to the player
                    // gameManager.sendOlympiaInformation(nickname);
                }
                //o: player makes a selection in the Olympia UI
                else if (message[0] == 'o')
                {
                    throw new Exception();
                    //o(id)
                    //play the card for free from hand
                    // gameManager.playCardForFreeWithOlympia(nickname, message.Substring(1));
                    //Update the Played card panel
                    //gameManager.updatePlayedCardPanel(nickname);
                }
                //h: player asks for halicarnassus information
                else if (message[0] == 'h')
                {
                    throw new Exception();
                    //get the halicarnassus information and send it to the player
                    // gameManager.sendHalicarnassusInformation(nickname);
                }
                //H: player makes selection in Halicarnassus UI
                else if (message[0] == 'H')
                {
                    throw new Exception();
                    /*
                    //H(id)
                    //play the card for free from discard pile
                    gameManager.playCardForFreeWithHalicarnassus(nickname, message.Substring(1));
                    //Update the Played card panel
                    gameManager.updatePlayedCardPanel(nickname);
                    */
                }
                //b: player asks for babylon information
                else if (message[0] == 'b')
                {
                    throw new Exception("Babylon UI is no longer used.");
                    // gameManager.sendBabylonInformation(nickname);
                }
                //t: player has taken an action for the turn
                else if (message[0] == 't')
                {
                    gameManager.turnTaken(nickname);
                }
                //"L" for leave a game
                else if (message[0] == 'L')
                {
                    ResetGMCoordinator();
                    //Server.sendMessageToAll("#" + nickname + " has left the table.");
                    host.sendMessageToAll("#" + nickname + " has left the table.");
                    host.sendMessageToAll("#Game has stopped.");
                    host.sendMessageToAll("e");

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

                /////////////////////////////////////////////////////////////////////////////////
                //Leaders only

                //recruit the leader during age 0
                /*
                else if (message[0] == 'l')
                {
                    //id of the selected Leader card
                    int id = int.Parse(message.Substring(1));

                    gameManager.recruitLeader(nickname, id);
                }
                //someone uses Esteban power
                else if (message == "Esteban")
                {
                    //if user attempts to use Esteban power during recruitment turn, then re-enable their button, because Esteban power can't be used
                    if (gameManager.currentTurn == 0)
                    {
                        sendMessage(gameManager.player[nickname], "EE");
                    }
                    //otherwise, toggle the Esteban variable in the gameManager
                    else
                    {
                        gameManager.esteban = true;
                    }
                }

                //bilkis power is used
                else if (message[0] == 'k')
                {
                    //Bilkis power format
                    //(0 is nothing, 1 is ore, 2 is stone, 3 is glass, 4 is papyrus, 5 is loom, 6 is wood, 7 is brick

                    gameManager.bilkisPower(nickname, (byte)(int.Parse(message.Substring(1))));
                }

                //Rome power is used to play a Leader for free
                else if (message[0] == 'M')
                {
                    gameManager.playLeaderForFreeWithRome(nickname, message.Substring(1));
                    // gameManager.updatePlayedCardPanel(nickname);
                }

                //Courtesan's guild
                else if (message[0] == 'c')
                {
                    gameManager.playCourtesansGuild(nickname, message.Substring(1));
                    // gameManager.updatePlayedCardPanel(nickname);
                }
                */
                else
                {
                    // shouldn't get here.
                    throw new Exception();
                }
            }
        }
    }
}
