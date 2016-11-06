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
using System.Web;

namespace SevenWonders
{
    /// <summary>
    /// Interaction logic for NewCommerce.xaml
    /// Computation is mostly local. 
    /// </summary>
    public partial class NewCommerce : Window
    {
        const int ICON_WIDTH = 25;
        const int DAG_BUTTON_WIDTH = ICON_WIDTH;

        //player's coin that will be resetted to everytime the reset button is pressed
        //unfortunately, cant make this value constant.
        int PLAYER_COIN;

        Coordinator coordinator;

        Cost cardCost;

        bool hasBilkis;
        bool usedBilkis;
        string leaderDiscountCardId;
        bool leftRawMarket, rightRawMarket, marketplace;
        string leftName, middleName, rightName;
        Card cardToBuild;
        // int ID;
        bool isStage;

        //current accumulated resources
        string strCurrentResourcesUsed = "";
        //how much coin to pay to left and right
        int leftcoin = 0, rightcoin = 0;
        //how many resources are still needed. 0 means no more resources are needed
        int resourcesNeeded;

        // Resource Managers
        ResourceManager leftDag = new ResourceManager(), middleDag = new ResourceManager(), rightDag = new ResourceManager();

        //DAG buttons. [level][number]
        //e.g. For a DAG that has only 1 level, consisting of WBO, to get O, use [0][2]
        Button[,] leftDagButton, middleDagButton, rightDagButton;

        void CreateDag(ResourceManager d, string sourceStr)
        {
            string[] playerEffectsSplit = sourceStr.Split(',');

            for (int i = 0; i < playerEffectsSplit.Length; ++i)
            {
                d.add(new ResourceEffect(true, playerEffectsSplit[i]));
            }
        }

        /// <summary>
        /// Set the coordinator and handle CommerceInformation, which contains all necessary UI data, from GameManager
        /// </summary>
        public NewCommerce(Coordinator coordinator, Card cardToBuild, bool isWonderStage, /* List<Card> cardList, */ /*string cardName, int wonderStage,*/ NameValueCollection qscoll)
        {
            //intialise all the UI components in the xaml file (labels, etc.) to avoid null pointer
            InitializeComponent();

            this.coordinator = coordinator;

            leftName = "Left Neighbor";
            middleName = "Player";
            rightName = "Right Neighbor";

            this.cardToBuild = cardToBuild;
            this.isStage = isWonderStage;

            if (isStage)
            {
                string strWonderName = qscoll["WonderStageCard"];

                cardToBuild = coordinator.FindCard(strWonderName);
            }

            cardCost = cardToBuild.cost;

            string strLeaderDiscounts = qscoll["LeaderDiscountCards"];

            if (strLeaderDiscounts != string.Empty)
            {
                foreach (string strCardId in strLeaderDiscounts.Split(','))
                {
                    Card leaderDiscountCard = coordinator.FindCard(strCardId);

                    if (((StructureDiscountEffect)leaderDiscountCard.effect).discountedStructureType == cardToBuild.structureType)
                    {
                        leaderDiscountCardId = leaderDiscountCard.Id.ToString();
                    }
                }
            }

            leftRawMarket = false;
            rightRawMarket = false;

            CommercialDiscountEffect.RawMaterials rawMaterialsDiscount = (CommercialDiscountEffect.RawMaterials)
                Enum.Parse(typeof(CommercialDiscountEffect.RawMaterials), qscoll["resourceDiscount"]);

            switch (rawMaterialsDiscount)
            {
                case CommercialDiscountEffect.RawMaterials.BothNeighbors:
                    leftRawMarket = rightRawMarket = true;
                    break;

                case CommercialDiscountEffect.RawMaterials.LeftNeighbor:
                    leftRawMarket = true;
                    break;

                case CommercialDiscountEffect.RawMaterials.RightNeighbor:
                    rightRawMarket = true;
                    break;
            }

            marketplace = ((CommercialDiscountEffect.Goods)
                Enum.Parse(typeof(CommercialDiscountEffect.Goods), qscoll["goodsDiscount"]) == CommercialDiscountEffect.Goods.BothNeighbors);

            PLAYER_COIN = int.Parse(qscoll["coin"]);

            CreateDag(middleDag, qscoll["PlayerResources"]);
            CreateDag(leftDag, qscoll["LeftResources"]);
            CreateDag(rightDag, qscoll["RightResources"]);

            //set the name labels
            leftNameLabel.Content = leftName;
            middleNameLabel.Content = middleName;
            rightNameLabel.Content = rightName;

            //set the player's total coins
            playerCoinsLabel.Content = PLAYER_COIN;

            bankCoinsLabel.Content = cardCost.coin;

            //set the market images
            leftRawImage.Source = FindResource(leftRawMarket ? "1r" : "2r") as BitmapImage;
            rightRawImage.Source = FindResource(rightRawMarket ? "1r" : "2r") as BitmapImage;
            leftManuImage.Source = rightManuImage.Source = FindResource(marketplace ? "1m" : "2m") as BitmapImage;

            if (leaderDiscountCardId != null)
            {
                middleDag.add(new ResourceEffect(false, "WSBOCGP"));
            }

            hasBilkis = qscoll["Bilkis"] != null;

            if (hasBilkis)
            {
                imgBilkisPower.Visibility = Visibility.Visible;

                // Add Bilkis' choice
                middleDag.add(new ResourceEffect(false, "WSBOCGP"));
            }

            //generate mutable elements (DAG buttons, Price representations, currentResources, etc.)
            reset();
        }

        BitmapImage GetButtonIcon(char resource)
        {
            string resourceName = "";

            switch(resource)
            {
                case 'B': resourceName = "brick"; break;
                case 'O': resourceName = "ore"; break;
                case 'S': resourceName = "stone"; break;
                case 'W': resourceName = "wood"; break;
                case 'G': resourceName = "glass"; break;
                case 'C': resourceName = "loom"; break;
                case 'P': resourceName = "papyrus"; break;
            }

            return FindResource(resourceName) as BitmapImage;
        }

        /// <summary>
        /// Use the 3 DAGs in the object to generate the necessary Buttons in the UI and add EventHandlers for these newly added Buttons
        /// </summary>
        private void generateOneDAG(StackPanel pnl, out Button[,] b, ResourceManager dag, string buttonNamePrefix, bool isDagOwnedByPlayer)
        {
            //reset all DAG panels
            pnl.Children.Clear();

            List<ResourceEffect> dagGraphSimple = dag.getResourceList(isDagOwnedByPlayer).ToList();

            //generate a DAG for self or a neighbor
            //generate the needed amount of stackPanels, each representing a level
            StackPanel[] levelPanels = new StackPanel[dagGraphSimple.Count];

            //generate the needed amount of buttons
            b = new Button[dagGraphSimple.Count, 7];

            //look at each level of the DAG
            for (int i = 0 ; i < dagGraphSimple.Count; i++)
            {
                //initialise a StackPanels for the current level
                levelPanels[i] = new StackPanel();
                levelPanels[i].Orientation = Orientation.Horizontal;
                levelPanels[i].HorizontalAlignment = HorizontalAlignment.Center;

                //add to the StackPanels the appropriate buttons
                for (int j = 0; j < dagGraphSimple[i].resourceTypes.Length; j++)
                {
                    b[i, j] = new Button();
                    b[i, j].Content = dagGraphSimple[i];
                    b[i, j].FontSize = 1;

                    //set the Button's image to correspond with the resource
                    b[i, j].Background = new ImageBrush(GetButtonIcon(dagGraphSimple[i].resourceTypes[j]));

                    b[i, j].Width = DAG_BUTTON_WIDTH;
                    b[i, j].Height = DAG_BUTTON_WIDTH;

                    //set the name of the Button for eventHandler purposes
                    //Format: L_(level number)
                    b[i, j].Name = buttonNamePrefix + i + j;

                    b[i, j].IsEnabled = true;

                    //set action listener and add the button to the appropriate panel
                    b[i, j].Click += dagResourceButtonPressed;
                    levelPanels[i].Children.Add(b[i, j]);

                    // levelPanels[i] has b[i,j] added
                } // levelPanels[i] has added all the buttons appropriate for that level and its event handlers

                if (isDagOwnedByPlayer && hasBilkis && i == dagGraphSimple.Count-1)
                {
                    // Insert a label telling the player the Wild resource on
                    // the next line will cost 1 coin to use
                    Label bilkisLabel = new Label();
                    bilkisLabel.Content = "Bilkis (costs 1 coin)";
                    pnl.Children.Add(bilkisLabel);
                }

                //add the stack to the parent panel.
                pnl.Children.Add(levelPanels[i]);
            }
        }

        private void generateDAGs()
        {
            generateOneDAG(leftDagPanel, out leftDagButton, leftDag, "L_", false);
            generateOneDAG(middleDagPanel, out middleDagButton, middleDag, "M_", true);
            generateOneDAG(rightDagPanel, out rightDagButton, rightDag, "R_", false);
        }

        /// <summary>
        /// Event handler for the DAG buttons
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dagResourceButtonPressed(object sender, RoutedEventArgs e)
        {
            //determine which button was pressed
            Button pressed = sender as Button;
            string s = pressed.Name;

            //determine some information about the pressed button

            //level of the resource
            int level = Convert.ToInt32(s.Substring(2,1));
            //the location of the button (whether left, right, or middle)
            char location = s[0];

            //resource obtained
            ResourceEffect rce = pressed.Content as ResourceEffect;

            int resourceStringIndex = Convert.ToInt32(s.Substring(3));

            char resource = rce.resourceTypes[resourceStringIndex];

            //remember the current resource obtained amount for comparison with new resource obtained amount later
            int previous = resourcesNeeded;

            //add to the currentResources
            string strPossibleNewResourceList = strCurrentResourcesUsed + resource;

            //check if the newResource gets us closer to paying the cost.
            //If the newResource has the same distance as previous, then we have not gotten closer, and therefore we have just added an unnecessar resource
            //pop out an error to show this.

            if (resourcesNeeded == 0)
            {
                MessageBox.Show("You have for all necessary resources already");
                return;
            }
            // else if (ResourceManager.eliminate(cardCost.Copy(), false, strPossibleNewResourceList).Total() == previous)
            else if (middleDag.eliminate(cardCost.Copy(), false, strPossibleNewResourceList).Total() == previous)
            {
                MessageBox.Show("This resource will not help you pay for your cost");
                return;
            }

            bool isResourceRawMaterial = (resource == 'B' || resource == 'O' || resource == 'S' || resource == 'W');
            bool isResourceGoods = (resource == 'G' || resource == 'C' || resource == 'P');

            //add the appropriate amount of coins to the appropriate recepient
            //as well as doing appropriate checks
            if (location == 'M')
            {
                if (hasBilkis && level == middleDag.getResourceList(true).Count() - 1)
                {
                    // This is Bilkis' resource.
                    if (PLAYER_COIN == 0)
                    {
                        MessageBox.Show("You cannot afford this resource");
                        return;
                    }

                    usedBilkis = true;
                    imgBilkisPower.Opacity = 0.5;
                }
            }
            else if (location == 'L')
            {
                int coinsRequired = (isResourceRawMaterial && leftRawMarket) || (isResourceGoods && marketplace) ? 1 : 2;

                if ((PLAYER_COIN - (leftcoin + rightcoin)) < coinsRequired)
                {
                    MessageBox.Show("You cannot afford this resource");
                    return;
                }

                leftcoin += coinsRequired;
            }
            else if (location == 'R')
            {
                int coinsRequired = (isResourceRawMaterial && rightRawMarket) || (isResourceGoods && marketplace) ? 1 : 2;

                if ((PLAYER_COIN - (leftcoin + rightcoin)) < coinsRequired)
                {
                    MessageBox.Show("You cannot afford this resource");
                    return;
                }

                rightcoin += coinsRequired;
            }

            // The resource chosen is good: it is required and affordable.
            resourcesNeeded--;
            strCurrentResourcesUsed = strPossibleNewResourceList;

            if (location == 'L')
            {
                if (rce.IsDoubleResource())
                {
                    // Only hide the pressed button
                    pressed.Visibility = Visibility.Hidden;
                }
                else
                {
                    // Hide the other buttons on the same level 
                    for (int i = 0; i < leftDag.getResourceList(false).ToList()[level].resourceTypes.Length; i++)
                    {
                        leftDagButton[level, i].Visibility = Visibility.Hidden;
                    }
                }
            }
            else if (location == 'M')
            {
                if (rce.IsDoubleResource())
                {
                    // Only hide the pressed button
                    pressed.Visibility = Visibility.Hidden;
                }
                else
                {
                    for (int i = 0; i < middleDag.getResourceList(true).ToList()[level].resourceTypes.Length; i++)
                    {
                        // Hide the other buttons on the same level 
                        middleDagButton[level, i].Visibility = Visibility.Hidden;
                    }
                }
            }
            else if (location == 'R')
            {
                if (rce.IsDoubleResource())
                {
                    // Only hide the pressed button
                    pressed.Visibility = Visibility.Hidden;
                }
                else
                {
                    for (int i = 0; i < rightDag.getResourceList(false).ToList()[level].resourceTypes.Length; i++)
                    {
                        // Hide the other buttons on the same level 
                        rightDagButton[level, i].Visibility = Visibility.Hidden;
                    }
                }
            }

            //refresh the cost panel
            generateCostPanel();
        }

        /// <summary>
        /// Construct the labels at the cost panel, given the overall cost minus the current paid cost
        /// </summary>
        private void generateCostPanel()
        {
            generateCostPanelAndUpdateSubtotal(middleDag.eliminate(cardCost.Copy(), false, strCurrentResourcesUsed));
        }

        /// <summary>
        /// Construct the labels at the cost panel, given a cost
        /// </summary>
        /// <param name="cost"></param>
        private void generateCostPanelAndUpdateSubtotal(Cost cost)
        {
            costPanel.Children.Clear();
            Label[] costLabels = new Label[resourcesNeeded];

            Cost cpyCost = cost.Copy();

            //fill the labels with the appropriate image
            for (int i = 0; i < resourcesNeeded; i++)
            {
                BitmapImage iconImage = null;

                if (cpyCost.wood != 0)
                {
                    iconImage = GetButtonIcon('W');
                    --cpyCost.wood;
                }
                else if (cpyCost.stone != 0)
                {
                    iconImage = GetButtonIcon('S');
                    --cpyCost.stone;
                }
                else if (cpyCost.clay != 0)
                {
                    iconImage = GetButtonIcon('B');
                    --cpyCost.clay;
                }
                else if (cpyCost.ore != 0)
                {
                    iconImage = GetButtonIcon('O');
                    --cpyCost.ore;
                }
                else if (cpyCost.cloth != 0)
                {
                    iconImage = GetButtonIcon('C');
                    --cpyCost.cloth;
                }
                else if (cpyCost.glass != 0)
                {
                    iconImage = GetButtonIcon('G');
                    --cpyCost.glass;
                }
                else if (cpyCost.papyrus != 0)
                {
                    iconImage = GetButtonIcon('P');
                    --cpyCost.papyrus;
                }
                else
                {
                    // something went wrong
                    throw new Exception();
                }

                costLabels[i] = new Label();

                costLabels[i].Background = new ImageBrush(iconImage);
                costLabels[i].Width = ICON_WIDTH;
                costLabels[i].Height = ICON_WIDTH;

                //add the labels to costPanel
                costPanel.Children.Add(costLabels[i]);
            }

            int coinCost = cost.coin;

            if (usedBilkis) ++coinCost;

            //update the subtotals
            leftSubtotalLabel.Content = leftcoin;
            rightSubtotalLabel.Content = rightcoin;
            bankCoinsLabel.Content = coinCost;
            subTotalLabel.Content = coinCost + leftcoin + rightcoin;
        }

        /// <summary>
        /// Reset all information back to the beginning state (before user has taken any action)
        /// Called by constructor and resetButton
        /// </summary>
        private void reset()
        {
            strCurrentResourcesUsed = string.Empty;
            leftcoin = 0;
            rightcoin = 0;
            usedBilkis = false;
            imgBilkisPower.Opacity = 1.0;

            resourcesNeeded = cardCost.Total();

            generateCostPanel();
            generateDAGs();
        }

        /// <summary>
        /// Submit button event handler. Client sends to server the relevant data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void submitButton_Click(object sender, RoutedEventArgs e)
        {
            if (resourcesNeeded == 0)
            {
                // TODO: the response should be what resources were used from each neighbor.  The server should
                // calculate the cost and exchange coins.
                BuildAction buildAction = isStage ? BuildAction.BuildWonderStage : BuildAction.BuildStructure;

                string strResponse = string.Format("####&Action={0}&Structure={1}&leftCoins={2}&rightCoins={3}", buildAction, cardToBuild.Id, leftcoin, rightcoin);

                if (usedBilkis)
                {
                    strResponse += "&Bilkis=";
                }
                coordinator.sendToHost(strResponse);

                //signify to MainWindow that turn has been played
                coordinator.gameUI.playerPlayedHisTurn = true;

                Close();
            }
            //does not fulfill requirements
            else
            {
                MessageBox.Show("You must pay for unpaid resources");
            }
        }

        /// <summary>
        /// Event handler for the Reset button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            reset();
        }

        /// <summary>
        /// Event handler for the Close button
        /// Just close the window without any further actions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
