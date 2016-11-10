using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SevenWonders
{
    //Strategy 0: Discard the first card in the hand
    public class AIMoveAlgorithm0 : AIMoveBehaviour
    {
        public void makeMove(Player player, GameManager gm)
        {
            gm.playCard(player, player.hand[0], BuildAction.Discard, true);
        }
    }
}
