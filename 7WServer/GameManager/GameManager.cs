using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace SevenWonders
{
    public class GameManager
    {
        public int numOfPlayers { get; set; }
        public int numOfAI { get; set; }

        public GMCoordinator gmCoordinator;

        public int currentAge;

        public int currentTurn;

        public int nTurnsInEachAge;

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

        string[] playerNicks;

        public bool gameConcluded { get; set; }

        public GamePhase phase { get; private set; }

        // This is needed in case a recruited leader is used to build a wonder stage, which could then be used to do something else (e.g.
        // play a card from the discard pile.  In that case, when we transition from to Roma B to Solomon, we should NOT update the previous
        // phase.  i.e. only update the prevPhase to a non-special phase.  Non-special phases are LeaderDraft, LeaderRecruitment, and
        // playing.  All other phases are special.
        private GamePhase prevPhase;

        private Random rnd;

        /// <summary>
        /// Shared constructor for GameManager and LeadersGameManager
        /// Common begin of game tasks that are shared amongst all versions of 7W
        /// </summary>
        /// <param name="gmCoordinator"></param>
        public GameManager(GMCoordinator gmCoordinator, List<PlayerInfo> players)
        {
            this.gmCoordinator = gmCoordinator;

            // set the maximum number of players in the game to numOfPlayers + numOfAI
            // this.numOfPlayers = numOfPlayers;
            this.numOfPlayers = players.Where(x => x.isAI == false).Count();
            // this.numOfAI = numOfAI;
            this.numOfAI = players.Where(x => x.isAI == true).Count();
            // this.playerNicks = playerNicks;
            this.playerNicks = new string[players.Count];
            for (int i = 0; i < players.Count; ++i)
            {
                playerNicks[i] = players[i].name;
                player.Add(playerNicks[i], new Player(playerNicks[i], players[i].isAI, this));
            }

            gameConcluded = false;

            // If the Leaders expansion pack is enabled, start with age 0 (leaders draft)
            currentAge = gmCoordinator.leadersEnabled ? 0 : 1;
            currentTurn = 1;

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
            {
                // Remove all cards which are included with the Leaders expansion pack or 
                // the Cities expansion pack but are Leaders (e.g. Darius, Aspasia, etc).
                fullCardList.RemoveAll(x => x.expansion == ExpansionSet.Leaders || x.structureType == StructureType.Leader);
            }

            //initialize the vanilla boards objects
            //does not assign the boards to players yet
            createBoards();

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

            // set the number of turns for each age.  If playing with the Cities expansion, 
            nTurnsInEachAge = gmCoordinator.citiesEnabled ? 7 : 6;

            rnd = new Random();
        }

        /*
        /// <summary>
        /// Creating a vanilla AI controlled Player class
        /// </summary>
        /// <param name="name"></param>
        protected Player createAI(String name, char strategy)
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
        */

        ///////////////////////////////////////////////////////////////////////////////////////////////////////



        public void sendBoardNames()
        {
            string strMsg = "SetBoard";

            foreach (Player p in player.Values)
            {
                strMsg += string.Format("&{0}={1}/{2}", p.nickname, p.playerBoard.numOfStages, p.playerBoard.name);
            }

            gmCoordinator.SendMessageToAll(strMsg);

            foreach (Player p in player.Values)
            {
                p.executeAction();
            }
        }

        /// <summary>
        /// Beginning of Session Actions for Vanilla game
        /// 1) Distribute a random board and 3 coins to all
        /// 2) Remove from all decks card that will not be used in the game
        /// </summary>
        /// <param name="numOfPlayers + numOfAI"></param>
        /// 
        public void beginningOfSessionActions()
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

            sendBoardNames();

            for (int i = 0; i < 4; i++)
            {
                //deck[1] is age 1. deck[2] is age 2 ....
                deckList.Add(new Deck(fullCardList, i, numOfAI + numOfPlayers));
            }

            if (gmCoordinator.citiesEnabled)
            {
                for (int i = 1; i <= 3; ++i)
                {
                    deckList[i].removeCityCards(9 - (numOfPlayers + numOfAI));
                }
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
            resetDiplomacyState();

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

            gmCoordinator.SendMessageToAll(strUpdateMilitaryTokens);

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
                if (p.diplomacyEnabled)
                {
                    // skip this if we played a diplomacy card this age.
                    continue;
                }

                Player playerToCompare = p.rightNeighbour;

                while (playerToCompare.diplomacyEnabled)
                {
                    // find the first neighbor to the right who hasn't played diplomacy.
                    playerToCompare = playerToCompare.rightNeighbour;
                }

                if (playerToCompare == p)
                {
                    // all other players have played diplomacy for this age.
                    continue;
                }

                //if the current player's shield is greater than the next person, increase conflicttoken by the appropriate age
                //if less, get a losstoken
                if (p.shield > playerToCompare.shield &&
                    playerToCompare != p.leftNeighbour // If this condition is true, only two players are fighting this age.  The winner gets one win, the loser gets one loss.  We only have to distribute the win once.
                    )
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

                    if (p.playedStructure.Exists(x => x.Id == CardId.Nero))
                    {
                        // Nero grants 2 coins for each Victory token earned by the player from this point forward. These coins are taken from the bank when the Victory tokens are gained.
                        // Bug: this transaction needs to be executed immediately
                        p.addTransaction(2);
                        p.executeAction();
                    }

                    //check if right neighbour has played Tomyris: return conflict loss token received
                    //if no, receive lossToken
                    //if yes, do not get lossToken, instead, give lossToken to winner
                    if (playerToCompare.playedStructure.Exists(x => x.Id == CardId.Tomyris))
                    {
                        //the loser is rightNeighbour
                        //the winner is current player. current player will get the loss token
                        p.lossToken++;
                    }
                    else
                    {
                        playerToCompare.lossToken++;
                    }
                }
                else if (p.shield < playerToCompare.shield &&
                    playerToCompare != p.leftNeighbour // If this condition is true, only two players are fighting this age.  The winner gets one win, the loser gets one loss.  We only have to distribute the win once.
                    )
                {
                    if (currentAge == 1)
                    {
                        playerToCompare.conflictTokenOne += 1;
                    }
                    else if (currentAge == 2)
                    {
                        playerToCompare.conflictTokenTwo += 1;
                    }
                    else if (currentAge == 3)
                    {
                        playerToCompare.conflictTokenThree += 1;
                    }

                    if (playerToCompare.playedStructure.Exists(x => x.Id == CardId.Nero))
                    {
                        // Nero grants 2 coins for each Victory token earned by the player from this point forward. These coins are taken from the bank when the Victory tokens are gained.
                        // Bug: this transaction needs to be executed immediately
                        playerToCompare.addTransaction(2);
                        playerToCompare.executeAction();
                    }

                    if (p.playedStructure.Exists(x => x.Id == CardId.Tomyris))
                    {
                        //the loser is rightNeighbour
                        //the winner is current player. current player will get the loss token
                        playerToCompare.lossToken++;
                    }
                    else
                    {
                        p.lossToken++;
                    }
                }
            }
        }

        /// <summary>
        /// Clear the diplomacy state following the military conflict resolution
        /// </summary>
        void resetDiplomacyState()
        {
            foreach (Player p in player.Values)
            {
                p.diplomacyEnabled = false;
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

                strFinalScore += string.Format("&{0}={1},{2},{3},{4},{5},{6},{7},{8},{9},{10}",
                    s.Key, sc.military, sc.coins, sc.wonders, sc.civilian, sc.commerce, sc.guilds, sc.science, sc.leaders, sc.cities, sc.Total());
            }

            gmCoordinator.SendMessageToAll(strFinalScore);
        }

        ///////////////////////////////////////////////////////////////////////////////////////


        /// <summary>
        /// Shuffle and Deal a deck to all Players
        /// </summary>
        /// <param name="d"></param>
        protected void dealDeck(int currentAge)
        {
            //shuffle the deck
            Deck deck = deckList[currentAge];

            deck.shuffle();

            int numCardsToDeal = currentAge == 0 ? 4 : gmCoordinator.citiesEnabled ? 8 : 7;

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

            /*
            Disabled temporarily until I get around to scanning images of these boards.
            if (gmCoordinator.citiesEnabled)
            {
                board.Add(Board.Wonder.Petra_A, new Board(ExpansionSet.Cities, Board.Wonder.Petra_B, "Petra (A)", CardId.Petra_A_Board, new ResourceEffect(true, "B"), 3));
                board.Add(Board.Wonder.Petra_B, new Board(ExpansionSet.Cities, Board.Wonder.Petra_A, "Petra (B)", CardId.Petra_B_Board, new ResourceEffect(true, "B"), 2));
                board.Add(Board.Wonder.Byzantium_A, new Board(ExpansionSet.Cities, Board.Wonder.Byzantium_B, "Byzantium (A)", CardId.Byzantium_A_Board, new ResourceEffect(true, "S"), 3));
                board.Add(Board.Wonder.Byzantium_B, new Board(ExpansionSet.Cities, Board.Wonder.Byzantium_A, "Byzantium (B)", CardId.Byzantium_B_Board, new ResourceEffect(true, "S"), 2));
            }
            */

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
            int index = rnd.Next(0, board.Where(x => !x.Value.inPlay).Count());

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

        public void playClientCard(string playerNickname, NameValueCollection qscoll)
        {
            string strLeftCoins = qscoll["leftCoins"];
            string strRightCoins = qscoll["rightCoins"];

            Player p = player[playerNickname];

            BuildAction buildAction = (BuildAction)Enum.Parse(typeof(BuildAction), qscoll["Action"]);

            // The structure name & the age must match (structure name is not enough as Loom, Glassworks, and Press
            // have versions in Age 1 and in Age 2)
            Card c = null;
            CardId cardId = Card.CardNameFromStringName(qscoll["Structure"]);

            if (cardId == CardId.Loom || cardId == CardId.Press || cardId == CardId.Glassworks)
            {
                c = fullCardList.Find(x => x.Id == cardId && (x.age == currentAge));
            }
            else
            {
                c = fullCardList.Find(x => x.Id == cardId);
            }

            int nLeftCoins = 0, nRightCoins = 0;

            if (strLeftCoins != null)
                nLeftCoins = int.Parse(strLeftCoins);

            if (strRightCoins != null)
                nRightCoins = int.Parse(strRightCoins);

            playCard(p, c, buildAction, false, qscoll["FreeBuild"] != null,
                nLeftCoins, nRightCoins, qscoll["Bilkis"] != null);
        }

        /// <summary>
        /// build a structure from hand, given the Card id number and the Player
        /// </summary>
        public void playCard(Player p, Card c, BuildAction buildAction, bool isAI, bool freeBuild = false, int nLeftCoins = 0, int nRightCoins = 0, bool usedBilkis = false)
        {
            bool bFound = false;

            if (phase == GamePhase.LeaderRecruitment || phase == GamePhase.RomaB)
            {
                // Leader Recruitment is not a special phase, so the players' phase state is
                // not set to this value and it's valid to receive meessages in any order.

                if (phase == GamePhase.RomaB && p.phase != GamePhase.RomaB)
                    throw new Exception("RomaB: received a message from a player who is not in the RomaB phase.");

                bFound = p.draftedLeaders.Remove(c);
            }
            else if (phase == GamePhase.Halikarnassos || phase == GamePhase.Solomon)
            {
                if (phase == GamePhase.Halikarnassos && p.phase != GamePhase.Halikarnassos)
                    throw new Exception("Halikarnassos: received a message from a player who is not in the Halikarnassos phase.");

                if (phase == GamePhase.Solomon && p.phase != GamePhase.Solomon)
                    throw new Exception("Solomon: received a message from a player who is not in the Solomon phase.");

                bFound = discardPile.Remove(c);
            }
            else if (phase == GamePhase.Courtesan)
            {
                if (p.phase != GamePhase.Courtesan)
                    throw new Exception("Courtesan phase: received a message from a player who is not in the Courtesan phase.");

                bFound = p.leftNeighbour.playedStructure.Exists(x => x.Id == c.Id);

                if (!bFound)
                    bFound = p.rightNeighbour.playedStructure.Exists(x => x.Id == c.Id);
            }
            else
            {
                // Normal turn

                bFound = p.hand.Remove(c);
            }

            if (!bFound)
                throw new Exception("Invalid card play");

            if (phase == GamePhase.LeaderDraft)
            {
                p.draftedLeaders.Add(c);

                if (!isAI)
                    turnTaken();

                // Nothing else to do during the leader draft phase
                return;
            }

            if (buildAction == BuildAction.Discard)
            {
                // give 3 coins.
                p.addTransaction(3);

                if (c.structureType != StructureType.Leader)
                {
                    // add the card to the discard pile for Halikarnassos or Solomon, unless it's a leader
                    // card.  Leader cards cannot be built from the discard pile.
                    discardPile.Add(c);
                }

                // Not sure whether I need to do this or not.
                if (p.hand.Count == 1)
                {
                    // discard the unplayed card, unless the player is Babylon (B) and their Power is enabled (the 2nd wonder stage)
                    if (!p.babylonPowerEnabled)
                    {
                        discardPile.Add(p.hand.First());
                        p.hand.Clear();
                    }
                }

                if (!isAI)
                    turnTaken();

                return;
            }
            else if (buildAction == BuildAction.BuildWonderStage)
            {
                if (p.currentStageOfWonder >= p.playerBoard.numOfStages)
                {
                    //Player is attempting to build a Stage of Wonder when he has already built all of the Wonders. Something is wrong. This should never be reached.
                    throw new Exception("GameManager.buildStageOfWonder(Player p) error");
                }

                c = p.playerBoard.stageCard[p.currentStageOfWonder];
                p.currentStageOfWonder++;
            }

            // Possible TODO: move these to _after_ the coin cost has been resolved.  Maecenas has a problem where he was being played
            // for free because his effect took place _after_ the card had been added.  Need to do some testing around this to ensure
            // moving this down doesn't screw up Olympia.
            //add the card to played card structure
            p.addPlayedCardStructure(c);

            //store the card's action
            p.storeCardEffect(c);

            if (freeBuild)
            {
                // check that the player has the Olympia Power (build a card without paying its resource cost, once per age)
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

            if (phase == GamePhase.Halikarnassos || phase == GamePhase.Solomon)
            {
                // cards built from the discard pile do not have a cost associated with them
                costInCoins = 0;
            }

            if (c.structureType == StructureType.Leader)
            {
                if (p.playerBoard.name == "Roma (A)" || (p.playedStructure.Exists(x => x.Id == CardId.Maecenas) && (c.Id != CardId.Maecenas)) || phase == GamePhase.Courtesan)
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

            if (costInCoins != 0)
            {
                p.addTransaction(-costInCoins);
            }

            // Hatshepsut: Each purchase of one or more resources from  a neighbor grants 1 coin from the bank (max 2 per turn, if resources from both neighbors are used)
            bool hasHatshepsut = p.playedStructure.Exists(x => x.Id == CardId.Hatshepsut);

            // TODO: fix this for the case when a Clandestine Dock has allowed us to use a neighbor's resource without paying him anything.
            if (nLeftCoins != 0)
            {
                p.leftNeighbour.addTransaction(nLeftCoins);
                if (hasHatshepsut) p.addTransaction(1);     // Probably should log this
            }

            if (nRightCoins != 0)
            {
                p.rightNeighbour.addTransaction(nRightCoins);
                if (hasHatshepsut) p.addTransaction(1);     // Probably should log this
            }

            if ((phase == GamePhase.Babylon || phase == GamePhase.Playing) &&
                (p.playedStructure.Exists(x => x.Id == CardId.Xenophon) && (c.structureType == StructureType.Commerce)) ||
                (p.playedStructure.Exists(x => x.Id == CardId.Vitruvius) && (p.playedStructure.Exists(y => (y.chain[0] == c.strName) || (y.chain[1] == c.strName)))))
            {
                // Xenophon grants 2 coins for each yellow structure that the player builds
                // from this point forward.  The coins are taken from the bank when the structures are built.

                // Vitruvius grants 2 coins whenever the player builds a structure through
                // building chains. The coins are taken from the bank when the structures are built.

                p.addTransaction(2);        // TODO: log this.  Would be nice to show a visual representation on the screen as well.
            }

            if (!isAI)
                turnTaken();
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
            if (phase == GamePhase.LeaderDraft || phase == GamePhase.LeaderRecruitment || phase == GamePhase.Playing)
            {
                //make AI moves
                foreach (Player p in player.Values)
                {
                    if (p.isAI) p.makeMove();
                }
            }

            if (phase == GamePhase.LeaderDraft)
                return;

            //execute the Actions for each players
            foreach (Player p in player.Values)
            {
                p.executeAction();

                if (phase == GamePhase.Playing && p.hand.Count == 1 && !p.babylonPowerEnabled)
                {
                    // discard the last card in the hand, unless their board is Babylon (B) and their Power is enabled (the 2nd wonder stage)
                    // Note that this must be done _before_ Halikarnassos plays its Power (free build from discard pile).
                    discardPile.Add(p.hand.First());
                    p.hand.Clear();
                }

                p.loseCoins();
            }
        }

        private string BuildResourceString(string who, Player plyr, bool isSelf = false)
        {
            string strRet = string.Format("&{0}Resources=", who);

            foreach (ResourceEffect se in plyr.dag.getResourceList(isSelf))
            {
                strRet += se.resourceTypes + ",";
            }

            return strRet.TrimEnd(',');
        }

        private string MakeHandString(Player p, List<Card> cardList, bool buildingFromDiscardedCards = false)
        {
            string strHand = "SetPlyrH";

            string strCards = "&Cards=";
            string strBuildStates = "&BuildStates=";

            foreach (Card card in cardList)
            {
                if (buildingFromDiscardedCards)
                {
                    // Filter out structures that have already been built in the players' city.
                    if (p.isCardBuildable(card) == Buildable.StructureAlreadyBuilt)
                        continue;
                }

                strCards += card.Id + ",";

                if (buildingFromDiscardedCards)
                {
                    strBuildStates += Buildable.True + ",";
                }
                else
                {
                    strBuildStates += p.isCardBuildable(card) + ",";
                }
            }

            strHand += strCards.TrimEnd(',');
            strHand += strBuildStates.TrimEnd(',');

            strHand += string.Format("&WonderStage={0},{1}", p.currentStageOfWonder, p.isStageBuildable());
            strHand += string.Format("&GamePhase={0}", this.phase);

            return strHand;
        }

        private string MakeCommerceInfoString(Player p)
        {
            // Commerce data
            string strCommerce = string.Empty;

            strCommerce += BuildResourceString("Player", p, true);
            strCommerce += BuildResourceString("Left", p.leftNeighbour);
            strCommerce += BuildResourceString("Right", p.rightNeighbour);

            strCommerce += string.Format("&coin={0}", p.coin);
            if (p.hasWestTradingPost) strCommerce += "&hasWestTradingPost=";
            if (p.hasEastTradingPost) strCommerce += "&hasEastTradingPost=";
            if (p.hasMarketplace) strCommerce += string.Format("&hasMarketplace=");
            if (p.hasClandestineDockWest) strCommerce += "&hasClandestineDockWest=";
            if (p.hasClandestineDockEast) strCommerce += "&hasClandestineDockEast=";
            if (p.hasSecretWarehouse) strCommerce += "&hasSecretWarehouse=";
            if (p.nBlackMarket != 0) strCommerce += string.Format("&nBlackMarket={0}", p.nBlackMarket);

            if (p.currentStageOfWonder < p.playerBoard.numOfStages)
                strCommerce += string.Format("&WonderStageCard={0}", Card.CardNameFromStringName(p.playerBoard.name, p.currentStageOfWonder + 1));

            Card bilkis = p.playedStructure.Find(x => x.Id == CardId.Bilkis);
            if (bilkis != null)
            {
                // Tell the commmerce window that the last entry in the resource list for the player is for Bilkis
                // and isn't due to another leader effect.
                strCommerce += "&" + bilkis.Id + "=";
            }

            strCommerce += "&LeaderDiscountCards=";

            foreach (Card c in p.playedStructure.Where(x => x.effect is StructureDiscountEffect))
            {
                strCommerce += c.Id + ",";
            }

            strCommerce = strCommerce.TrimEnd(',');

            return strCommerce;
        }

        /// <summary>
        /// Send the main display information for all players
        /// This is called at the beginning of the game and after each turn
        /// </summary>
        public void updateAllGameUI()
        {
            string strPlayerNames = "&Names=";
            string strCoins = "&Coins=";
            string strDebt = "&Debt=";
            string strDiplomacy = "&Diplomacy=";
            string strCardNames = "&CardNames=";

            foreach (Player p in player.Values)
            {
                strPlayerNames += p.nickname + ",";

                if (p.bUIRequiresUpdating)
                {
                    // TODO: update this to send built Wonder stage updates as well as the cards played panel.
                    Card card = p.playedStructure.Last();

                    if (card.structureType == StructureType.WonderStage)
                    {
                        strCardNames += string.Format("WonderStage{0},", card.wonderStage);
                    }
                    else
                    {
                        strCardNames += card.Id + ",";
                    }
                    p.bUIRequiresUpdating = false;
                }
                else
                {
                    strCardNames += "Discarded,";
                }

                strCoins += p.coin + ",";
                strDebt += p.debtToken + ",";
                strDiplomacy += p.diplomacyEnabled + ",";
            }

            string strCardsPlayed = "UpdateUI" +
                strPlayerNames.TrimEnd(',') +
                strCoins.TrimEnd(',') +
                strDebt.TrimEnd(',') +
                strDiplomacy.TrimEnd(',') +
                strCardNames.TrimEnd(',');

            foreach (Player p in player.Values)
            {
                if (phase != GamePhase.LeaderDraft)
                {
                    gmCoordinator.sendMessage(p, strCardsPlayed);
                }

                if (phase == GamePhase.LeaderDraft)
                {
                    string strLeaderHand = "LdrDraft";

                    foreach (Card card in p.hand)
                    {
                        strLeaderHand += string.Format("&{0}=", card.Id);
                    }

                    gmCoordinator.sendMessage(p, strLeaderHand);
                }
                else if (phase == GamePhase.LeaderRecruitment || (phase == GamePhase.RomaB && p.phase == GamePhase.RomaB))
                {
                    string strLeaderIcons = "LeadrIcn";

                    foreach (Card c in p.draftedLeaders)
                    {
                        strLeaderIcons += string.Format("&{0}=", c.Id);
                    }

                    gmCoordinator.sendMessage(p, strLeaderIcons);

                    string strHand = MakeHandString(p, p.draftedLeaders) + MakeCommerceInfoString(p);

                    strHand += "&Instructions=Leader Recruitment: choose a leader to play, build a wonder stage with, or discard for 3 coins";

                    gmCoordinator.sendMessage(p, strHand);
                }
                else if (phase == GamePhase.Courtesan && p.phase == GamePhase.Courtesan)
                {
                    string strCourtesanInfo = "Courtesn";

                    foreach (Card c in p.leftNeighbour.playedStructure.Where(x => x.structureType == StructureType.Leader))
                    {
                        strCourtesanInfo += string.Format("&{0}=", c.Id);
                    }

                    foreach (Card c in p.rightNeighbour.playedStructure.Where(x => x.structureType == StructureType.Leader))
                    {
                        strCourtesanInfo += string.Format("&{0}=", c.Id);
                    }

                    // tell the client to pick a leader card
                    gmCoordinator.sendMessage(p, strCourtesanInfo);
                }
                else if ((phase == GamePhase.Halikarnassos && p.phase == GamePhase.Halikarnassos) || (phase == GamePhase.Solomon && p.phase == GamePhase.Solomon))
                {
                    string strHand = MakeHandString(p, discardPile, true);

                    strHand += "&Instructions=Choose a card to play for free from the discard pile.  It cannot be discarded or used to build a wonder stage.";
                    strHand += "&CanDiscard=False";

                    gmCoordinator.sendMessage(p, strHand);
                }
                else if (phase == GamePhase.Babylon && p.phase == GamePhase.Babylon)
                {
                    string strHand = MakeHandString(p, p.hand) + MakeCommerceInfoString(p);
                    strHand += "&Instructions=Babylon: you may build the last card in your hand, use it to build a wonder stage, or discard it for 3 coins.";

                    gmCoordinator.sendMessage(p, strHand);
                }
                else if (phase == GamePhase.Playing || phase == GamePhase.End)
                {
                    // prevent other players from seeing their hands until all special phases are done.
                    // Normal turn: everyone gets a hand of cards to choose from.
                    string strHand = MakeHandString(p, p.hand) + MakeCommerceInfoString(p);
                    strHand += "&Instructions=Choose a card from the list below to play, build a wonder stage with, or discard";

                    gmCoordinator.sendMessage(p, strHand);
                }

                // TODO: should I send a message to the others players about what they're waiting for when there's a post-build phase?

                //send the timer signal if the current Age is less than 4 (i.e. game is still going)
                /*
                if (gameConcluded == false)
                {
                    gmCoordinator.sendMessage(p, "t");
                }
                else
                {
                    gmCoordinator.sendMessage(p, "e");
                }
                */
            }
        }

        protected int numOfPlayersThatHaveTakenTheirTurn = 0;

        bool SetSpecialPhase(Player p, GamePhase specialPhase)
        {
            if (this.phase == specialPhase && p.phase == specialPhase)
            {
                this.phase = this.prevPhase;
                this.prevPhase = GamePhase.None;
                p.phase = GamePhase.None;
            }
            else if (p.phase == specialPhase)
            {
                if (this.phase == GamePhase.LeaderRecruitment || this.phase == GamePhase.LeaderDraft || this.phase == GamePhase.Playing)
                    this.prevPhase = this.phase;
                this.phase = specialPhase;

                // we are in a special phase
                return true;
            }

            return false;
        }

        public void turnTaken()
        {
            numOfPlayersThatHaveTakenTheirTurn++;

            if ((numOfPlayersThatHaveTakenTheirTurn == numOfPlayers) || phase == GamePhase.Babylon || phase == GamePhase.Halikarnassos || phase == GamePhase.Solomon || phase == GamePhase.RomaB || phase == GamePhase.Courtesan)
            {
                //reset the number of players that have taken their turn
                numOfPlayersThatHaveTakenTheirTurn = 0;

                executeActionsAtEndOfTurn();

                if (currentTurn == nTurnsInEachAge)
                {
                    foreach (Player p in player.Values)
                    {
                        if (p.babylonPowerEnabled)
                            p.phase = GamePhase.Babylon;
                    }
                }

                bool bSpecialPhase = false;

                foreach (Player p in player.Values)
                {

                    // do this action first, so that if they discard their other card, Halikarnassos could built it if they
                    // are building from the discard pile.
                    // I will need to go through this logic carefully.  Babylon (B) must play or discard their last
                    // card _before_ Halikarnassos looks at the discard pile.  Will need to connect a 2nd client first,
                    // though.  Basically the GameManager has to get Babylon's choice before Halikarnassos can choose
                    // a card from the discard pile.  Note.  I _think_ I may be able to just add an "else" before the
                    // "if" here.  That way the game manager will do the Babylon extra card first, and only after that's
                    // done, do Halikarnassos.  Or maybe I'll need to add a game manager state to control this, rather
                    // than using booleans to control the logic.

                    if (!bSpecialPhase) bSpecialPhase = SetSpecialPhase(p, GamePhase.Babylon);
                    if (!bSpecialPhase) bSpecialPhase = SetSpecialPhase(p, GamePhase.Halikarnassos);
                    if (!bSpecialPhase) bSpecialPhase = SetSpecialPhase(p, GamePhase.Solomon);
                    if (!bSpecialPhase) bSpecialPhase = SetSpecialPhase(p, GamePhase.RomaB);
                    if (!bSpecialPhase) bSpecialPhase = SetSpecialPhase(p, GamePhase.Courtesan);

                    if (bSpecialPhase)
                        break;
                }

                //all players have completed their turn
                if (!bSpecialPhase)
                {
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
                            if (currentTurn == (nTurnsInEachAge+1))
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

                    default:
                        throw new Exception("specialPhase is false but phase is not LeaderDraft, LeaderRecruitment, or Playing.  Logic error somewhere...");
                    }
                }

                updateAllGameUI();

                if (gameConcluded)
                    endOfSessionActions();
            }
        }
    }
}
