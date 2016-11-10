using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SevenWonders
{
    class AIMoveAlgorithm4 : AIMoveBehaviour
    {
        int maxOBW = 2;
        int maxStone = 3;
        int maxLPG = 1;

        public void makeMove(Player player, GameManager gm)
        {
            //go for blue cards only on the third age
            //if not, Discard Red Cards
            //otherwise, discard first card

            string strOutput = string.Format("{0} hand: [ ", player.nickname);

            if (gm.phase == GamePhase.LeaderRecruitment)
            {
                foreach (Card card in player.draftedLeaders)
                {
                    strOutput += card.Id;
                    strOutput += " ";
                }
            }
            else
            {
                foreach (Card card in player.hand)
                {
                    strOutput += card.Id;
                    strOutput += " ";
                }
            }

            strOutput += "]";

            Console.WriteLine(strOutput);

            if (gm.phase == GamePhase.LeaderDraft || gm.phase == GamePhase.LeaderRecruitment)
            {
                // int[] favouredLeaders = { 216, 220, 222, 232, 200, 208, 205, 221, 214, 236, 213 };
                CardId [] favouredLeaders = { CardId.Leonidas, CardId.Nero, CardId.Pericles, CardId.Tomyris, CardId.Alexander, CardId.Hannibal, CardId.Caesar, CardId.Nefertiti, CardId.Cleopatra, CardId.Zenobia, CardId.Justinian };

                Card bestLeader = null;

                //try to find the highest rated card in hand
                //start looking for the highest rated card, then go down to the next highest, etc.
                foreach (CardId leaderName in favouredLeaders)
                {
                    if (gm.phase == GamePhase.LeaderDraft)
                    {
                        bestLeader = player.hand.Find(x => x.Id == leaderName);
                    }
                    else if (gm.phase == GamePhase.LeaderRecruitment)
                    {
                        bestLeader = player.draftedLeaders.Find(x => x.Id == leaderName);
                    }

                    if (bestLeader != null && player.isCardBuildable(bestLeader) == Buildable.True)
                    {
                        break;
                    }
                }

                if (bestLeader == null && gm.phase == GamePhase.LeaderDraft)
                {
                    // this hand didn't contain a favoured leader, so draft the first one in the list.  We cannot
                    // discard during the draft.  Leaders may only be discarded for 3 coins during recruitment.
                    bestLeader = player.hand[0];
                }

                if (bestLeader != null)
                {
                    Console.WriteLine(player.nickname + "Drafted leader: {0}", bestLeader.Id);
                    gm.playCard(player, bestLeader, BuildAction.BuildStructure, true, false, 0, 0);
                }
                else
                {
                    Console.WriteLine(player.nickname + " Action: Discard {0}", player.draftedLeaders[0].Id);
                    gm.playCard(player, player.draftedLeaders[0], BuildAction.Discard, true);
                }

                return;
            }

            // Build Guild cards in the 3rd age (except for the Courtesan's Guild, which requires entering a special Game Phase
            // that the AI has not been programmed to think about.
            Card c = player.hand.Find(x => x.structureType == StructureType.Guild && x.Id != CardId.Courtesans_Guild && player.isCardBuildable(x) == Buildable.True);

            if (c == null)
            {
                //look for buildable blue cards at the third age ..
                c = player.hand.Find(x => x.structureType == StructureType.Civilian && player.isCardBuildable(x) == Buildable.True && x.age == 3);
            }

            if (c == null)
            {
                //look for buildable green cards
                c = player.hand.Find(x => x.structureType == StructureType.Science && player.isCardBuildable(x) == Buildable.True);
            }

            if (c == null)
            {
                //look for buildable resource cards that give more than one manufactory resources ...
                foreach (Card card in player.hand)
                {
                    if ((card.structureType == StructureType.Commerce && player.isCardBuildable(card) == Buildable.True) && card.effect is ResourceEffect)
                    {
                        // char resource = player.hand[i].effect[2];        // hunh?
                        string resource = ((ResourceEffect)card.effect).resourceTypes;

                        if (resource.Length < 3)
                            continue;

                        if (resource.Contains("C") && player.loom < maxLPG * 2) { c = card; }
                        else if (resource.Contains("P") && player.papyrus < maxLPG * 2) { c = card; }
                        else if (resource.Contains("G") && player.glass < maxLPG * 2) { c = card; }

                        // not sure what's going on here.  I think there may have been a bug in the original implementation.
                    }
                }
            }

            if (c == null)
            {
                //look for buildable resource cards that give more than one resource ...
                foreach (Card card in player.hand)
                {
                    if ((card.structureType == StructureType.RawMaterial && player.isCardBuildable(card) == Buildable.True) && card.effect is ResourceEffect)
                    {
                        string resource = ((ResourceEffect)card.effect).resourceTypes;

                        if (player.brick < maxOBW && resource.Contains('B') ) { c = card; }
                        else if (player.ore < maxOBW && resource.Contains('O') ) { c = card; }
                        else if (player.stone < maxStone && resource.Contains('S') ) { c = card; }
                        else if (player.wood < maxOBW && resource.Contains('W') ) { c = card; }
                    }
                }
            }


            if (c == null)
            {
                //look for buildable resource cards that only give one and the manufactory resources ..
                foreach (Card card in player.hand)
                {
                    if ((card.structureType == StructureType.RawMaterial || card.structureType == StructureType.Goods) && player.isCardBuildable(card) == Buildable.True && card.effect is ResourceEffect)
                    {
                        ResourceEffect e = card.effect as ResourceEffect;

                        char resource = e.resourceTypes[0];
                        int numOfResource = e.IsDoubleResource() ? 2 : 1;

                        if (resource == 'C' && player.loom < maxLPG) { c = card; }
                        else if (resource == 'G' && player.glass < maxLPG) { c = card; }
                        else if (resource == 'P' && player.papyrus < maxLPG) { c = card; }
                        else if (resource == 'B' && numOfResource + player.brick < maxOBW) { c = card; }
                        else if (resource == 'O' && numOfResource + player.ore < maxOBW) { c = card; }
                        else if (resource == 'S' && numOfResource + player.stone < maxStone) { c = card; }
                        else if (resource == 'W' && numOfResource + player.wood < maxOBW) { c = card; }
                    }
                }
            }

            if (c == null)
            {
                //look for buildable Red cards
                c = player.hand.Find(x => x.structureType == StructureType.Military && player.isCardBuildable(x) == Buildable.True);
            }

            if (c == null)
            {
                // play a city card, if there is one
                List<Card> cityCardList = player.hand.FindAll(x => x.structureType == StructureType.City && player.isCardBuildable(x) == Buildable.True);

                if (cityCardList.Count > 0)
                {
                    // Try to find a card that causes other players to lose cards
                    c = cityCardList.Find(x => x.effect is LossOfCoinsEffect);

                    if (c == null)
                    {
                        c = cityCardList.Find(x => x.effect is CopyScienceSymbolFromNeighborEffect);
                    }

                    if (c == null)
                    {
                        c = cityCardList.Find(x => x.effect is MilitaryEffect);
                    }

                    if (c == null)
                    {
                        c = cityCardList.Find(x => x.effect is DiplomacyEffect);
                    }

                    if (c == null)
                    {
                        // play a point-scoring card.
                        c = cityCardList.Find(x => x.effect is CoinsAndPointsEffect);
                    }

                    // if none of the above criteria match, it means this card is has a commerce special effect,
                    // such as the Secret Warehouse, Black Market, Clandestine Dock, or Architect Cabinet, which
                    // this AI has not been programmed to think about.  So in that case, just play the first
                    // card in the list of playable city cards.
                    c = cityCardList[0];
                }
            }

            if (c == null)
            {
                //Discard the non-buildable Red cards
                foreach (Card card in player.hand)
                {
                    if (card.structureType == StructureType.Military && player.isCardBuildable(card) != Buildable.True)
                    {
                        Console.WriteLine(player.nickname + " Action: Discard {0}", card.Id);
                        gm.playCard(player, card, BuildAction.Discard, true);
                        return;
                    }
                }
            }

            if (c != null)
            {
                Console.WriteLine(player.nickname + " Action: Construct {0}", c.Id);
                gm.playCard(player, c, BuildAction.BuildStructure, true);
            }
            else
            {
                // If a card is not found that matches any of the above criteria, discard the first card listed.
                c = player.hand[0];
                Console.WriteLine(player.nickname + " Action: Discard {0}", c.Id);
                gm.playCard(player, c, BuildAction.Discard, true);
            }
        }
        
        public void loseCoins(Player player, int nCoins)
        {
            int nDebtTokens = 0;

            int c = player.coin - nCoins;

            if (c < 3)
            {
                nDebtTokens = player.coin - c;
            }

            player.takeDebtTokens(nDebtTokens);
        }
    }
}
