using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SevenWonders
{
    public class GameManager
    {
        public enum GamePhase
        {
            // WaitingForPlayers,   // not used presently
            // Start,               // not used presently
            LeaderDraft,            // drafting leaders (i.e. before Age 1)
            LeaderRecruitment,      // playing a leader card at the start of each Age
            Playing,                // normal turn
            Babylon,                // Waiting for Babylon to play its last card in the age.
            Halikarnassos,          // Waiting for Halikarnassos to play from the discard pile.
            RomaB,                  // Waiting  for Roma (B) to play a leader after building 2nd or 3rd wonder stage
            Solomon,                // Waiting for Solomon to play from the discard pile (can coincide with Halikarnassos if Rome B builds its 2nd or 3rd wonder stage and the player plays Solomon)
            End,
        };

        // I have a feeling that we really only need 3 phases: LeaderDraft (drawing 4 leaders before the game), Leader Recruitment, and Playing.
        // Playing needs I think just two substates: Regular turn or Extra turn.  Extra turn would be any turn where the game has to wait for one
        // player to choose a card/action (i.e. Babylon B, Halikarnassos, Solomon, Roma B, China)

        public int numOfPlayers { get; set; }
        public int numOfAI { get; set; }

        public GMCoordinator gmCoordinator;

        public int currentAge;

        public int currentTurn;

        // JDF I think player should be a dictionary rather than an array.
        // public Dictionary<string, Player> player;

        public Dictionary<string, Player> player = new Dictionary<string, Player>();

        // A list would be better here.
        private Dictionary<Board.Wonder, Board> board;

        // I'd prefer to use a dictionary, but because there may be 2 (or even 3) of the same card,
        // I'll stay with a List container.
        List<Card> fullCardList = new List<Card>();

        public List<Deck> deckList = new List<Deck>();

        public List<Card> discardPile = new List<Card>();

        public bool esteban = false;

        // is true for the extra turn awarded when a player is playing the last card in his hand
        bool gettingBabylonExtraCard = false;

        // is true for the extra turn awarded when a player is playing a card from the discard pile.
        bool playingCardFromDiscardPile = false;

        List<Card> savedHandWhenPlayingFromDiscardPile;

        string[] playerNicks;

        public bool gameConcluded { get; set; }

        public GamePhase phase { get; private set; }

        /// <summary>
        /// Shared constructor for GameManager and LeadersGameManager
        /// Common begin of game tasks that are shared amongst all versions of 7W
        /// </summary>
        /// <param name="gmCoordinator"></param>
        public GameManager(GMCoordinator gmCoordinator, int numOfPlayers, String []playerNicks, int numOfAI, char []AIStrats)
        {
            this.gmCoordinator = gmCoordinator;

            //set the maximum number of players in the game to numOfPlayers + numOfAI
            this.numOfPlayers = numOfPlayers;
            this.numOfAI = numOfAI;
            this.playerNicks = playerNicks;

            //set the game to not finished, since we are just starting
            gameConcluded = false;

            // If the Leaders expansion pack is enabled, start with age 0 (leaders draft)
            currentAge = gmCoordinator.leadersEnabled ? 0 : 1;
            currentTurn = 1;

            //AI initialisation
            this.numOfAI = numOfAI;

            //player initialisation
            for (int i = 0; i < numOfPlayers; i++)
            {
                player.Add(playerNicks[i], new Player(playerNicks[i], false, this));
            }

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

            if (!gmCoordinator.citiesEnabled)
                fullCardList.RemoveAll(x => x.expansion == ExpansionSet.Cities);

            if (!gmCoordinator.leadersEnabled)
                fullCardList.RemoveAll(x => x.expansion == ExpansionSet.Leaders);

            //initialize the vanilla boards objects
            //does not assign the boards to players yet
            createBoards();

            // create the AIs
            for (int i = numOfPlayers; i < numOfAI + numOfPlayers; i++)
            {
                playerNicks[i] = "AI" + (i + 1);
                player.Add(playerNicks[i], createAI(playerNicks[i], AIStrats[i-numOfPlayers]));
            }

            // set each Player's left and right neighbours
            // this determines player positioning
            for (int i = 0; i < player.Count; i++)
            {
                if (i == 0)
                {
                    player[playerNicks[i]].setNeighbours(player[playerNicks[numOfPlayers + numOfAI - 1]], player[playerNicks[i + 1]]);
                }
                else if (i == numOfPlayers + numOfAI - 1)
                {
                    player[playerNicks[i]].setNeighbours(player[playerNicks[i - 1]], player[playerNicks[0]]);
                }
                else
                {
                    player[playerNicks[i]].setNeighbours(player[playerNicks[i - 1]], player[playerNicks[i + 1]]);
                }
            }

            phase = gmCoordinator.leadersEnabled ? GamePhase.LeaderDraft : GamePhase.Playing;
        }

        /// <summary>
        /// Creating a vanilla AI controlled Player class
        /// </summary>
        /// <param name="name"></param>
        protected virtual Player createAI(String name, char strategy)
        {
            Player thisAI = new Player(name, true, this);
            switch (strategy)
            {
                case '0':
                    thisAI.AIBehaviour = new AIMoveAlgorithm0();
                    break;
                case '1':
                    thisAI.AIBehaviour = new AIMoveAlgorithm1();
                    break;
                case '2':
                    thisAI.AIBehaviour = new AIMoveAlgorithm2();
                    break;
                case '3':
                    thisAI.AIBehaviour = new AIMoveAlgorithm3();
                    break;
                case '4':
                    thisAI.AIBehaviour = new AIMoveAlgorithm4();
                    break;
            }

            return thisAI;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////



        public virtual void sendBoardNames()
        {
            string strMsg = string.Empty;

            foreach (Player p in player.Values)
            {
                strMsg += string.Format("&{0}={1}/{2}", p.nickname, p.playerBoard.numOfStages, p.playerBoard.name);
            }

            foreach (Player p in player.Values)
            {
                gmCoordinator.sendMessage(p, "SetBoard" + strMsg);

                p.executeAction(this);
            }
        }

        /// <summary>
        /// Beginning of Session Actions for Vanilla game
        /// 1) Distribute a random board and 3 coins to all
        /// 2) Remove from all decks card that will not be used in the game
        /// </summary>
        /// <param name="numOfPlayers + numOfAI"></param>
        /// 
        public virtual void beginningOfSessionActions()
        {
            //distribute a random board and 3 coins to all and give the player their free resource
            //send the board display at this point
            foreach (Player p in player.Values)
            {
                p.playerBoard = popRandomBoard();
                int startingCoins = gmCoordinator.leadersEnabled ? 6 : 3;
                p.addTransaction(startingCoins);
                p.storeCardEffect(p.playerBoard.startingResourceCard);
            }

            for (int i = 0; i < 4; i++)
            {
                //deck[1] is age 1. deck[2] is age 2 ....
                deckList.Add(new Deck(fullCardList, i, numOfAI + numOfPlayers));
            }

            deckList[3].removeAge3Guilds(fullCardList.Where(x => x.structureType == StructureType.Guild).Count() - (numOfPlayers + numOfAI + 2));

            //deal the cards for the first age to the players
            //currentAge not incremented?
            dealDeck(currentAge);
        }

        /// <summary>
        /// End of age actions
        /// 1) Calculate and distribute conflict tokens
        /// 2) Re-enable Olympia
        /// 3) Take player's remaining card and deposit it to the discard pile
        /// 4) Go to the next 
        /// </summary>
        public void endOfAgeActions()
        {
            //distribute tokens
            distributeConflictTokens();

            string strUpdateMilitaryTokens = "Military";

            foreach (Player p in player.Values)
            {
                int nVictoryTokens = 0;

                switch(currentAge)
                {
                    case 1: nVictoryTokens = p.conflictTokenOne; break;
                    case 2: nVictoryTokens = p.conflictTokenTwo; break;
                    case 3: nVictoryTokens = p.conflictTokenThree; break;
                }

                // this string has the format: Current Age/Victories (0, 1, 2) for the *current* age/total loss tokens so far (0 or more)
                strUpdateMilitaryTokens += string.Format("&{0}={1}/{2}/{3}", p.nickname, currentAge, nVictoryTokens, p.lossToken);
            }

            foreach (Player p in player.Values)
            {
                gmCoordinator.sendMessage(p, strUpdateMilitaryTokens);
            }

            // check that all players' hands are empty, and re-enable Olympia's Power (build a card for free, once per age),
            // if it has been activated.
            foreach (Player p in player.Values)
            {
                if (p.olympiaPowerEnabled)
                {
                    p.olympiaPowerAvailable = true;
                    gmCoordinator.sendMessage(p, "EnableFB&Olympia=true");
                }

                // Check that the players' hand is empty as they should be discarding their last card on the 6th
                // turn of the age.
                if (p.hand.Count != 0)
                {
                    throw new Exception("Bug!  This player still has one or more cards in his hand at the end of the age.  Logic screwup.");
                }
            }
        }

        /// <summary>
        /// calculate and distribute conflict tokens
        /// </summary>
        public void distributeConflictTokens()
        {
            //go through each player and compare to the next person
            //remember the first player and keep follow the right
            //when first player is encountered again, all conflict tokens are distributed

            //the amount of conflict tokens one would get depends on the number of shields

            foreach (Player p in player.Values)
            {

                //if the current player's shield is greater than the next person, increase conflicttoken by the appropriate age
                //if less, get a losstoken
                if (p.shield > p.rightNeighbour.shield)
                {
                    if (currentAge == 1)
                    {
                        p.conflictTokenOne += 1;
                    }
                    else if (currentAge == 2)
                    {
                        p.conflictTokenTwo += 1;
                    }
                    else if (currentAge == 3)
                    {
                        p.conflictTokenThree += 1;
                    }

                    //check if player has played card 220: gain 2 coins for every victory point gained
                    //give 2 coins if so
                    if (p.playedStructure.Exists(x => x.Id == CardId.Nero))
                    {
                        throw new Exception();
                        // fix this.  Likely by making the player.GiveConflictToken into a function and
                        // having that function check whether the current player has Nero.
                        // p.coin += 2;
                    }

                    //check if right neighbour has played card 232: return conflict loss token received
                    //if no, receive lossToken
                    //if yes, do not get lossToken, instead, give lossToken to winner
                    if (p.rightNeighbour.playedStructure.Exists(x => x.Id == CardId.Tomyris) == false)
                    {
                        p.rightNeighbour.lossToken++;
                    }
                    else
                    {
                        //the loser is rightNeighbour
                        //the winner is current player. current player will get the loss token
                        p.lossToken++;
                    }
                }
                else if (p.shield < p.rightNeighbour.shield)
                {
                    if (currentAge == 1)
                    {
                        p.rightNeighbour.conflictTokenOne += 1;
                    }
                    else if (currentAge == 2)
                    {
                        p.rightNeighbour.conflictTokenTwo += 1;
                    }
                    else if (currentAge == 3)
                    {
                        p.rightNeighbour.conflictTokenThree += 1;
                    }

                    /*
                    //check if player has played card 220: gain 2 coins for every victory point gained
                    //give 2 coins if so
                    if (p.rightNeighbour.playedStructure.Exists(x => x.name == "Nero"))
                    {
                        p.rightNeighbour.coin += 2;
                    }
                    */

                    //check if I have played card 232: return conflict loss token received
                    //if no, receive lossToken
                    //if yes, do not get lossToken, instead, give lossToken to rightNeighbour
                    if (p.playedStructure.Exists(x => x.Id == CardId.Tomyris) == false)
                    {
                        p.lossToken++;
                    }
                    else
                    {
                        //the loser is rightNeighbour
                        //the winner is current player. current player will get the loss token
                        p.rightNeighbour.lossToken++;
                    }
                }

            }
        }

        /// <summary>
        /// End of game actions
        /// Calculate the score and proclaim the winner
        /// </summary>
        protected void endOfSessionActions()
        {
            string strFinalScoreMsg = string.Empty;
            List<KeyValuePair<string, Score>> playerScores = new List<KeyValuePair<string, Score>>(numOfPlayers + numOfAI);

            //execute the end of game actions for all players
            //find the maximum final score
            foreach (Player p in player.Values)
            {
                Score sc = p.executeEndOfGameActions();
                playerScores.Add(new KeyValuePair<string, Score>(p.nickname, sc));
            }

            // sort the scores into lowest to highest
            playerScores.Sort(delegate (KeyValuePair<string, Score> p1, KeyValuePair<string, Score> p2)
            {
                int victoryPointDiff = p2.Value.Total() - p1.Value.Total();

                if (victoryPointDiff != 0)
                    return victoryPointDiff;
                else
                    return p2.Value.coins - p1.Value.coins;
            });

            string strFinalScore = "FinalSco";

            //broadcast the individual scores
            foreach (KeyValuePair<string, Score> s in playerScores)
            {
                Score sc = s.Value;

                strFinalScore += string.Format("&{0}={1},{2},{3},{4},{5},{6},{7},{8},{9}",
                    s.Key, sc.military, sc.coins, sc.wonders, sc.civilian, sc.commerce, sc.guilds, sc.science, sc.leaders, sc.Total());
            }

            foreach (Player p in player.Values)
            {
                gmCoordinator.sendMessage(p, strFinalScore);
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////


        /// <summary>
        /// Shuffle and Deal a deck to all Players
        /// </summary>
        /// <param name="d"></param>
        protected void dealDeck(int currentAge)
        {
            //shuffle the deck
            Deck deck = deckList.Find(x => x.age == currentAge);

            if (currentAge != 0)
                deck.shuffle();

            //if the current deck is 0, then that means we are dealing with the leaders deck
            int numCardsToDeal;

            numCardsToDeal = currentAge == 0 ? 4 : 7;

            //deal cards to each Player from Deck d
            foreach (Player p in player.Values)
            {
                for (int j = 0; j < numCardsToDeal; j++)
                {
                    Card c = deck.GetTopCard();
                    p.hand.Add(c);
                }
            }
        }

        /// <summary>
        /// Inherited class that will return to the caller class and subclasses a set of 14 or 16 boards
        /// Depending on the specified boardfile
        /// Vanilla: boards.txt
        /// Leaders: leadersboards.txt
        /// </summary>
        protected void createBoards()
        {
            board = new Dictionary<Board.Wonder, Board>(16)
            {
                { Board.Wonder.Alexandria_A, new Board(ExpansionSet.Original, Board.Wonder.Alexandria_B, "Alexandria (A)", CardId.Alexandria_A_Board, new ResourceEffect(true, "G"), 3) },
                { Board.Wonder.Alexandria_B, new Board(ExpansionSet.Original, Board.Wonder.Alexandria_A, "Alexandria (B)", CardId.Alexandria_B_Board, new ResourceEffect(true, "G"), 3) },
                { Board.Wonder.Babylon_A, new Board(ExpansionSet.Original, Board.Wonder.Babylon_B, "Babylon (A)", CardId.Babylon_A_Board, new ResourceEffect(true, "B"), 3) },
                { Board.Wonder.Babylon_B, new Board(ExpansionSet.Original, Board.Wonder.Babylon_A, "Babylon (B)", CardId.Babylon_B_Board, new ResourceEffect(true, "B"), 3) },
                { Board.Wonder.Ephesos_A, new Board(ExpansionSet.Original, Board.Wonder.Ephesos_B, "Ephesos (A)", CardId.Ephesos_A_Board, new ResourceEffect(true, "P"), 3) },
                { Board.Wonder.Ephesos_B, new Board(ExpansionSet.Original, Board.Wonder.Ephesos_A, "Ephesos (B)", CardId.Ephesos_B_Board, new ResourceEffect(true, "P"), 3) },
                { Board.Wonder.Giza_A, new Board(ExpansionSet.Original, Board.Wonder.Giza_B, "Giza (A)", CardId.Giza_A_Board, new ResourceEffect(true, "S"), 3) },
                { Board.Wonder.Giza_B, new Board(ExpansionSet.Original, Board.Wonder.Giza_A, "Giza (B)", CardId.Giza_B_Board, new ResourceEffect(true, "S"), 4) },
                { Board.Wonder.Halikarnassos_A, new Board(ExpansionSet.Original, Board.Wonder.Halikarnassos_B, "Halikarnassos (A)", CardId.Halikarnassos_A_Board, new ResourceEffect(true, "C"), 3) },
                { Board.Wonder.Halikarnassos_B, new Board(ExpansionSet.Original, Board.Wonder.Halikarnassos_A, "Halikarnassos (B)", CardId.Halikarnassos_B_Board, new ResourceEffect(true, "C"), 3) },
                { Board.Wonder.Olympia_A, new Board(ExpansionSet.Original, Board.Wonder.Olympia_B, "Olympia (A)", CardId.Olympia_A_Board, new ResourceEffect(true, "W"), 3) },
                { Board.Wonder.Olympia_B, new Board(ExpansionSet.Original, Board.Wonder.Olympia_A, "Olympia (B)", CardId.Olympia_B_Board, new ResourceEffect(true, "W"), 3) },
                { Board.Wonder.Rhodos_A, new Board(ExpansionSet.Original, Board.Wonder.Rhodos_B, "Rhodos (A)", CardId.Rhodos_A_Board, new ResourceEffect(true, "O"), 3) },
                { Board.Wonder.Rhodos_B, new Board(ExpansionSet.Original, Board.Wonder.Rhodos_A, "Rhodos (B)", CardId.Rhodos_B_Board, new ResourceEffect(true, "O"), 2) },
            };

            if (gmCoordinator.leadersEnabled)
            {
                board.Add(Board.Wonder.Roma_A, new Board(ExpansionSet.Leaders, Board.Wonder.Roma_B, "Roma (A)", CardId.Roma_A_Board, new FreeLeadersEffect(), 2));
                board.Add(Board.Wonder.Roma_B, new Board(ExpansionSet.Leaders, Board.Wonder.Roma_A, "Roma (B)", CardId.Roma_B_Board, null, 3));
            }

            // Take the board effects from the card list.

            foreach (Board b in board.Values)
            {
                if (b.expansionSet == ExpansionSet.Leaders && !gmCoordinator.leadersEnabled)
                    continue;

                if (b.expansionSet == ExpansionSet.Cities && !gmCoordinator.citiesEnabled)
                    continue;

                b.stageCard = new List<Card>(b.numOfStages);

                for (int i = 0; i < b.numOfStages; ++i)
                {
                    CardId wonderStageName = Card.CardNameFromStringName(b.name, i + 1);
                    Card card = fullCardList.Find(c => c.Id == wonderStageName);

                    fullCardList.Remove(card);
                    b.stageCard.Add(card);
                }
            }
        }

        /// <summary>
        /// Return a random board, popping it from the array of Boards initialially created in createBoards(String filename)
        /// </summary>
        /// <returns></returns>
        protected Board popRandomBoard()
        {
            // int index = (new Random()).Next(0, board.Count);
            int index = 0;

            KeyValuePair<Board.Wonder, Board> randomBoard = board.ElementAt(index);

            while(board[randomBoard.Key].inPlay == true)
            {
                ++index;

                if (index >= board.Count)
                    index = 0;

                randomBoard = board.ElementAt(index);
            }

            // Remove the other side (i.e. if we returned the Babylon A, remove Babylon B from
            // the board list)
            board[randomBoard.Key].inPlay = true;
            board[randomBoard.Value.otherSide].inPlay = true;

            return randomBoard.Value;
        }

        public void buildStructureFromHand(string playerNickname, string cardName, string strWonderStage, string strFreeBuild, string strLeftCoins, string strRightCoins, string strUsedBilkis)
        {
            Player p = player[playerNickname];

            CardId cName = Card.CardNameFromStringName(cardName);

            Card c = null;
            if (phase == GamePhase.LeaderRecruitment || phase == GamePhase.RomaB)
            {
                c = p.draftedLeaders.Find(x => x.Id == cName);
            }
            else
            {
                c = p.hand.Find(x => x.Id == cName);
            }

            if (c == null)
                throw new Exception("Received a message from the client to build a card that wasn't in the player's hand.");

            int nLeftCoins = 0, nRightCoins = 0;

            if (strLeftCoins != null)
                nLeftCoins = int.Parse(strLeftCoins);

            if (strRightCoins != null)
                nRightCoins = int.Parse(strRightCoins);

            bool freeBuild = strFreeBuild != null && strFreeBuild == "True";

            bool usedBilkis = strUsedBilkis != null && strUsedBilkis == "True";

            buildStructureFromHand(p, c, strWonderStage == "True", freeBuild, nLeftCoins, nRightCoins, usedBilkis);
        }

        /// <summary>
        /// build a structure from hand, given the Card id number and the Player
        /// </summary>
        public void buildStructureFromHand(Player p, Card c, bool wonderStage, bool freeBuild = false, int nLeftCoins = 0, int nRightCoins = 0, bool usedBilkis = false)
        {
            if (phase == GamePhase.LeaderRecruitment || phase == GamePhase.RomaB)
            {
                p.draftedLeaders.Remove(c);
            }
            else
            {
                p.hand.Remove(c);
            }

            if (phase == GamePhase.LeaderDraft)
            {
                p.draftedLeaders.Add(c);
                return;
            }

            if (wonderStage)
            {
                if (p.currentStageOfWonder >= p.playerBoard.numOfStages)
                {
                    //Player is attempting to build a Stage of Wonder when he has already built all of the Wonders. Something is wrong. This should never be reached.
                    throw new Exception("GameManager.buildStageOfWonder(Player p) error");
                }

                c = p.playerBoard.stageCard[p.currentStageOfWonder];
                p.currentStageOfWonder++;

                // if they just built the 2nd stage of Babylon B's wonder, update the player state.
                if (p.playerBoard.name == "Babylon (B)" && p.currentStageOfWonder == 2)
                    p.babylonPowerEnabled = true;
            }

            if (p.hand.Count == 1 && !p.babylonPowerEnabled)
            {
                // discard the last card in the hand, unless the player is Babylon (B) and their Power is enabled (the 2nd wonder stage)
                discardPile.Add(p.hand.First());
                p.hand.Clear();
            }

            //add the card to played card structure
            p.addPlayedCardStructure(c);

            //store the card's action
            p.storeCardEffect(c);

            if (freeBuild)
            {
                // check that the player
                if (p.olympiaPowerAvailable)
                {
                    p.olympiaPowerAvailable = false;
                }
                else
                {
                    // the player is cheating
                    throw new Exception("You do not have the ability to build a free structure");
                }
            }

            //if the structure played costs money, deduct it
            //check if the Card costs money and add the coins paid to the neighbors for their resources
            int costInCoins = c.cost.coin + nLeftCoins + nRightCoins + (usedBilkis ? 1 : 0);

            if (c.structureType == StructureType.Leader)
            {
                if (p.playerBoard.name == "Roma (A)" || p.playedStructure.Exists(x => x.Id == CardId.Maecenas))
                {
                    costInCoins = 0;
                }
                else if (p.playerBoard.name == "Roma (B)")
                {
                    costInCoins = Math.Max(0, costInCoins - 2);
                }
                else if (p.leftNeighbour.playerBoard.name == "Roma (B)" || p.rightNeighbour.playerBoard.name == "Roma (B)")
                {
                    costInCoins -= 1;
                }
            }

#if FALSE
            //if player has Rome B, then playing leaders will refund a 2 coin discount
            if (/*p.playerBoard.freeResource == 'd' && */c.structureType == StructureType.Leader)
            {
                //give 2 coins back if the card cost more than 2
                //else give less than 2 coins back
                int coins = c.cost.coin;
                if (c.cost.coin >= 2)
                {
                    coins = 2;
                    // p.storeAction("12$");
                }
                else
                {
                    // JDF.  Not sure this is correct.  Will need to test it.
                    // p.storeAction("1" + c.cost.Length + "$");
                }
                p.storeAction(new CoinEffect(coins));
            }
#endif

            /*
            //if player's neighbour has Rome B, then refund a 1 coin discount instead
            else if ((p.leftNeighbour.playerBoard.freeResource == 'd' || p.rightNeighbour.playerBoard.freeResource == 'd') && c.structureType == StructureType.Leader)
            {
                if (c.cost.coin >= 1)
                {
                    p.storeAction(new SimpleEffect(1, '$'));
                    // p.storeAction("11$");
                }
            }
            */

            if (costInCoins != 0)
            {
                p.addTransaction(-costInCoins);
            }

            if (nLeftCoins != 0)
                p.leftNeighbour.addTransaction(nLeftCoins);

            if (nRightCoins != 0)
                p.rightNeighbour.addTransaction(nRightCoins);

            //determine if the player should get 2 coins for having those leaders (get 2 coins for playing a yellow and playing a pre-req
            giveCoinFromLeadersOnBuild(p, c);
        }

        /// <summary>
        /// Determines if 2 coins should be given for playing a card. Give 2 coins when the appropriate leader was played.
        /// </summary>
        private void giveCoinFromLeadersOnBuild(Player p, Card c)
        {
            /*
            for (int i = 0; i < p.numOfPlayedCards; i++)
            {
                //235 - 2 coin for yellow card played
                if (p.playedStructure[i].id == 235)
                {
                    if (c.colour == "Yellow")
                    {
                        p.storeAction("12$");
                        break;
                    }
                }
            }

            for (int i = 0; i < p.numOfPlayedCards; i++)
            {
                //234 - 2 coins if played from a pre-requisite
                if (p.playedStructure[i].id == 234 && c.id != 234)
                {
                    for (int j = 0; j < p.numOfPlayedCards; j++)
                    {
                        if (c.freePreq == p.playedStructure[j].name)
                        {
                            p.storeAction("12$");
                            return;
                        }
                    }
                }
            }
            */
        }

#if FALSE
        /// <summary>
        /// Player finishes conducting commerce to pay for a card
        /// </summary>
        /// <param name="nickname"></param>
        /// <param name="commerceInformation"></param>
        public void buildStructureFromCommerce(string nickname, string structureName, int leftcoins, int rightcoins)//string commerceInformation)
        {
            Player p = player[nickname];

            /*
            CommerceClientToServerResponse response = (CommerceClientToServerResponse)Marshaller.StringToObject(commerceInformation);

            string structureName = response.structureName;
            int leftcoins = response.leftCoins;
            int rightcoins = response.rightCoins;
            */

            //Find the card with the id number
            Card c = p.hand.Find(x => x.name == structureName);

            if (c == null)
                throw new Exception("Unexpected card name");

            p.hand.Remove(c);

            //add the card to played card structure
            p.addPlayedCardStructure(c);
            //store the card's action
            p.storeAction(c.effect);

            //charge the player the appropriate amount of coins
            int commerceCost = leftcoins + rightcoins;

            //store the deduction
            // p.storeAction("$" + commerceCost);
            p.storeAction(new CostEffect(commerceCost));

            //give the coins that neigbours earned from commerce
            // p.leftNeighbour.storeAction("1" + leftcoins + "$");
            // p.rightNeighbour.storeAction("1" + rightcoins + "$");
            p.leftNeighbour.storeAction(new SimpleEffect(leftcoins, '$'));
            p.rightNeighbour.storeAction(new SimpleEffect(rightcoins, '$'));


            //determine if the player should get 2 coins for having those leaders (get 2 coins for playing a yellow and playing a pre-req
            giveCoinFromLeadersOnBuild(p, c);

            //Leaders: if Player has card 209 (gain 1 coin for using commerce per neighbouring player)
            //then gain 1 coin
            /*
            if (this is LeadersGameManager)
            {
                if (p.hasIDPlayed(209))
                {
                    if (leftcoins != 0) p.storeAction("11$");
                    if (rightcoins != 0) p.storeAction("11$");
                }
            }
            */
        }

        /// <summary>
        /// Player finishes conducting commerce to pay for a stage of wonder
        /// </summary>
        /// <param name="nickname"></param>
        /// <param name="information"></param>
        public virtual void buildStageOfWonderFromCommerce(string nickname, string structureName, int leftcoins, int rightcoins)
        {
            Player p = player[nickname];

            /*
            CommerceClientToServerResponse response = (CommerceClientToServerResponse)Marshaller.StringToObject(information);

            int leftcoins = response.leftCoins;
            int rightcoins = response.rightCoins;
            string structureName = response.structureName;
            */

            //build the stage of wonder
            buildStageOfWonder(structureName, nickname);

            //charge the player the appropriate amount of coins
            int commerceCost = leftcoins + rightcoins;

            //store the deduction
            // p.storeAction("$" + commerceCost);
            p.storeAction(new CostEffect(commerceCost));

            //give the coins that neigbours earned from commerce
            // p.leftNeighbour.storeAction("1" + leftcoins + "$");
            // p.rightNeighbour.storeAction("1" + rightcoins + "$");
            p.leftNeighbour.storeAction(new SimpleEffect(leftcoins, '$'));
            p.rightNeighbour.storeAction(new SimpleEffect(rightcoins, '$'));

            //Leaders: if Player has card 209 (gain 1 coin for using commerce per neighbouring player)
            //then gain 1 coin
            /*
            if (this is LeadersGameManager)
            {
                if (p.hasIDPlayed(209))
                {
                    if (leftcoins != 0) p.storeAction("11$");
                    if (rightcoins != 0) p.storeAction("11$");
                }
            }
            */
        }

        /// <summary>
        /// build a structure from a card in discard pile, given the Card id number and the Player
        /// </summary>
        /// <param name="id"></param>
        /// <param name="p"></param>
        public void buildStructureFromDiscardPile(string name, string playerNickname)
        {
            //Find the Player object given the playerNickname
            Player p = player[playerNickname];

            //if the structure costed money, reimburse the money
            //check if the Card costs money
            int costInCoins = 0;

            throw new NotImplementedException();

            for (int i = 0; i < discardPile.Count; i++)
            {
                //found the card
                if (discardPile[i].name == name)
                {
                    // I think the loop below shoudl be replaced with:
                    costInCoins = discardPile[i].cost.coin;

                    // TODO: figure this out.  Not really sure what they were doing before.
                    throw new NotImplementedException();

                    /*
                    //count how many $ signs in the cost. Each $ means 1 coin cost
                    for (int j = 0; j < discardPile[i].cost.Length; j++)
                    {
                        if (discardPile[i].cost[j] == '$')
                        {
                            costInCoins++;
                        }
                    }
                    */

                    break;
                }
            }

            //store the reimbursement
            p.storeAction(new CoinEffect(costInCoins));

            //Find the card with the id number
            Card c = discardPile.Find(x => x.name == name);
            discardPile.Remove(c);

            //add the card to played card structure
            p.addPlayedCardStructure(c);
            //store the card's action
            p.storeAction(c.effect);

            //determine if the player should get 2 coins for having those leaders (get 2 coins for playing a yellow and playing a pre-req
            giveCoinFromLeadersOnBuild(p, c);
        }
#endif
        public void discardCardForThreeCoins(string nickname, string name)
        {
            Player p = player[nickname];

            discardCardForThreeCoins(p, p.hand.Find(x => x.Id == Card.CardNameFromStringName(name)));
        }

        /// <summary>
        /// discard a given card's id for three coins
        /// </summary>
        /// <param name="id"></param>
        /// <param name="p"></param>
        public void discardCardForThreeCoins(Player p, Card c)
        {
            p.addTransaction(3);

            //Find the card with the id number and find its effects
            p.hand.Remove(c);

            //add the card to the discard pile
            discardPile.Add(c);

            if (p.hand.Count == 1)
            {
                // discard the unplayed card, unless the player is Babylon (B) and their Power is enabled (the 2nd wonder stage)
                if (!p.babylonPowerEnabled)
                {
                    discardPile.Add(p.hand.First());
                    p.hand.Clear();
                }
            }
        }



        /// <summary>
        /// Pass remaining cards to neighbour
        /// </summary>
        public void passRemainingCardsToNeighbour()
        {
            Player p = player.Values.First();
            List<Card> p1hand = p.hand;

            do
            {
                if (currentAge % 2 == 1)
                {
                    // First and third age the cards are passed to each player's left neighbor
                    p.hand = p.rightNeighbour.hand;
                    p = p.rightNeighbour;
                }
                else
                {
                    p.hand = p.leftNeighbour.hand;
                    p = p.leftNeighbour;
                }

            } while (p != player.Values.First());

            if (currentAge % 2 == 1)
            {
                p.leftNeighbour.hand = p1hand;
            }
            else
            {
                p.rightNeighbour.hand = p1hand;
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
        //Utility functions

        /// <summary>
        /// Execute the Action of All players
        /// </summary>
        public void executeActionsAtEndOfTurn()
        {
            if (!gettingBabylonExtraCard && !playingCardFromDiscardPile && phase != GamePhase.RomaB)
            {
                //make AI moves
                executeAIActions();
            }

            //execute the Actions for each players
            foreach (Player p in player.Values)
            {
                p.executeAction(this);
            }
        }

        private void executeAIActions()
        {
            foreach (Player p in player.Values)
            {
                if (p.isAI)
                {
                    p.makeMove(this);
                }
            }
        }

        /// <summary>
        /// Send the main display information for all players
        /// This is called at the beginning of the game and after each turn
        /// </summary>
        public virtual void updateAllGameUI()
        {
            string strCardsPlayed = "CardPlay";
            string strUpdateCoinsMessage = "SetCoins";

            foreach (Player p in player.Values)
            {
                if (p.bUIRequiresUpdating)
                {
                    // TODO: update this to send built Wonder stage updates as well as the cards played panel.
                    Card card = p.playedStructure.Last();

                    if (card.structureType == StructureType.WonderStage)
                    {
                        strCardsPlayed += string.Format("&{0}=WonderStage{1}", p.nickname, card.wonderStage);
                    }
                    else
                    {
                        strCardsPlayed += string.Format("&{0}={1}", p.nickname, card.Id);
                    }
                    p.bUIRequiresUpdating = false;
                }
                else
                {
                    strCardsPlayed += string.Format("&{0}={1}", p.nickname, "Discarded");
                }

                strUpdateCoinsMessage += string.Format("&{0}={1}", p.nickname, p.coin);
            }

            foreach (Player p in player.Values)
            {
                //Update the Player Bar Panel
                //send the playerBarPanel information
                // Replaced with "SetCoins" message
                // gmCoordinator.sendMessage(p, "B" + Marshaller.ObjectToString(new PlayerBarInformation(player)));
                //send the current stage of wonder information and tell it to start up the timer
                // gmCoordinator.sendMessage(p, "s" + p.currentStageOfWonder);

                /*
                //if player has Bilkis AND has at least 1 coin, then send the message to enable Bilkis button
                if (p.hasBilkis && p.coin > 0)
                {
                    //EB = Enable Bilkis
                    gmCoordinator.sendMessage(p, "EB");
                }

                //send current turn information
                gmCoordinator.sendMessage(p, "T" + currentTurn);
                */

                if (phase == GamePhase.LeaderDraft)
                {
                    string strLeaderHand = "LdrDraft";

                    foreach (Card card in p.hand)
                    {
                        strLeaderHand += string.Format("&{0}=", card.Id);
                    }

                    // send the list of cards to the player
                    gmCoordinator.sendMessage(p, strLeaderHand);
                }
                else if (phase == GamePhase.LeaderRecruitment || (phase == GamePhase.RomaB && p.draftingExtraLeader))
                {
                    string strLeaderIcons = "LeadrIcn";

                    foreach (Card c in p.draftedLeaders)
                    {
                        strLeaderIcons += string.Format("&{0}=", c.Id);
                    }

                    gmCoordinator.sendMessage(p, strLeaderIcons);
                    gmCoordinator.sendMessage(p, strCardsPlayed);
                    gmCoordinator.sendMessage(p, strUpdateCoinsMessage);

                    string strHand = "SetPlyrH";

                    foreach (Card card in p.draftedLeaders)
                    {
                        strHand += string.Format("&{0}={1}", card.Id, p.isCardBuildable(card).ToString());
                    }

                    strHand += string.Format("&WonderStage{0}={1}", p.currentStageOfWonder, p.isStageBuildable().ToString());

                    strHand += "&Instructions=Leader Recruitment: choose a leader to play, build a wonder stage with, or discard for 3 coins";

                    gmCoordinator.sendMessage(p, strHand);
                }
                else
                {
                    gmCoordinator.sendMessage(p, strCardsPlayed);
                    gmCoordinator.sendMessage(p, strUpdateCoinsMessage);

                    // Check if we're in a special state - extra turn for Babylon (B)
                    if (gettingBabylonExtraCard && !p.babylonPowerEnabled)
                        continue;

                    // Check if we're in a special state - playing a card from the discard pile
                    // only the player (or players) who are getting the extra turn can proceed here.
                    if (playingCardFromDiscardPile && !p.playCardFromDiscardPile)
                        continue;

                    //send the hand panel (action information) for regular ages (not the Recruitment phase i.e. Age 0)
                    string strHand = "SetPlyrH";

                    if (playingCardFromDiscardPile)
                    {
                        savedHandWhenPlayingFromDiscardPile = p.hand;   // save the player's hand
                        p.hand = discardPile;                           // the player's hand now points to the discard pile.

                        foreach (Card card in discardPile)
                        {
                            // Filter out structures that have already been built in the players' city.
                            if (p.isCardBuildable(card) != Buildable.StructureAlreadyBuilt)
                                strHand += string.Format("&{0}={1}", card.Id, Buildable.True.ToString());
                        }

                        // The free build for Halikarnassos/Solomon requires the card be put in play.
                        // It cannot be used to build a wonder stage, nor can it be discarded for 3
                        // coins.
                        strHand += string.Format("&WonderStage{0}={1}&Instructions=Choose a card to play for free from the discard pile&CanDiscard=False", p.currentStageOfWonder, Buildable.InsufficientResources.ToString());
                    }
                    else
                    {
                        foreach (Card card in p.hand)
                        {
                            strHand += string.Format("&{0}={1}", card.Id, p.isCardBuildable(card).ToString());
                        }

                        strHand += string.Format("&WonderStage{0}={1}", p.currentStageOfWonder, p.isStageBuildable().ToString());

                        if (gettingBabylonExtraCard)
                        // if (phase == GamePhase.Babylon)
                        {
                            strHand += "&Instructions=Babylon: you may build the last card in your hand or discard it for 3 coins";
                        }
                        else
                        {
                            strHand += "&Instructions=Choose a card from the list below to play, build a wonder stage with, or discard";
                        }
                    }

                    gmCoordinator.sendMessage(p, strHand);
                }

                //send the timer signal if the current Age is less than 4 (i.e. game is still going)
                if (gameConcluded == false)
                {
                    gmCoordinator.sendMessage(p, "t");
                }
                else
                {
                    gmCoordinator.sendMessage(p, "e");
                }
            }

            if (gameConcluded)
                endOfSessionActions();
        }

        string BuildResourceString(string who, Player plyr, bool isSelf, bool hasDiscountEffect, bool hasBilkis)
        {
            string strRet = string.Format("&{0}Resources=", who);

            foreach (Effect e in plyr.dag.getResourceList(isSelf))
            {
                ResourceEffect se = e as ResourceEffect;

                strRet += se.resourceTypes + ",";
            }

            if (hasDiscountEffect)
            {
                // add a wild card effect, which has a choice of all 7 resources.  This is free
                // to use, but can only be used by that type of structure (Wonder/Civilian/Military/Science)
                strRet += "WSBOCGP,";
            }

            if (hasBilkis)
            {
                // add a wild card effect for Bilkis, which has a choice of all 7 resources, but costs
                // 1 coin to use.
                strRet += "WSBOCGP,";
            }

            // remove the trailing comma, if necessary
            if (strRet.EndsWith(","))
                strRet = strRet.Remove(strRet.Length - 1);

            return strRet;
        }

        /// <summary>
        /// Server to Client message consisting of Commerce UI updates
        /// </summary>
        /// <param name="id"></param>
        /// <param name="nickname"></param>
        public void updateCommercePanel(string nickname, string structureName, string wonderStage)
        {
            Player p = player[nickname];

            // Leaders who give discounts on certain card types:
            // "Imhotep" (Wonder stages)
            // "Archimedes" (Science cards)
            // "Hammurabi" (Civilian)
            // "Leonidas" (Military)

            // old structure included structure name, cost, whether a Wonder stage is being constructed, and leader discount
            // I'm going to skip the name and whether it is a wonder stage (client already knows these) and the cost (client knows all card info) and the leaders, for now.

            string strCommerce = "CommData";

            strCommerce += string.Format("&coin={0}", p.coin);
            strCommerce += string.Format("&resourceDiscount={0}", p.rawMaterialsDiscount.ToString());
            strCommerce += string.Format("&goodsDiscount={0}", p.goodsDiscount.ToString());

            if (wonderStage == "True")
                strCommerce += string.Format("&wonderCard={0}", Card.CardNameFromStringName(p.playerBoard.name, p.currentStageOfWonder+1));

            strCommerce += string.Format("&Structure={0}&WonderStage={1}", structureName, wonderStage);

            Card card = null;

            if (wonderStage == "True")
            {
                card = p.playerBoard.stageCard[p.currentStageOfWonder];
            }
            else
            {
                card = fullCardList.Find(y => y.Id == (CardId)Enum.Parse(typeof(CardId), structureName));
            }

            bool hasDisountEffect = p.playedStructure.Exists(x => x.effect is StructureDiscountEffect && ((StructureDiscountEffect)x.effect).discountedStructureType == card.structureType);

            Card bilkis = p.playedStructure.Find(x => x.Id == CardId.Bilkis);
            if (bilkis != null)
            {
                // Tell the commmerce window that the last entry in the resource list for the player is for Bilkis
                // and isn't due to another leader effect.
                strCommerce += string.Format("&Bilkis=");
            }

            strCommerce += BuildResourceString("Player", p, true, hasDisountEffect, bilkis != null);
            strCommerce += BuildResourceString("Left", p.leftNeighbour, false, false, false);
            strCommerce += BuildResourceString("Right", p.rightNeighbour, false, false, false);

            gmCoordinator.sendMessage(p, strCommerce);
        }

        protected int numOfPlayersThatHaveTakenTheirTurn = 0;

        /*
         * Player sends "t" message (turn complete) to the GameManager
         * Increment the amount of players that have taken their turn
         * If it is equal to the number of players, then go to the next turn
         */
        public virtual void turnTaken(String nickname)
        {
            Player p = player[nickname];

            numOfPlayersThatHaveTakenTheirTurn++;

            if ((numOfPlayersThatHaveTakenTheirTurn == numOfPlayers) || gettingBabylonExtraCard || playingCardFromDiscardPile || phase == GamePhase.RomaB)
            {
                //reset the number of players that have taken their turn
                numOfPlayersThatHaveTakenTheirTurn = 0;

                //execute every player's action
                    // any other turn, execute everyone's actions.
                executeActionsAtEndOfTurn();

                if (gettingBabylonExtraCard && currentTurn == 6 && p.babylonPowerEnabled)
                {
                    gettingBabylonExtraCard = false;
                }
                else if (currentTurn == 6 && p.babylonPowerEnabled)
                {
                    gettingBabylonExtraCard = true;
                }

                // I will need to go through this logic carefully.  Babylon (B) must play or discard their last
                // card _before_ Halikarnassos looks at the discard pile.  Will need to connect a 2nd client first,
                // though.  Basically the GameManager has to get Babylon's choice before Halikarnassos can choose
                // a card from the discard pile.  Note.  I _think_ I may be able to just add an "else" before the
                // "if" here.  That way the game manager will do the Babylon extra card first, and only after that's
                // done, do Halikarnassos.  Or maybe I'll need to add a game manager state to control this, rather
                // than using booleans to control the logic.
                if (playingCardFromDiscardPile && p.playCardFromDiscardPile)
                {
                    playingCardFromDiscardPile = false;
                    p.playCardFromDiscardPile = false;

                    // restore the player's original hand
                    p.hand = savedHandWhenPlayingFromDiscardPile;
                    savedHandWhenPlayingFromDiscardPile = null;
                }
                else if (p.playCardFromDiscardPile)
                {
                    playingCardFromDiscardPile = true;
                }

                if (phase == GamePhase.RomaB && p.draftingExtraLeader)
                {
                    phase = GamePhase.Playing;
                    p.draftingExtraLeader = false;
                }
                else if (p.draftingExtraLeader)
                {
                    phase = GamePhase.RomaB;
                }

                //all players have completed their turn
                if (!gettingBabylonExtraCard && !playingCardFromDiscardPile && phase != GamePhase.RomaB)
                {
                    // gettingBabylonExtraCard = false;
                    // playingCardFromDiscardPile = false;

                    switch (phase)
                    {
                        case GamePhase.LeaderDraft:
                            passRemainingCardsToNeighbour();

                            currentTurn++;

                            if (currentTurn == 5)
                            {
                                phase = GamePhase.LeaderRecruitment;
                                currentAge = 1;
                                currentTurn = 1;
                                dealDeck(currentAge);
                            }
                            break;

                        case GamePhase.LeaderRecruitment:
                            phase = GamePhase.Playing;
                            break;

                        case GamePhase.Playing:
                            passRemainingCardsToNeighbour();

                            currentTurn++;

                            //if the current turn is last turn of Age, do end of Age calculation
                            if (currentTurn == 7)
                            {
                                //perform end of Age actions
                                endOfAgeActions();
                                currentAge++;

                                //if game hasn't ended, then deal deck, reset turn
                                if (currentAge < 4)
                                {
                                    dealDeck(currentAge);
                                    if (gmCoordinator.leadersEnabled)
                                        phase = GamePhase.LeaderRecruitment;
                                    currentTurn = 1;
                                }
                                //else do end of session actions
                                else
                                {
                                    gameConcluded = true;
                                }
                            }
                            break;
                    }
                }

                updateAllGameUI();
            }
        }

        /*
        /// <summary>
        /// A "virtual" function that is of no use to GameManager, and is meant for Leaders to implement
        /// Added here so that GMCoordinator is able to call the Leader's version of it.
        /// Program won't compile without the base class having this.
        /// </summary>
        /// <param name="nickname"></param>
        /// <param name="id"></param>
        public virtual void recruitLeader(String nickname, int id)
        {
            throw new NotImplementedException();
        }

        public virtual void bilkisPower(String nickname, byte resource)
        {
            throw new NotImplementedException();
        }

        public virtual void playLeaderForFreeWithRome(string nickname, string message)
        {
            throw new NotImplementedException();
        }

        public virtual void playCourtesansGuild(string nickname, string message)
        {
            throw new NotImplementedException();
        }
        */
    }
}
