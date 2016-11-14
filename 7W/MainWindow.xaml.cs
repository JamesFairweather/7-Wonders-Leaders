using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Effects;
using System.Web;

namespace SevenWonders
{
    public class PlayerState
    {
        public Dictionary<StructureType, WrapPanel> structuresBuilt = new Dictionary<StructureType, WrapPanel>(9);
        public Image lastCardPlayed;

        public PlayerStateWindow state;

        public PlayerState(PlayerStateWindow plyr, string name)
        {
            state = plyr;

            structuresBuilt[StructureType.RawMaterial] = plyr.ResourceStructures;
            structuresBuilt[StructureType.Goods] = plyr.GoodsStructures;
            structuresBuilt[StructureType.Commerce] = plyr.CommerceStructures;
            structuresBuilt[StructureType.Military] = plyr.MilitaryStructures;
            structuresBuilt[StructureType.Science] = plyr.ScienceStructures;
            structuresBuilt[StructureType.Civilian] = plyr.CivilianStructures;
            structuresBuilt[StructureType.Guild] = plyr.GuildStructures;
            structuresBuilt[StructureType.Leader] = plyr.LeaderStructures;
            structuresBuilt[StructureType.City] = plyr.CityStructures;

            plyr.CoinsImage.Visibility = Visibility.Visible;

            plyr.PlayerName.Content = name;
        }
    };

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //dimensions for the icons at the Player bars
        const int ICON_HEIGHT = 40;

        //Client's coordinator
        Coordinator coordinator;

        Dictionary<string, PlayerState> playerState = new Dictionary<string, PlayerState>();

        public bool playerPlayedHisTurn = false;

        NameValueCollection handData;

        List<KeyValuePair<Card, Buildable>> hand = new List<KeyValuePair<Card, Buildable>>();

        Buildable stageBuildable;

        bool canDiscardStructure;

        GamePhase phase;

        //constructor: create the UI. create the Coordinator object
        public MainWindow()
        {
            //create the coordinator
            coordinator = new Coordinator(this);

            InitializeComponent();


            //make graphics better
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.Fant);

            /* Test code for displaying the Final Score window without actually having to play through a game.
             * Uncomment if you want the UI to display this window, then exit.
             * 
            NameValueCollection testScores = HttpUtility.ParseQueryString("JaPlayer1=1,2,3,4,5,6,7,8,9,69&Player2=2,2,2,2,2,2,2,2,2,22&Player3=-3,-3,-3,-3,-3,-3,-3,-3,-3,33");
            NameValueCollection testScores = HttpUtility.ParseQueryString("Player1=1,2,3,4,5,6,7,8,9,69&Player2=2,2,2,2,2,2,2,2,2,22&Player3=-3,-3,-3,-3,-3,-3,-3,-3,-3,33&Player4=4,4,4,4,4,4,4,4,4,44&Player5=5,5,5,5,5,5,5,5,5,55&Player6=6,6,6,6,6,6,6,6,6,66&Player7=7,7,7,7,7,7,7,7,7,77");
            NameValueCollection testScores = HttpUtility.ParseQueryString("Player1=1,2,3,4,5,6,7,8,9,69&Player2=2,2,2,2,2,2,2,2,2,22&Player3=-3,-3,-3,-3,-3,-3,-3,-3,-3,33&Player4=4,4,4,4,4,4,4,4,4,44&Player5=5,5,5,5,5,5,5,5,5,55&Player6=6,6,6,6,6,6,6,6,6,66&Player7=7,7,7,7,7,7,7,7,7,77&Player8=8,8,8,8,8,8,8,8,8,88");
            FinalScore finalScoreUI = new FinalScore(this, testScores);
            finalScoreUI.ShowDialog();
            return;
            */

            /* Test code for displaying the debt token window without having to play a game.
             * Uncomment if you want the UI to display this window, then exit.
             * 
            //NameValueCollection testDebtWindow = HttpUtility.ParseQueryString("GetDebtTokens&coin=15&coinsToLose=10");
            NameValueCollection testDebtWindow = HttpUtility.ParseQueryString("GetDebtTokens&coin=10&coinsToLose=15");
            GetDebtToken d = new GetDebtToken(coordinator, testDebtWindow);
            d.ShowDialog();
            return;
            */

            JoinTableUI joinGameDlg = new JoinTableUI();
            bool succeeded = (bool)joinGameDlg.ShowDialog();

            if (!succeeded)
            {
                Close();
                return;
            }

            // Maybe I should have the ability to choose between Joining and Creating?
            // Original code allowed the creator to add AI and select the leaders.
            coordinator.joinGame(joinGameDlg.userName, IPAddress.Parse(joinGameDlg.ipAddressAsText));

            // coordinator.createGame();

            if (!coordinator.client.Connected)
            {
                Close();
                return;
            }

            PlayerStateWindow[,] seatMap = new PlayerStateWindow[,] {
                { SeatA, SeatF, SeatD, null, null, null, null, null },      // 3 players
                { SeatA, SeatG, SeatE, SeatC, null, null, null, null },     // 4 players
                { SeatA, SeatG, SeatF, SeatD, SeatC, null, null, null },    // 5 players
                { SeatA, SeatH, SeatF, SeatE, SeatD, SeatB, null, null },   // 6 players
                { SeatA, SeatH, SeatG, SeatF, SeatD, SeatC, SeatB, null},   // 7 players
                { SeatA, SeatH, SeatG, SeatF, SeatE, SeatD, SeatC, SeatB }, // 8 players
            };

            int playerIndex = 0;

            // each player should see the "A" position for his cards.  Find our index.  Will be 0 for the
            // first player (the game creator), 1 for the 2nd player, 2 for the 3rd, etc.
            while (coordinator.playerNames[playerIndex] != coordinator.nickname)
                playerIndex++;

            for (int i = 0; i < coordinator.playerNames.Length; ++i)
            {
                playerState.Add(coordinator.playerNames[playerIndex],
                    new PlayerState(seatMap[coordinator.playerNames.Length - 3, i], coordinator.playerNames[playerIndex]));
                ++playerIndex;
                if (playerIndex == coordinator.playerNames.Length)
                    playerIndex = 0;
            }

            // coordinator.sendToHost("U");
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //Menu UI event handlers

#if FALSE
        //Event handlers for clicking the Create Table button
        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            //tell the coordinator that create game Button is pressed
            //UC-01 R01
        }

        //Event handler for clicking the Join Table button
        private void JoinButton_Click(object sender, RoutedEventArgs e)
        {
            //tell the coordinator that join game Button is pressed
            //UC-02 R01
            coordinator.displayJoinGameUI();
        }

        private void NickNameButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {

            this.Close();
        }
#endif


        /////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // UI Updates
        // Receives Strings from Coordinator
        // to update various UI information


        public void updateLeaderIcons(NameValueCollection leaderNames)
        {
            lbLeaderIcons.Children.Clear();

            foreach (string leaderCardName in leaderNames.Keys)
            {
                Card leaderCard = coordinator.FindCard(leaderCardName);

                Image img = new Image();
                img.Source = FindResource("Icons/" + leaderCard.iconName) as BitmapImage;
                img.Height = ICON_HEIGHT;

                img.ToolTip = string.Format("{0} - cost: {1} coin{2}.  {3}",
                    leaderCard.Id, leaderCard.cost.coin, leaderCard.cost.coin >= 2 ? "(s)" : string.Empty, leaderCard.description);
                img.Name = leaderCard.strName;
                img.Margin = new Thickness(2);

                lbLeaderIcons.Children.Add(img);
            }
        }

        /// <summary>
        /// display the Cards in Player's hands and the available actions
        /// </summary>
        /// <param name="information"></param>
        public void showHandPanel(NameValueCollection qscoll)
        {
            handData = qscoll;

            //the player is in a new turn now because his UI are still updating.
            //Therefore set playerPlayedHisturn to false
            playerPlayedHisTurn = false;
            canDiscardStructure = true;

            hand.Clear();

            string[] strCards = handData["Cards"].Split(',');
            string[] strBuildStates = handData["BuildStates"].Split(',');

            for (int i = 0; i < strCards.Length; ++i)
            {
                if (strCards[i] != string.Empty)
                {
                    // can get an empty string after last card in the age was built.
                    hand.Add(new KeyValuePair<Card, Buildable>(coordinator.FindCard(strCards[i]), (Buildable)Enum.Parse(typeof(Buildable), strBuildStates[i])));
                }
            }

            string strWonderStage = handData["WonderStage"];
            if (strWonderStage != null)
            {
                stageBuildable = (Buildable)Enum.Parse(typeof(Buildable), strWonderStage.Substring(2));
            }

            phase = (GamePhase)Enum.Parse(typeof(GamePhase), handData["GamePhase"]);

            if (handData["Instructions"] != null)
            {
                lblPlayMessage.Content = new TextBlock()
                {
                    Text = handData["Instructions"],
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 14,
                };
            }

            canDiscardStructure = handData["CanDiscard"] == null || (handData["CanDiscard"] == "True");

            handPanel.Items.Clear();

            foreach (KeyValuePair<Card, Buildable> kvp in hand)
            {
                Image img = new Image();
                img.Source = FindResource(kvp.Key.Id.ToString()) as BitmapImage;

                ListBoxItem entry = new ListBoxItem();
                entry.Content = img;
                entry.BorderThickness = new Thickness(6);

                switch (kvp.Value)
                {
                    case Buildable.True:
                        entry.BorderBrush = new SolidColorBrush(Colors.Green);
                        break;

                    case Buildable.CommerceRequired:
                        entry.BorderBrush = new SolidColorBrush(Colors.Yellow);
                        break;

                    default:
                        entry.BorderBrush = new SolidColorBrush(Colors.Red);
                        break;
                }

                handPanel.Items.Add(entry);
            }

            // A card must be selected before the action buttons are activated.
            btnBuildStructure.IsEnabled = false;
            btnBuildWonderStage.IsEnabled = false;
            btnDiscardStructure.IsEnabled = false;
            btnBuildStructureForFree.IsEnabled = false;

            btnBuildStructure.Content = null;
            btnBuildStructureForFree.Content = null;

            if (canDiscardStructure)
            {
                btnBuildWonderStage.Content = null;
                btnDiscardStructure.Content = null;
            }
            else
            {
                btnBuildWonderStage.Content = new TextBlock()
                {
                    Text = "A free build card cannot be used to constructed a wonder stage",
                            TextAlignment = TextAlignment.Center,
                            TextWrapping = TextWrapping.Wrap
                };
                btnDiscardStructure.Content = new TextBlock()
                {
                    Text = string.Format("A free build card cannot be discarded"),
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                };
            }
        }

        private void handPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (handPanel.SelectedIndex < 0)
                return;

            if (coordinator.isFreeBuildButtonEnabled && phase == GamePhase.Playing)
            {
                if (hand[handPanel.SelectedIndex].Value != Buildable.StructureAlreadyBuilt)
                {
                    btnBuildStructureForFree.Content = new TextBlock()
                    {
                        Text = string.Format("Build this structure for free."),
                        TextAlignment = TextAlignment.Center,
                        TextWrapping = TextWrapping.Wrap
                    };

                    btnBuildStructureForFree.Visibility = Visibility.Visible;
                    btnBuildStructureForFree.IsEnabled = true;
                }
                else
                {
                    btnBuildStructureForFree.Content = new TextBlock()
                    {
                        Text = string.Format("You have already built the {0}", hand[handPanel.SelectedIndex].Key.strName),
                        TextAlignment = TextAlignment.Center,
                        TextWrapping = TextWrapping.Wrap
                    };

                    btnBuildStructureForFree.IsEnabled = false;
                }
            }

            lblDescription.Content = new TextBlock()
            {
                Text = hand[handPanel.SelectedIndex].Key.description,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };

            Card card = hand[handPanel.SelectedIndex].Key;

            // Update the status of the build buttons when a card is selected.
            switch (hand[handPanel.SelectedIndex].Value)
            {
                case Buildable.True:
                    btnBuildStructure.Content = new TextBlock()
                    {
                        Text = string.Format(card.isLeader ? "Recruit {0}" : "Build the {0}", card.strName),
                        TextAlignment = TextAlignment.Center,
                        TextWrapping = TextWrapping.Wrap
                    };
                    btnBuildStructure.IsEnabled = true;
                    break;

                case Buildable.CommerceRequired:
                    btnBuildStructure.Content = new TextBlock()
                    {
                        Text = string.Format("Build the {0} (commerce required)", card.strName),
                        TextAlignment = TextAlignment.Center,
                        TextWrapping = TextWrapping.Wrap
                    };
                    btnBuildStructure.IsEnabled = true;
                    break;

                case Buildable.InsufficientResources:
                    btnBuildStructure.Content = new TextBlock()
                    {
                        Text = string.Format("You do not have enough resources to build the {0}", card.strName),
                        TextAlignment = TextAlignment.Center,
                        TextWrapping = TextWrapping.Wrap
                    };
                    btnBuildStructure.IsEnabled = false;
                    break;

                case Buildable.InsufficientCoins:
                    btnBuildStructure.Content = new TextBlock()
                    {
                        Text = string.Format(card.isLeader ? "You do not have enough coins to recruit {0}" : "You don't have enough coins to buy the {0}", card.strName),
                        TextAlignment = TextAlignment.Center,
                        TextWrapping = TextWrapping.Wrap
                    };
                    btnBuildStructure.IsEnabled = false;
                    break;

                case Buildable.StructureAlreadyBuilt:
                    btnBuildStructure.Content = new TextBlock()
                    {
                        Text = string.Format("You have already built the {0}", card.strName),
                        TextAlignment = TextAlignment.Center,
                        TextWrapping = TextWrapping.Wrap
                    };
                    btnBuildStructure.IsEnabled = false;
                    break;
            }

            if (!canDiscardStructure)
                return;

            switch (stageBuildable)
            {
                case Buildable.True:
                    //                    btnBuildWonderStage.Content = new TextBlock() { new Run(string.Format("Build a wonder stage with the {0}", hand[handPanel.SelectedIndex].Key)));
                    btnBuildWonderStage.Content = new TextBlock()
                    {
                        Text = string.Format(card.isLeader ? "Use {0} to build a wonder stage" : "Build a wonder stage with the {0}", card.strName),
                        TextAlignment = TextAlignment.Center,
                        TextWrapping = TextWrapping.Wrap
                    };
                    btnBuildWonderStage.IsEnabled = true;
                    break;

                case Buildable.CommerceRequired:
                    btnBuildWonderStage.Content = new TextBlock() {
                        Text = string.Format(card.isLeader ? "Use {0} to build a wonder stage (commerce required)" : "Build a wonder stage with the {0} (commerce required)", card.strName),
                        TextAlignment = TextAlignment.Center,
                        TextWrapping = TextWrapping.Wrap
                    };
                    btnBuildWonderStage.IsEnabled = true;
                    break;

                case Buildable.InsufficientCoins:
                case Buildable.InsufficientResources:
                    btnBuildWonderStage.Content = new TextBlock()
                    {
                        Text = "Insufficient resources available to build the next wonder stage",
                        TextAlignment = TextAlignment.Center,
                        TextWrapping = TextWrapping.Wrap
                    };
                    btnBuildWonderStage.IsEnabled = false;
                    break;

                case Buildable.StructureAlreadyBuilt:
                    btnBuildWonderStage.Content = new TextBlock()
                    {
                        Text = "All wonder stages have been built",
                        TextAlignment = TextAlignment.Center,
                        TextWrapping = TextWrapping.Wrap
                    };
                    btnBuildWonderStage.IsEnabled = false;
                    break;
            }

            btnDiscardStructure.IsEnabled = true;
            btnDiscardStructure.Content = new TextBlock()
            {
                Text = string.Format(card.isLeader ? "Discard {0} for 3 coins" : "Discard the {0} for 3 coins", card.strName),
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };
        }

        /// <summary>
        /// Event handler for the Card Action Buttons created in showActionPanel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBuildStructureForFree_Click(object sender, RoutedEventArgs e)
        {
            if (playerPlayedHisTurn)
                return;

            ((Button)sender).IsEnabled = false;
            ((Button)sender).Visibility = Visibility.Hidden;
            coordinator.isFreeBuildButtonEnabled = false;
            // btnBuildStructureForFree_isEnabled = false;
            playerPlayedHisTurn = true;
            coordinator.sendToHost(string.Format("####&Action={0}&FreeBuild=&Structure={1}", BuildAction.BuildStructure, hand[handPanel.SelectedIndex].Key.Id));
        }

        private void btnBuildStructure_Click(object sender, RoutedEventArgs e)
        {
            if (playerPlayedHisTurn)
                return;

            if (hand[handPanel.SelectedIndex].Value == Buildable.True)
            {
                ((Button)sender).IsEnabled = false;
                playerPlayedHisTurn = true;
                coordinator.sendToHost(string.Format("####&Action={0}&Structure={1}", BuildAction.BuildStructure, hand[handPanel.SelectedIndex].Key.Id));
            }
            else
            {
                coordinator.commerceUI = new NewCommerce(coordinator, hand[handPanel.SelectedIndex].Key, false, handData);
                coordinator.commerceUI.ShowDialog();
            }

            if (hand[handPanel.SelectedIndex].Key.structureType == StructureType.Leader)
            {
                // Remove the recruited leader from the recruited leader list.
                foreach (Object obj in lbLeaderIcons.Children)
                {
                    Image img = obj as Image;
                    if (img.Name == hand[handPanel.SelectedIndex].Key.strName)
                    {
                        lbLeaderIcons.Children.Remove(img);
                        break;
                    }
                }
            }
        }

        private void btnBuildWonderStage_Click(object sender, RoutedEventArgs e)
        {
            if (playerPlayedHisTurn)
                return;

            if (stageBuildable == Buildable.True)
            {
                ((Button)sender).IsEnabled = false;
                playerPlayedHisTurn = true;
                coordinator.sendToHost(string.Format("####&Action={0}&Structure={1}", BuildAction.BuildWonderStage, hand[handPanel.SelectedIndex].Key.Id));
            }
            else
            {
                coordinator.commerceUI = new NewCommerce(coordinator, hand[handPanel.SelectedIndex].Key, true, handData);
                coordinator.commerceUI.ShowDialog();
            }

            if (hand[handPanel.SelectedIndex].Key.structureType == StructureType.Leader)
            {
                // Remove the recruited leader from the recruited leader list.
                foreach (Object obj in lbLeaderIcons.Children)
                {
                    Image img = obj as Image;
                    if (img.Name == hand[handPanel.SelectedIndex].Key.strName)
                    {
                        lbLeaderIcons.Children.Remove(img);
                        break;
                    }
                }
            }
        }

        private void btnDiscardStructure_Click(object sender, RoutedEventArgs e)
        {
            if (playerPlayedHisTurn)
                return;

            ((Button)sender).IsEnabled = false;
            playerPlayedHisTurn = true;
            coordinator.sendToHost(string.Format("####&Action={0}&Structure={1}", BuildAction.Discard, hand[handPanel.SelectedIndex].Key.Id));

            if (hand[handPanel.SelectedIndex].Key.structureType == StructureType.Leader)
            {
                // Remove the recruited leader from the recruited leader list.
                foreach (Object obj in lbLeaderIcons.Children)
                {
                    Image img = obj as Image;
                    if (img.Name == hand[handPanel.SelectedIndex].Key.strName)
                    {
                        lbLeaderIcons.Children.Remove(img);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// display the Board, given the String from Coordinator
        /// </summary>
        /// <param name="information"></param>
        public void showBoardImage(string player, String boardInformation)
        {
            playerState[player].state.PlayerBoard.Source = FindResource(boardInformation.Substring(2)) as BitmapImage;

            int nWonderStages = Int32.Parse(boardInformation.Substring(0, 1));

            for (int i = 0; i < nWonderStages; ++i)
            {
                ColumnDefinition cd = new ColumnDefinition();
                cd.Width = new GridLength(1, GridUnitType.Star);

                playerState[player].state.WonderStage.ColumnDefinitions.Add(cd);
            }

            for (int i = 0; i < nWonderStages; ++i)
            {
                Label b = new Label();

                b.Background = new SolidColorBrush(Colors.Azure);
                Grid.SetColumn(b, i);
                playerState[player].state.WonderStage.Children.Add(b);
            }
        }

        /// <summary>
        /// display the Played Cards combo boxes, given the String from Coordinator
        /// </summary>
        /// <param name="player">Player ID (0..7)</param>
        /// <param name="cardName">Name of the card</param>
        public void updateCoinsAndCardsPlayed(NameValueCollection qscoll)
        {
            string[] playerNames = qscoll["Names"].Split(',');
            string[] coins = qscoll["Coins"].Split(',');
            string[] debt = qscoll["Debt"].Split(',');
            string[] diplomacy = qscoll["Diplomacy"].Split(',');
            string[] cardNames = qscoll["CardNames"].Split(',');

            for (int i = 0; i < playerNames.Length; ++i)
            {
                string strCoins = coins[i];
                string strDebt = debt[i];
                string playerName = playerNames[i];
                string diplomacyTokenVisible = diplomacy[i];
                string cardName = cardNames[i];

                // some of these functions should be in the PlayerState class.
                TextBlock coins_text = new TextBlock()
                {
                    Text = "x " + strCoins,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    FontFamily = new FontFamily("Lucida Handwriting"),
                    FontSize = 18,
                    Foreground = new SolidColorBrush(Colors.White),
                };

                playerState[playerName].state.CoinsLabel.Content = coins_text;

                if (strDebt != "0")
                {
                    TextBlock debt_text = new TextBlock()
                    {
                        Text = "x " + strDebt,
                        TextAlignment = TextAlignment.Center,
                        TextWrapping = TextWrapping.Wrap,
                        FontFamily = new FontFamily("Lucida Handwriting"),
                        FontSize = 18,
                        Foreground = new SolidColorBrush(Colors.White),
                    };

                    playerState[playerName].state.DebtLabel.Content = debt_text;

                    // make the debt image visible as it is hidden until the player has taken a debt token
                    if (playerState[playerName].state.DebtImage.Visibility == Visibility.Hidden)
                    {
                        playerState[playerName].state.DebtImage.Visibility = Visibility.Visible;
                    }
                }

                // toggle the visibility of the diplomacy token, if necessary
                if (Convert.ToBoolean(diplomacyTokenVisible) &&
                    playerState[playerName].state.DiplomacyImage.Visibility == Visibility.Hidden) {
                    playerState[playerName].state.DiplomacyImage.Visibility = Visibility.Visible;
                } else if (!Convert.ToBoolean(diplomacyTokenVisible) &&
                    playerState[playerName].state.DiplomacyImage.Visibility == Visibility.Visible) {
                    playerState[playerName].state.DiplomacyImage.Visibility = Visibility.Hidden;
                }

                if (cardName.Length == 12 && cardName.Substring(0, 11) == "WonderStage")
                {
                    int stage = int.Parse(cardName.Substring(11));

                    Label l = playerState[playerName].state.WonderStage.Children[stage - 1] as Label;

                    l.Content = string.Format("Stage {0}", stage);
                    l.Background = new SolidColorBrush(Colors.Yellow);
                }
                else if (cardName == "Discarded")
                {
                    if (playerState[playerName].lastCardPlayed != null)
                    {
                        playerState[playerName].lastCardPlayed.Effect = null;
                        playerState[playerName].lastCardPlayed = null;
                    }
                }
                else
                {
                    Card lastPlayedCard = coordinator.FindCard(cardName);

                    if (coordinator.copiedLeader != null && coordinator.copiedLeader.Id == lastPlayedCard.Id)
                    {
                        /*
                        // Attempting to render the copied leader on top of the Courtesan's Guild icon.  Not working yet, so using
                        // the tooltip for now.
                        // update the Courtesan's Guild icon by adding the icon of the copied leader into it.
                        BitmapImage bmi_copied = new BitmapImage();
                        bmi_copied.BeginInit();
                        bmi_copied.UriSource = new Uri("pack://application:,,,/7W;component/Resources/Images/Icons/" + coordinator.copiedLeader.iconName + ".png");
                        bmi_copied.EndInit();
                        Image img_copied = new Image();
                        img_copied.Source = bmi_copied;

                        int CourtesanUIElement = playerState[playerName].structuresBuilt[StructureType.Guild].Children.Count - 1;

                        Image imgCourtesan = playerState[playerName].structuresBuilt[StructureType.Guild].Children[CourtesanUIElement] as Image;

                        RenderTargetBitmap rndBmp = new RenderTargetBitmap( Convert.ToInt32(imgCourtesan.ActualWidth), Convert.ToInt32(imgCourtesan.ActualHeight), 96, 96, PixelFormats.Pbgra32);

                        rndBmp.Render(imgCourtesan);
                        rndBmp.Render(img_copied);

                        imgCourtesan.Source = rndBmp;
                        */

                        int CourtesanUIElement = playerState[playerName].structuresBuilt[StructureType.Guild].Children.Count - 1;

                        Image uiCourtesan = playerState[playerName].structuresBuilt[StructureType.Guild].Children[CourtesanUIElement] as Image;

                        uiCourtesan.ToolTip = string.Format("Courtesan: The copied leader is {0}", coordinator.copiedLeader.strName);

                        return;
                    }

                    if (playerState[playerName].lastCardPlayed != null)
                        playerState[playerName].lastCardPlayed.Effect = null;

                    // Create a halo around the last card each player played to make it more clear which one was the last card played.
                    DropShadowEffect be = new DropShadowEffect();
                    be.ShadowDepth = 0;
                    be.BlurRadius = 25;
                    be.Color = Colors.OrangeRed;

                    Image iconImage = new Image();
                    iconImage.Source = FindResource("Icons/" + lastPlayedCard.iconName) as BitmapImage;
                    iconImage.Height = ICON_HEIGHT;                 // limit the height of each card icon to 30 pixels.
                    string strToolTip = string.Format("{0}: {1}", lastPlayedCard.strName, lastPlayedCard.description);
                    if (lastPlayedCard.chain[0] != string.Empty)
                    {
                        strToolTip += "  Chains to: " + lastPlayedCard.chain[0];
                        if (lastPlayedCard.chain[1] != string.Empty)
                        {
                            strToolTip += ", " + lastPlayedCard.chain[1];
                        }
                    }

                    iconImage.ToolTip = strToolTip;
                    iconImage.Margin = new Thickness(2);   // keep a 1-pixel margin around each card icon.
                    iconImage.Effect = be;

                    playerState[playerName].lastCardPlayed = iconImage;
                    playerState[playerName].structuresBuilt[lastPlayedCard.structureType].Children.Add(iconImage);
                }
            }
        }

        public void updateMilitaryTokens(NameValueCollection qcoll)
        {
            foreach (string playerName in qcoll.Keys)
            {
                // string should be age/victories in this age/total losses
                string[] s = qcoll[playerName].Split('/');

                PlayerState ps = playerState[playerName];

                if (s.Length != 3)
                    throw new Exception();

                int age = int.Parse(s[0]);
                int victoriesInThisAge = int.Parse(s[1]);

                if (victoriesInThisAge != 0)
                {
                    switch (age)
                    {
                        case 1:
                            for (int i = 0; i < victoriesInThisAge; ++i)
                            {
                                Image image = new Image();
                                image.Source = FindResource("ConflictAge1") as BitmapImage;
                                image.Height = 24;
                                image.ToolTip = "Age I military wins are worth 1 point each.";
                                ps.state.ConflictTokens.Children.Add(image);
                            }
                            break;

                        case 2:

                            for (int i = 0; i < victoriesInThisAge; ++i)
                            {
                                Image image = new Image();
                                image.Source = FindResource("ConflictAge2") as BitmapImage;
                                image.Height = 32;
                                image.ToolTip = "Age II military wins are worth 3 points each.";
                                ps.state.ConflictTokens.Children.Add(image);
                            }
                            break;

                        case 3:

                            for (int i = 0; i < victoriesInThisAge; ++i)
                            {
                                Image image = new Image();
                                image.Source = FindResource("ConflictAge3") as BitmapImage;
                                image.Height = 40;
                                image.ToolTip = "Age III military wins are worth 5 points each.";
                                ps.state.ConflictTokens.Children.Add(image);
                            }
                            break;
                    }
                }

                string strTotalLossTokens = s[2];

                if (strTotalLossTokens != "0")
                {
                    TextBlock losses_text = new TextBlock()
                    {
                        Text = "x " + strTotalLossTokens,
                        TextAlignment = TextAlignment.Center,
                        TextWrapping = TextWrapping.Wrap,
                        FontFamily = new FontFamily("Lucida Handwriting"),
                        FontSize = 18,
                        Foreground = new SolidColorBrush(Colors.White),
                    };

                    ps.state.LossLabel.Content = losses_text;

                    // make the debt image visible as it is hidden until the player has taken a debt token
                    if (ps.state.LossImage.Visibility == Visibility.Hidden)
                    {
                        ps.state.LossImage.Visibility = Visibility.Visible;
                    }

                }
            }
        }

#if FALSE
        private void chatTextField_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return) coordinator.sendChat(); 
        }

        private void joinGameIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            coordinator.displayJoinGameUI();
        }

        private void quitIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            coordinator.quit();
        }

        private void helpButton_Click(object sender, RoutedEventArgs e)
        {
            Help helpUI = new Help();
        }
#endif
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //If there is an ongoing game, then coordinator must quit the game first
            if (coordinator.hasGame == true)
            {
                coordinator.quit();
            }
        }
    }
}