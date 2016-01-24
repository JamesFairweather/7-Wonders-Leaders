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
        public NewCommerce commerceUI;
        //JoinTableUI joinTableUI;
        LeaderDraft leaderDraftWindow;
        FinalScore finalScoreUI;

        //The client that the application will use to interact with the server.
        public Client client { get; private set; }

        //User's nickname
        public string nickname;

        public string[] playerNames;

        public bool isLeadersEnabled = false;

        //Timer
        // int timeElapsed;
        // private const int MAX_TIME = 120;
        // private System.Windows.Threading.DispatcherTimer timer;

        //current turn
        // int currentTurn;

        List<Card> fullCardList = new List<Card>();

        public Card copiedLeader;

        public bool isFreeBuildButtonEnabled;

        public Coordinator(MainWindow gameUI)
        {
            this.gameUI = gameUI;

            nickname = "";

            hasGame = false;

            isFreeBuildButtonEnabled = false;

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
        /// User quits the Client program
        /// </summary>
        public void quit()
        {
            sendToHost("L");
            client.CloseConnection();
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

            if (!client.Connected)
                return;

            //set hasGame to true
            hasGame = true;

            //Display the non-host player version of TableUI
            sendToHost("J" + nickname);

            tableUI = new TableUI(this);
            /*
            I commented these lines out.  Previously, they were only enabled if you were the creator.  Which kind of makes sense.
            tableUI.addAIButton.IsEnabled = false;
            tableUI.removeAIButton.IsEnabled = false;
            tableUI.leaders_Checkbox.IsEnabled = false;
            */
            tableUI.ShowDialog();

            if (playerNames == null)
            {
                // If we get here and playerNames is null, the user closed the table UI dialog box
                // before pressing the ready button (or another player didn't press the ready button)
                // In that case, we will quit the game without showing the Main Window.
                hasGame = false;
                client.CloseConnection();
            }

            // create the leader draft window if the Leaders expansion is enabled.
            if (isLeadersEnabled)
                leaderDraftWindow = new LeaderDraft(this, false);
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
            if (message.Length >= 8)
            {
                bool messageHandled = false;

                NameValueCollection qcoll;

                switch (message.Substring(0, 8))
                {
                    case "UpdateUI":
                        qcoll = HttpUtility.ParseQueryString(message.Substring(9));

                        Application.Current.Dispatcher.Invoke(new Action(delegate
                        {
                            gameUI.updateCoinsAndCardsPlayed(qcoll);
                        }));
                        messageHandled = true;
                        break;

                    case "ChngMode":
                        // Basic/Leaders/Cities
                        qcoll = HttpUtility.ParseQueryString(message.Substring(9));

                        Application.Current.Dispatcher.Invoke(new Action(delegate
                        {
                            if (qcoll["Leaders"] != null)
                            {
                                tableUI.leaders_Checkbox.IsChecked = qcoll["Leaders"] == "True";
                                isLeadersEnabled = (bool)tableUI.leaders_Checkbox.IsChecked;
                            }

                            if (qcoll["Cities"] != null)
                            {
                                tableUI.cities_Checkbox.IsChecked = qcoll["Leaders"] == "True";
                            }
                        }));
                        messageHandled = true;
                        break;

                    case "Courtesn":        // send which neighboring leader is being copied.
                        qcoll = HttpUtility.ParseQueryString(message.Substring(9));

                        Application.Current.Dispatcher.Invoke(new Action(delegate
                        {
                            LeaderDraft leaderDraft = new LeaderDraft(this, true);
                            leaderDraft.UpdateUI(qcoll);
                            leaderDraft.Show();
                        }));
                        messageHandled = true;
                        break;

                    case "EnableFB":
                        isFreeBuildButtonEnabled = true;
                        messageHandled = true;
                        break;

                    case "FinalSco":
                        qcoll = HttpUtility.ParseQueryString(message.Substring(9));
                        Application.Current.Dispatcher.Invoke(new Action(delegate
                        {
                            finalScoreUI = new FinalScore(gameUI, qcoll);
                            finalScoreUI.Show();
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

                        Application.Current.Dispatcher.Invoke(new Action(delegate
                        {
                            gameUI.updateMilitaryTokens(qcoll);
                        }));
                        messageHandled = true;
                        break;

                    case "PlyrInfo":
                        qcoll = HttpUtility.ParseQueryString(message.Substring(9));
                        Application.Current.Dispatcher.Invoke(new Action(delegate
                        {
                            tableUI.SetPlayerInfo(qcoll);
                        }));

                        messageHandled = true;
                        break;

                    case "RespFail":
                        MessageBox.Show(message.Substring(9));
                        messageHandled = true;
                        break;
                    
                    case "StrtGame":
                        //Handle when game cannot start
                        if (message[1] == '0')
                        {
                            //re-enable the ready button
                            Application.Current.Dispatcher.Invoke(new Action(delegate
                            {
                                tableUI.btnReady.IsEnabled = true;
                            }));
                        }
                        //game is starting
                        else
                        {
                            //tell the server UI initialisation is done

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
                        // sendToHost("r");

                        messageHandled = true;
                        break;

                    case "SetPlyrH":        // Set player hand
                        qcoll = HttpUtility.ParseQueryString(message.Substring(9));

                        Application.Current.Dispatcher.Invoke(new Action(delegate
                        {
                            gameUI.showHandPanel(qcoll);
                        }));

                        messageHandled = true;
                        break;
                }

                if (messageHandled)
                    return;
            }

            //chat
            //enable Olympia power OR Rome power
            //activate the Olympia UI
            //receive the information on the current turn
            if (message[0] == 'T')
            {
                //get the current turn information
                //currentTurn = int.Parse(message[1] + "");
            }
            //received an unable to join message from the server
            //UC-02 R07
            else if (message[0] == '0')
            {
                MessageBox.Show(message.Substring(2));

                tableUI.Close();

                // displayJoinGameUI();
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

        public Card FindCard(string name)
        {
            return fullCardList.Find(x => x.Id == (CardId)Enum.Parse(typeof(CardId), name));
        }
    }
}
