using LogServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Messaging;

namespace LogService
{
    public class LogService : MarshalByRefObject, ILogService
    {
        const string Queue = @".\private$\Slasher";
        private static List<Match> Matches = new List<Match>();

        public void AddNewLogEntry(string entry)
        {
            if (!MessageQueue.Exists(Queue))
            {
                MessageQueue.Create(Queue);
            }
            using (var queue = new MessageQueue(Queue))
            {
                var message = new Message(entry + " @ " + DateTime.Now)
                {
                    Label = Guid.NewGuid().ToString()
                };

                queue.Send(message);
            }
        }

        public List<string> GetGameLog()
        {
            List<string> messages = new List<string>();
            using (var queue = new MessageQueue(Queue))
            {
                queue.Formatter = new XmlMessageFormatter(new Type[] { typeof(string) });
                Message[] mm = queue.GetAllMessages();
                foreach (Message m in mm)
                {
                    messages.Add(m.Body.ToString());
                }
            }
            return messages;
        }

        public void StartNewGameLog()
        {
            if (MessageQueue.Exists(Queue))
            {
                MessageQueue.Delete(Queue);
            }
        }

        public void AddNewGameResult(List<PlayerStats> players)
        {
            Matches.Add(new Match() { PlayerStats = players });
        }

        public List<Match> GetMatchesStats()
        {
            List<Match> lastMatches = new List<Match>();
            int count = Matches.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                lastMatches.Add(Matches[i]);
                if (lastMatches.Count == 10)
                {
                    i = -1;
                }
            }
            return lastMatches;
        }

        public List<PlayerStats> TopTenScores()
        {
            List<PlayerStats> allPlayerStats = new List<PlayerStats>();
            foreach (Match match in Matches)
            {
                allPlayerStats.AddRange(match.PlayerStats);
            }

            List<PlayerStats> SortedList = allPlayerStats.OrderBy(o => o.KillScore).ToList();

            List<PlayerStats> top = new List<PlayerStats>();
            int count = SortedList.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                top.Add(SortedList[i]);
                if (top.Count == 10)
                {
                    i = -1;
                }
            }
            return top;
        }
    }
}
