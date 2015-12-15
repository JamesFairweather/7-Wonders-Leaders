using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
using System.Timers;
using System.Web;

namespace SevenWonders
{
    public class Coordinator
    {
        //Is the client connected to an ongoing game?
        public bool hasGame;

        //The various UI that Coordinator keeps track of
        public MainWindow gameUI;
        TableUI tableUI;
        //JoinTableUI joinTableUI;
        LeaderDraft leaderDraftWindow;

        //The client that the application will use to interact with the server.
        public Client client { get; private set; }

        //User's nickname
        public string nickname;

        public string[] playerNames;

        public ExpansionSet expansionSet = ExpansionSet.Original;

        //Timer
        // int timeElapsed;
        // private const int MAX_TIME = 120;
        // private System.Windows.Threading.DispatcherTimer timer;

        //current turn
        int currentTurn;

        //Leaders
        BilkisUI bilkisUI;

        List<Card> fullCardList = new List<Card>();

        public Coordinator(MainWindow gameUI)
        {
            this.gameUI = gameUI;

            nickname = "";

            hasGame = false;

            /*
            //prepare the timer
            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = new TimeSpan(0, 0, 1);
            */
            // load the card list
            using (System.IO.StreamReader file = new System.IO.StreamReader(System.Reflection.Assembly.Load("GameManager").
                GetManifestResourceStream("GameManager.7 Wonders Card list.csv")))
            {
                // skip the header line
                file.ReadLine();

                String line = file.ReadLine();

                while (line != null && line != String.Empty)
                {
                    fullCardList.Add(new Card(line.Split(',')));
                    line = file.ReadLine();
                }
            }
        }

        /*
        //Update the 100 Second timer field
        public void timer_Tick(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(delegate
            {
                gameUI.timerTextBox.Text = (MAX_TIME - timeElapsed) + "";
                timeElapsed++;   

                if (timeElapsed == MAX_TIME+1)
                {
                    discardRandomCard();
                    //close all open windows.
                    for(int intCounter = App.Current.Windows.Count - 1; intCounter > 0; intCounter--)
                        App.Current.Windows[intCounter].Close();

                    timer.Stop();
                }
            }));
        }
        */

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#if FALSE
        /// <summary>
        /// Update the current stage of wonder label
        /// Start up the timer at this point
        /// </summary>
        /// <param name="message"></param>
        private void updateCurrentStageLabelAndStartTimer(string message)
        {
            //get the current stage
            int currentAge = int.Parse(message[1] + "");

            Application.Current.Dispatcher.Invoke(new Action(delegate
            {
                string content = "Current Stage: " + currentAge;
                gameUI.currentStageLabel.Content = content;
            }));
        }

        /// <summary>
        /// Start the 100 second timer
        /// </summary>
        private void startTimer()
        {
            //start up the timer
            timeElapsed = 0;
            timer.Start();
        }
#endif


        /// <summary>
        /// Update the Chat logs
        /// </summary>
        /// <param name="s"></param>
        public void updateChatTextBox(string s)
        {
            s = s + "\n";

            Application.Current.Dispatcher.Invoke(new Action(delegate
            {
                // gameUI.chatTextBox.Text += s;
                tableUI.chatTextBox.Text += s;

                // gameUI.scroll.ScrollToEnd();
                tableUI.scroll.ScrollToEnd();
            }));
        }

        /// <summary>
        /// User quits the Client program
        /// </summary>
        public void quit()
        {
            //If the client is not a server, then send to the host the close connection signal.
            //if (gmCoordinator != null)
            {
                sendToHost("L");
                client.CloseConnection();
            }

            //If the client is a server, send the 
        }

        public void endTurn()
        {
            // timer.Stop();
            sendToHost("t");
        }

        /// <summary>
        /// Sends a message to the server telling it to start a game of vanilla or leaders
        /// send Rv for vanilla
        /// send Rl for leaders
        /// </summary>
        /// <param name="gameMode"></param>
        public void iAmReady()
        {
            // Disable the ready button now that we've indicated we are ready to start.
            Application.Current.Dispatcher.Invoke(new Action(delegate
            {
                tableUI.readyButton.IsEnabled = false;
            }));

            sendToHost("R");
        }

        public void sendChat()
        {
            string message;

            //determine the textfield that is non-empty and send that
            //this should not be necessary later on, when the UI is better
            // if (gameUI.chatTextField.Text.Length != 0)
            // {
            //     message = gameUI.chatTextField.Text;
            // }
            // else
            {
                message = tableUI.chatTextField.Text;
            }

            if (message.Length > 0) //check that the string is valid
            {
                //Tell the coordinator to send this a chat message to the server
                //# means chat message
                sendToHost("#" + message);
            }

            //reset the textfields
            // gameUI.chatTextField.Text = "";
            tableUI.chatTextField.Text = "";
        }

        public void removeAI()
        {
            //tell the GMCoordinator, which in turn tells the GameManager, to remove an AI
            sendToHost("ar");
        }

        //tell the GMCoordinator, which in turn tells the GameManager, to add AI
        //UC-03 R01
        public void newAIUI(char mode)
        {
            AIStrategy aistrategy = new AIStrategy(mode, this);
            aistrategy.ShowDialog();
        }

        //display the Bilkis UI
        public void bilkisUIActivate()
        {
            //create new Olympia UI
            Application.Current.Dispatcher.Invoke(new Action(delegate
            {
                this.bilkisUI = new BilkisUI(this);
                bilkisUI.ShowDialog();
            }));
        }

#if TRUE
        /*
         * Send the Join Game request to the Server.
         */
        public void joinGame(string nickname, IPAddress serverIP)
        {
            this.nickname = nickname;

            client = new Client(this, nickname);
            client.InitializeConnection(serverIP);

            //set hasGame to true
            hasGame = true;

            //Display the non-host player version of TableUI
            sendToHost("J" + nickname);

            //display the tableUI
            //UC-02 R06

            tableUI = new TableUI(this);
            /*
            I commented these lines out.  Previously, they were only enabled if you were the creator.  Which kind of makes sense.
            tableUI.addAIButton.IsEnabled = false;
            tableUI.removeAIButton.IsEnabled = false;
            tableUI.leaders_Checkbox.IsEnabled = false;
            */
            tableUI.ShowDialog();

            if (expansionSet == ExpansionSet.Leaders)
                leaderDraftWindow = new LeaderDraft(this);
        }

        /*
         * Display the join table UI
         * UC-02 R02
         */
        /*
       public void displayJoinGameUI()
       {
           joinTableUI = new JoinTableUI(this);
           joinTableUI.ShowDialog();
       }
       */
#endif

        /**
         * Function called by MainWindow for creating a new game
         * UC-01 R02
         */
        public void createGame()
        {
            //create a GM Coordinator
            // JDF - this shouldn't be needed any more, the GMCoordinator has moved to a separate process.
            // gmCoordinator = new GMCoordinator();

            hasGame = true;

            //automatically set the nickname to Host if the nickname is currently blank (which is the default)
            if (nickname == "")
            {
                nickname = "Host";
            }

            //get my IP
            IPAddress myIP = myIPAddress();

            //create the TCP Client
            client = new Client(this, nickname);

            client.InitializeConnection(myIP);

            if (!client.Connected)
                return;

            //display the TableUI
            tableUI = new TableUI(this);
            //join the game as a player
            //UC-01 R03

            sendToHost("J" + nickname);
            tableUI.ShowDialog();
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Utility Classes
        /// </summary>
        /// <returns></returns>


        private IPAddress myIPAddress()
        {
#if TRUE
            IPAddress localIP = null;
            IPHostEntry host;

            host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip;
                }
            }

            return localIP;
#else
            return IPAddress.Loopback;
#endif
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Networking functionalities
        /// only interpretUIAction will call these
        /// </summary>
        /// <param name="s"></param>

        /// <summary>
        /// Send string s to the Server
        /// The nickname will automatically be sent to the Server in the form of
        /// nickname_(message)
        /// </summary>
        /// <param name="s"></param>
        public void sendToHost(string s)
        {
            if (client != null && client.Connected)
                client.SendMessageToServer(s);
        }

        /// <summary>
        /// Client has received a message
        /// Call the appropriate action based on the first character
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void receiveMessage(string message)
        {
            //First character | Meaning
            //# --------------- chat message. E.g. "#John: I need some coins!" would print the string on the Chat
            //# is also used to display a Player joining a table.
            //J --------------- a player has joined the table. Add a player to the Game Manager
            //S --------------- the all ready signal. 5 second count down, then the join table window closes.

            if (message.Length >= 8)
            {
                bool messageHandled = false;

                NameValueCollection qcoll;

                switch (message.Substring(0, 8))
                {
                    case "CardPlay":
                        qcoll = HttpUtility.ParseQueryString(message.Substring(9));

                        foreach (string s in qcoll.Keys)
                        {
                            Application.Current.Dispatcher.Invoke(new Action(delegate
                            {
                                gameUI.updateCardsPlayed(s, qcoll[s]);
                            }));
                        }
                        messageHandled = true;
                        break;

                    case "CommData":        // Commerce data

                        qcoll = HttpUtility.ParseQueryString(message.Substring(9));
                        Application.Current.Dispatcher.Invoke(new Action(delegate
                        {
                            //gameUI.showCommerceUI(s);
                            NewCommerce commerce = new NewCommerce(this, qcoll);

                            commerce.ShowDialog();
                        }));
                        messageHandled = true;
                        break;

                    case "EnableFB":

                        qcoll = HttpUtility.ParseQueryString(message.Substring(9));
                        Application.Current.Dispatcher.Invoke(new Action(delegate
                        {
                            gameUI.btnBuildStructureForFree.Visibility = Visibility.Visible;
                            gameUI.btnBuildStructureForFree_isEnabled = true;
                        }));
                        messageHandled = true;
                        break;

                    case "FinalSco":
                        qcoll = HttpUtility.ParseQueryString(message.Substring(9));
                        Application.Current.Dispatcher.Invoke(new Action(delegate
                        {
                            FinalScore fs = new FinalScore(qcoll);
                            fs.Show();
                        }));
                        messageHandled = true;
                        break;

                    case "LdrDraft":
                        qcoll = HttpUtility.ParseQueryString(message.Substring(9));
                        Application.Current.Dispatcher.Invoke(new Action(delegate
                        {
                            leaderDraftWindow.UpdateUI(qcoll);
                            leaderDraftWindow.Show();
                        }));
                        messageHandled = true;
                        break;

                    case "LeadrIcn":
                        qcoll = HttpUtility.ParseQueryString(message.Substring(9));
                        Application.Current.Dispatcher.Invoke(new Action(delegate
                        {
                            gameUI.updateLeaderIcons(qcoll);
                        }));
                        messageHandled = true;
                        break;

                    case "Military":
                        qcoll = HttpUtility.ParseQueryString(message.Substring(9));

                        foreach (string s in qcoll.Keys)
                        {
                            Application.Current.Dispatcher.Invoke(new Action(delegate
                            {
                                gameUI.updateMilitaryTokens(s, qcoll[s]);
                            }));
                        }
                        messageHandled = true;
                        break;

                    case "StrtGame":
                        //Handle when game cannot start
                        if (message[1] == '0')
                        {
                            //re-enable the ready button
                            Application.Current.Dispatcher.Invoke(new Action(delegate
                            {
                                tableUI.readyButton.IsEnabled = true;
                            }));
                        }
                        //game is starting
                        else
                        {
                            //tell the server UI initialisation is done
                            // sendToHost("r"); // JDF - moved to another location until after the gameUI is created.

                            // find out the number of players.
                            int nPlayers = int.Parse(message.Substring(8, 1));

                            // I may be able to set this to playerNames, but I'm not sure about thread safety.
                            playerNames = message.Substring(10).Split(',');

                            if (playerNames.Length != nPlayers)
                            {
                                throw new Exception(string.Format("Server said there were {0} players, but sent {1} names.", nPlayers, playerNames.Length));
                            }

                            //close the TableUI
                            Application.Current.Dispatcher.Invoke(new Action(delegate
                            {
                                tableUI.Close();
                            }));
                        }
                        messageHandled = true;
                        break;

                    case "SetBoard":
                        // Parse the query string variables into a NameValueCollection.
                        qcoll = HttpUtility.ParseQueryString(message.Substring(9));

                        foreach (string s in qcoll.Keys)
                        {
                            Application.Current.Dispatcher.Invoke(new Action(delegate
                            {
                                gameUI.showBoardImage(s, qcoll[s]);
                            }));
                        }

                        // Tell game server this client is ready to receive its first UI update, which will
                        // include coins and hand of cards.
                        sendToHost("r");

                        messageHandled = true;
                        break;

                    case "SetCoins":
                        qcoll = HttpUtility.ParseQueryString(message.Substring(9));

                        foreach (string s in qcoll.Keys)
                        {
                            Application.Current.Dispatcher.Invoke(new Action(delegate
                            {
                                gameUI.showPlayerBarPanel(s, qcoll[s]);
                            }));
                        }
                        messageHandled = true;

                        break;

                    case "SetPlyrH":        // Set player hand
                        {
                            // we cannot use the nicer NameValuePair because there may be two of the same
                            // card in the hand and these would be duplicate keys.  So we have to use a data
                            // structure 
                            IList<KeyValuePair<string, string>> qscoll = UriExtensions.ParseQueryString(message.Substring(8));

                            Application.Current.Dispatcher.Invoke(new Action(delegate
                            {
                                gameUI.showHandPanel(qscoll);
                            }));
                        }

                        messageHandled = true;
                        break;
                }

                if (messageHandled)
                    return;
            }

            //chat
            if (message[0] == '#')
            {
                updateChatTextBox(message.Substring(1));
            }
            //Receives signal from GMCoordinator on whether the game can start or not
            //S0 means game can start
            //S1 means game cannot (because of insufficient players)
            else if (message[0] == 'S')
            {
                // S[0|n] message is no longer used.
                throw new Exception();
                /*
                //Handle when game cannot start
                if (message[1] == '0')
                {
                    //re-enable the ready button
                    Application.Current.Dispatcher.Invoke(new Action(delegate
                    {
                        tableUI.readyButton.IsEnabled = true;
                    }));
                }
                //game is starting
                else
                {
                    //tell the server UI initialisation is done
                    // sendToHost("r"); // JDF - moved to another location until after the gameUI is created.

                    // find out the number of players.
                    string strNumPlayers = message.Substring(1);

                    numPlayers = int.Parse(strNumPlayers);

                    //close the TableUI
                    Application.Current.Dispatcher.Invoke(new Action(delegate
                    {
                        tableUI.Close();
                    }));
                }
                */
            }
            //update the current stage of wonder information
            else if (message[0] == 's')
            {
                // updateCurrentStageLabelAndStartTimer(message);
            }
            //indicate to client to start timer
            else if (message[0] == 't')
            {
                // startTimer();
            }
            //enable Olympia power OR Rome power
            //activate the Olympia UI
            //receive the information on the current turn
            else if (message[0] == 'T')
            {
                //get the current turn information
                currentTurn = int.Parse(message[1] + "");
            }
            //received an unable to join message from the server
            //UC-02 R07
            else if (message[0] == '0')
            {
                MessageBox.Show(message.Substring(2));

                tableUI.Close();

                // displayJoinGameUI();
            }

            //leaders: received Recruitment phase turn display (Age 0 turn)
            else if (message[0] == 'r')
            {
                Application.Current.Dispatcher.Invoke(new Action(delegate
                {
                    gameUI.showHandPanelLeadersPhase(message.Substring(1));
                }));
            }
            /*
            //enable the Esteban button
            else if (message == "EE")
            {
                Application.Current.Dispatcher.Invoke(new Action(delegate
                {
                    gameUI.estebanButton.IsEnabled = true;
                }));
            }
            //enable the Bilkis Button...forever
            else if (message == "EB")
            {
                Application.Current.Dispatcher.Invoke(new Action(delegate
                {
                    gameUI.bilkisButton.IsEnabled = true;
                }));
            }
            //receive Courtesan's guild information
            else if (message[0] == 'c')
            {
                Application.Current.Dispatcher.Invoke(new Action(delegate
                {
                    CourtesanUI courtUI = new CourtesanUI(this, message.Substring(1));
                    courtUI.ShowDialog();
                }));
            }
            */
            //receive the end of game signal
            else if (message[0] == 'e')
            {
                // timer.Stop();
            }
            else if (message[0] == '1')
            {
                // don't do anything
            }
            else
            {
                // recieved a message from the server that the client cannot handle.
                throw new Exception();
            }
        }

        /// <summary>
        /// Return back if the current turn is the last turn of the age
        /// </summary>
        public bool currentTurnIsLastTurnOfAge()
        {
            if (currentTurn > 4) return true;
            else return false;
        }

        public Card FindCard(string name)
        {
            return fullCardList.Find(x => x.Id == (CardId)Enum.Parse(typeof(CardId), name));
        }
    }
}
