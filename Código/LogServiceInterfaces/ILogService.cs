using System.Collections.Generic;

namespace LogServiceInterfaces
{
    public interface ILogService
    {
        void StartNewGameLog();
        
        void AddNewLogEntry(string entry);
        
        List<string> GetGameLog();
        
        void AddNewGameResult(List<PlayerStats> players);
        
        List<PlayerStats> TopTenScores();
        
        List<Match> GetMatchesStats();
    }
}
