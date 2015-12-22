using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SevenWonders
{
    //Strategy 1: Build first in the hand that is buildable.  If there are no buildable cards, discard the first one
    public class AIMoveAlgorithm1 : AIMoveBehaviour
    {
        public void makeMove(Player p, GameManager gm)
        {
            // Build the first card in the hand that we can build.  If there
            // are no buildable cards, discard the first one.
            foreach (Card c in p.hand)
            {
                if (p.isCardBuildable(c) == Buildable.True)
                {
                    gm.playCard(p, c, BuildAction.BuildStructure, true);
                    return;
                }
            }

            gm.playCard(p, p.hand[0], BuildAction.Discard, true);
        }
    }
}
