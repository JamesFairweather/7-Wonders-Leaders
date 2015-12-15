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

            if (gm.phase == GameManager.GamePhase.LeaderRecruitment)
            {
                foreach (Card card in player.hand)
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

            if (gm.phase == GameManager.GamePhase.LeaderDraft || gm.phase == GameManager.GamePhase.LeaderRecruitment)
            {
                // int[] favouredLeaders = { 216, 220, 222, 232, 200, 208, 205, 221, 214, 236, 213 };
                CardId [] favouredLeaders = { CardId.Leonidas, CardId.Nero, CardId.Pericles, CardId.Tomyris, CardId.Alexander, CardId.Hannibal, CardId.Caesar, CardId.Nefertiti, CardId.Cleopatra, CardId.Zenobia, CardId.Justinian };

                Card bestLeader = null;

                //try to find the highest rated card in hand
                //start looking for the highest rated card, then go down to the next highest, etc.
                foreach (CardId leaderName in favouredLeaders)
                {
                    if (gm.phase == GameManager.GamePhase.LeaderDraft)
                    {
                        bestLeader = player.hand.Find(x => x.Id == leaderName);
                    }
                    else if (gm.phase == GameManager.GamePhase.LeaderRecruitment)
                    {
                        bestLeader = player.draftedLeaders.Find(x => x.Id == leaderName);
                    }

                    if (bestLeader != null)
                    {
                        break;
                    }
                }

                if (bestLeader == null && gm.phase == GameManager.GamePhase.LeaderDraft)
                {
                    // this hand didn't contain a favoured leader, so draft the first one in the list.  We cannot
                    // discard during the draft.  Leaders may only be discarded for 3 coins during recruitment.
                    bestLeader = player.hand[0];
                }

                if (bestLeader != null)
                {
                    Console.WriteLine(player.nickname + "Drafted leader: {0}", bestLeader.Id);
                    gm.buildStructureFromHand(player, bestLeader, false, false, 0, 0);
                }
                else
                {
                    Console.WriteLine(player.nickname + " Action: Discard {0}", player.draftedLeaders[0].Id);
                    gm.discardCardForThreeCoins(player, player.draftedLeaders[0]);
                }

                return;
            }

            // Build Guild cards in the 3rd age
            Card c = player.hand.Find(x => x.structureType == StructureType.Guild && player.isCardBuildable(x) == Buildable.True);

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
                //Discard the non-buildable Red cards
                foreach (Card card in player.hand)
                {
                    if (card.structureType == StructureType.Military && player.isCardBuildable(card) != Buildable.True)
                    {
                        Console.WriteLine(player.nickname + " Action: Discard {0}", card.Id);
                        gm.discardCardForThreeCoins(player, card);
                        return;
                    }
                }
            }

            if (c != null)
            {
                Console.WriteLine(player.nickname + " Action: Construct {0}", c.Id);
                gm.buildStructureFromHand(player, c, false);
            }
            else
            {
                c = player.hand[0];
                Console.WriteLine(player.nickname + " Action: Discard {0}", c.Id);
                gm.discardCardForThreeCoins(player, c);
            }
        } 
    }
}
