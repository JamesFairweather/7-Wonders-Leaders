﻿using System;
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
                gm.buildStructureFromHand(c.name, player.nickname, null, null, null);
                return;
            }
           
            //look for buildable yellow cards that gives some resources
            c = player.hand.Find(x => x.structureType == StructureType.Commerce &&
                player.isCardBuildable(x) == Buildable.True &&
                x.effect is ResourceChoiceEffect);

            if (c != null)
            {
                gm.buildStructureFromHand(c.name, player.nickname, null, null, null);
                return;
            }

            //look for buildable resource cards
            foreach(Card card in player.hand)
            {
                if ((card.structureType == StructureType.RawMaterial || card.structureType == StructureType.Goods) && player.isCardBuildable(card) == Buildable.True && card.effect is SimpleEffect)
                {
                    SimpleEffect e = card.effect as SimpleEffect;
                    char resource = e.type;
                    int numOfResource = e.multiplier;

                    if (resource == 'B' && numOfResource + player.brick < maxResourcesRequired ) { gm.buildStructureFromHand(card.name, player.nickname, null, null, null ); return; }
                    else if (resource == 'O' && numOfResource + player.ore < maxResourcesRequired) { gm.buildStructureFromHand(card.name, player.nickname, null, null, null ); return; }
                    else if (resource == 'T' && numOfResource + player.stone < maxResourcesRequired) { gm.buildStructureFromHand(card.name, player.nickname, null, null, null ); return; }
                    else if (resource == 'W' && numOfResource + player.wood < maxResourcesRequired) { gm.buildStructureFromHand(card.name, player.nickname, null, null, null ); return; }
                    else if (resource == 'G' && numOfResource + player.glass < maxResourcesRequired) { gm.buildStructureFromHand(card.name, player.nickname, null, null, null ); return; }
                    else if (resource == 'L' && numOfResource + player.loom < maxResourcesRequired) { gm.buildStructureFromHand(card.name, player.nickname, null, null, null ); return; }
                    else if (resource == 'P' && numOfResource + player.papyrus < maxResourcesRequired) { gm.buildStructureFromHand(card.name, player.nickname, null, null, null ); return; }
                }
            }

            //discard card[0]
            gm.discardCardForThreeCoins(player.hand[0], player);
        }
    }
}
