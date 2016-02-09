using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SevenWonders
{
    public interface AIMoveBehaviour
    {
        void makeMove(Player player, GameManager gm);

        void loseCoins(Player p, int nCoins);
    }
}
