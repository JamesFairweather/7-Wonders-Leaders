using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SevenWonders
{
    public class AIMoveAlgorithm2 : AIMoveBehaviour
    {
        int maxResourcesRequired = 3;

        public void makeMove(Player player, GameManager gm)
        {
            //go for blue cards whenever you can
            //if not, go for resources
            //otherwise, discard first card

            //look for buildable blue cards
            Card c = player.hand.Find(x => x.structureType == StructureType.Civilian && player.isCardBuildable(x) == Buildable.True);

            if (c != null)
            {
                gm.buildStructureFromHand(player, c, false, true);
                return;
            }
           
            //look for buildable yellow cards that gives some resources
            c = player.hand.Find(x => x.structureType == StructureType.Commerce &&
                player.isCardBuildable(x) == Buildable.True &&
                x.effect is ResourceEffect);

            if (c != null)
            {
                gm.buildStructureFromHand(player, c, false, true);
                return;
            }

            //look for buildable resource cards
            foreach(Card card in player.hand)
            {
                if ((card.structureType == StructureType.RawMaterial || card.structureType == StructureType.Goods) && player.isCardBuildable(card) == Buildable.True && card.effect is ResourceEffect)
                {
                    ResourceEffect e = card.effect as ResourceEffect;
                    char resource = e.resourceTypes[0];
                    int numOfResource = e.IsDoubleResource() ? 2 : 1;

                    if (resource == 'B' && numOfResource + player.brick < maxResourcesRequired ) { gm.buildStructureFromHand(player, card, false, true); return; }
                    else if (resource == 'O' && numOfResource + player.ore < maxResourcesRequired) { gm.buildStructureFromHand(player, card, false, true); return; }
                    else if (resource == 'T' && numOfResource + player.stone < maxResourcesRequired) { gm.buildStructureFromHand(player, card, false, true); return; }
                    else if (resource == 'W' && numOfResource + player.wood < maxResourcesRequired) { gm.buildStructureFromHand(player, card, false, true); return; }
                    else if (resource == 'G' && numOfResource + player.glass < maxResourcesRequired) { gm.buildStructureFromHand(player, card, false, true); return; }
                    else if (resource == 'L' && numOfResource + player.loom < maxResourcesRequired) { gm.buildStructureFromHand(player, card, false, true); return; }
                    else if (resource == 'P' && numOfResource + player.papyrus < maxResourcesRequired) { gm.buildStructureFromHand(player, card, false, true); return; }
                }
            }

            //discard card[0]
            gm.discardCardForThreeCoins(player, player.hand[0], true);
        }
    }
}
