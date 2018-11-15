using System;
using System.Collections.Generic;

namespace LogServiceInterfaces
{
    [Serializable]
    public class Match
    {
        public List<PlayerStats> PlayerStats { get; set; }
        public DateTime Date { get; set; }

        public Match()
        {
            Date = DateTime.Now;
            PlayerStats = new List<PlayerStats>();
        }
    }
}
