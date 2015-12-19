using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SevenWonders
{
    public class Score
    {
        public int military;
        public int coins;
        public int wonders;
        public int civilian;
        public int commerce;
        public int guilds;
        public int science;
        public int leaders;
        public int cities;

        public int Total()
        {
            return military + coins + wonders + civilian + commerce + guilds + science + leaders + cities;
        }
    };
}

