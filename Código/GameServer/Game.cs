using LogServiceInterfaces;
using Protocols;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Timers;
using static GameServer.GamePlayer;

namespace GameServer
{
    public static class Game
    {
        private static List<GamePlayer> Party;
        private static int CurrentPlayersNumber;
        private static GamePlayer[,] Matrix;

        private static Dictionary<string, GamePlayer> GamePlayers;
        private static List<string> DeadPlayers;

        private static List<PlayerStats> MatchPlayers;

        private const int KILL_POINTS = 10;
        private const int WINNER_POINTS = 100;

        private static bool ActiveMatch;
        private static Timer GameTimer;

        static readonly object objParty = new object();
        static readonly object objGamePlayers = new object();
        static readonly object objAttack = new object();

        public static ILogService Logger()
        {
            return (ILogService)Activator.GetObject(
                                        typeof(ILogService),
                                        "tcp://" + ConfigurationManager.AppSettings["LogServiceHostIP"] +
                                        ":" + ConfigurationManager.AppSettings["LogServiceHostPort"]
                                        + "/" + ConfigurationManager.AppSettings["LogServiceHostName"]);
        }

        public static bool IsDeadPlayer(Socket socket)
        {
            string nickname = GetNicknameBySocket(socket);
            return DeadPlayers.Contains(nickname);
        }

        static readonly object objMatrix = new object();

        const int THREE_MINUTES = 1000 * 60 * 3;

        public static void InitGame()
        {
            Party = new List<GamePlayer>();
            DeadPlayers = new List<string>();
            SetMatchHelpers();
        }

        public static void Move(Socket socket, string cmd)
        {
            GamePlayer gp = GamePlayers[GetNicknameBySocket(socket)];
            PlayerSpot oldSpot = new PlayerSpot(gp.Spot.Row, gp.Spot.Column);
            PlayerSpot newSpot = Utils.Move(gp.Spot, cmd);
            if (!ValidIndex(newSpot.Row, newSpot.Column))
            {
                throw new IncorrectMoveCmdEx();
            }
            if (!IsEmptySpot(newSpot.Row, newSpot.Column))
            {
                throw new ExistsPlayerForMoveEx();
            }
            lock (objMatrix)
            {
                Matrix[gp.Spot.Row, gp.Spot.Column] = null;
                Matrix[newSpot.Row, newSpot.Column] = gp;
                gp.Spot = newSpot;
            }
            Logger().AddNewLogEntry(Utils.MoveMessage(gp, oldSpot, newSpot));
            InspectCloserPlayers(gp.Nickname);
        }

        private static void SetMatchHelpers()
        {
            GamePlayers = new Dictionary<string, GamePlayer>();
            MatchPlayers = new List<PlayerStats>();
            CurrentPlayersNumber = 0;
            Matrix = new GamePlayer[8, 8];
            ActiveMatch = false;
            DeadPlayers = new List<string>();
        }

        public static void Attack(Socket socket)
        {
            string nickname = GetNicknameBySocket(socket);
            GamePlayer gp = GamePlayers[nickname];

            List<GamePlayer> closerPlayers = GetCloserPlayers(gp);

            foreach (GamePlayer playerToAttack in closerPlayers)
            {
                if (AreNotSurvivors(gp, playerToAttack))
                {
                    lock (objAttack)
                    {
                        gp.Attack(playerToAttack);
                    }

                    string msgToAttacker, msgToAttacked, logMessage = "";
                    if (playerToAttack.IsAlive)
                    {
                        msgToAttacker = Utils.GetAttackerDamageStatus(gp, playerToAttack);
                        msgToAttacked = Utils.GetAttackedDamageStatus(gp, playerToAttack);

                        logMessage = Utils.GetDamageStatus(gp, playerToAttack);
                        Logger().AddNewLogEntry(logMessage);
                    }
                    else
                    {
                        AssignScoreToPlayerStats(gp, KILL_POINTS);

                        msgToAttacker = Utils.GetAttackerKillStatus(playerToAttack);
                        msgToAttacked = Utils.GetAttackedKillStatus(gp);

                        logMessage = Utils.GetDamageStatus(gp, playerToAttack);
                        Logger().AddNewLogEntry(logMessage);
                        logMessage = Utils.GetKillStatus(gp, playerToAttack);
                        Logger().AddNewLogEntry(logMessage);
                        RemoveDeadPlayer(playerToAttack);
                    }
                    ServerTransmitter.Send(gp.PlayerSocket, msgToAttacker);
                    ServerTransmitter.Send(playerToAttack.PlayerSocket, msgToAttacked);
                }
            }
            EndMatch();
            InspectCloserPlayers(nickname);
        }

        private static void AssignScoreToPlayerStats(GamePlayer gp, int points)
        {
            MatchPlayers.Find(mp => mp.Nickname == gp.Nickname).KillScore += points;
        }

        public static void ExitClient(Socket socket)
        {
            string nick = GetNicknameBySocket(socket);
            GamePlayer gp = Party.Find(p => p.Nickname == nick);
            Party.Remove(gp);
        }

        public static void CheckIfPlayerIsConnected(string nickname)
        {
            if (!Party.Exists(p => p.Nickname == nickname))
            {
                throw new NotConnectedPlayerEx();
            }
        }

        private static bool AreNotSurvivors(GamePlayer gp, GamePlayer playerToAttack)
        {
            return !(gp.IsSurvivor() && playerToAttack.IsSurvivor());
        }

        private static void EndMatch()
        {
            int aliveMonsters = GetAliveMonsters(), aliveSurvivors = GetAliveSurvivors();

            if (aliveMonsters == 1 && aliveSurvivors == 0)
            {
                GamePlayer winner = MonsterWinner();
                AssignScoreToPlayerStats(winner, WINNER_POINTS);
                foreach (GamePlayer connectedPlayer in Party)
                {
                    ServerTransmitter.Separator(connectedPlayer.PlayerSocket);
                    ServerTransmitter.Send(connectedPlayer.PlayerSocket, Utils.GetWinnerMessage(winner));
                    ServerTransmitter.Separator(connectedPlayer.PlayerSocket);
                }
                Logger().AddNewLogEntry(Utils.GetWinnerMessage(winner));
                LogMatchResult();
                Game.ResetMatch();
            }
            else if (aliveMonsters == 0 && aliveSurvivors > 0)
            {
                AddWinPointsToSurvivors();
                foreach (GamePlayer connectedPlayer in Party)
                {
                    ServerTransmitter.Separator(connectedPlayer.PlayerSocket);
                    ServerTransmitter.Send(connectedPlayer.PlayerSocket, Utils.GetSurvivorWinMessage());
                    ServerTransmitter.Separator(connectedPlayer.PlayerSocket);
                }
                Logger().AddNewLogEntry(Utils.GetSurvivorWinMessage());
                LogMatchResult();
                Game.ResetMatch();
            }
        }

        private static GamePlayer MonsterWinner()
        {
            return GamePlayers.Values.First();
        }

        private static int GetAliveSurvivors()
        {
            int aliveSurvivors = 0;
            foreach (GamePlayer gp in GamePlayers.Values)
            {
                if (gp.IsSurvivor() && gp.IsAlive)
                {
                    aliveSurvivors++;
                }
            }
            return aliveSurvivors;
        }

        private static int GetAliveMonsters()
        {
            int aliveMonsters = 0;
            foreach (GamePlayer gp in GamePlayers.Values)
            {
                if (gp.IsMonster() && gp.IsAlive)
                {
                    aliveMonsters++;
                }
            }
            return aliveMonsters;
        }

        private static void AddWinPointsToSurvivors()
        {
            foreach (GamePlayer player in GamePlayers.Values)
            {
                if (player.IsSurvivor() && player.IsAlive)
                {
                    AssignScoreToPlayerStats(player, WINNER_POINTS);
                }
            }
        }

        private static void LogMatchResult(bool EndedByTime = false)
        {
            SetWinners(EndedByTime);
            Logger().AddNewGameResult(MatchPlayers);
        }

        private static void SetWinners(bool EndedByTime)
        {
            int aliveMonsters = GetAliveMonsters(), aliveSurvivors = GetAliveSurvivors();
            if (EndedByTime)
            {
                if (aliveSurvivors > 0)
                {
                    foreach (PlayerStats p in MatchPlayers)
                    {
                        if (p.IsSurvivor() && p.IsAlive)
                        {
                            p.IsWinner = true;
                        }
                    }
                }
            }
            else
            {
                if (aliveMonsters == 1 && aliveSurvivors == 0)
                {
                    PlayerStats p = MatchPlayers.Find(mp => mp.Nickname == MonsterWinner().Nickname);
                    p.IsWinner = true;
                }
                else if (aliveMonsters == 0 && aliveSurvivors > 0)
                {
                    foreach (PlayerStats p in MatchPlayers)
                    {
                        if (p.IsSurvivor() && p.IsAlive)
                        {
                            p.IsWinner = true;
                        }
                    }
                }
            }
        }

        public static void ListAllConnectedPlayers()
        {
            Console.WriteLine("CONNECTED PLAYERS:");

            foreach (GamePlayer p in Party)
            {
                IPEndPoint remoteIpEndPoint = p.PlayerSocket.RemoteEndPoint as IPEndPoint;
                Console.WriteLine(p.Nickname + " (" + remoteIpEndPoint.Address + ":" + remoteIpEndPoint.Port + ")");
            }
        }

        private static void ResetMatch()
        {
            foreach (GamePlayer gp in GamePlayers.Values)
            {
                gp.Reset();
            }
            SetMatchHelpers();
            Console.WriteLine(Utils.GetServerAvailableCmdsAfterEndMatch());
        }

        private static void RemoveDeadPlayer(GamePlayer playerToAttack)
        {
            MarkAsKilled(playerToAttack);
            DeadPlayers.Add(playerToAttack.Nickname);
            lock (objMatrix)
            {
                Matrix[playerToAttack.Spot.Row, playerToAttack.Spot.Column] = null;
            }
            lock (objGamePlayers)
            {
                GamePlayers.Remove(playerToAttack.Nickname);
            }
        }

        private static void MarkAsKilled(GamePlayer playerToAttack)
        {
            MatchPlayers.Find(mp => mp.Nickname == playerToAttack.Nickname).IsAlive = false;
        }

        private static List<GamePlayer> GetCloserPlayers(GamePlayer gp)
        {
            List<GamePlayer> closerPlayers = new List<GamePlayer>();

            for (int auxRow = -1; auxRow <= 1; auxRow++)
            {
                for (int auxColumn = -1; auxColumn <= 1; auxColumn++)
                {
                    int row = gp.Spot.Row + auxRow;
                    int column = gp.Spot.Column + auxColumn;
                    if (ValidIndex(row, column))
                    {
                        if (!IsSamePlayer(row, column, gp.Spot.Row, gp.Spot.Column))
                        {
                            if (!IsEmptySpot(row, column))
                            {
                                GamePlayer gpToInspect = Matrix[row, column];
                                closerPlayers.Add(gpToInspect);
                            }
                        }
                    }
                }
            }
            return closerPlayers;
        }

        public static void ConnectPlayerToParty(GamePlayer gp)
        {
            lock (objParty)
            {
                if (Party.Exists(p => p.Nickname == gp.Nickname))
                {
                    throw new ConnectedNicknameInUseEx();
                }
                Party.Add(gp);
            }
        }

        public static void StartGame()
        {
            ActiveMatch = true;
            GameTimer = new Timer(THREE_MINUTES);
            GameTimer.Elapsed += EndGameByTimer;
            GameTimer.AutoReset = false;
            GameTimer.Enabled = true;
            Logger().StartNewGameLog();
        }

        private static void EndGameByTimer(Object source, ElapsedEventArgs e)
        {
            int aliveMonsters = GetAliveMonsters(), aliveSurvivors = GetAliveSurvivors();
            if (aliveSurvivors > 0)
            {
                AddWinPointsToSurvivors();
                foreach (GamePlayer connectedPlayer in Party)
                {
                    ServerTransmitter.Separator(connectedPlayer.PlayerSocket);
                    ServerTransmitter.Send(connectedPlayer.PlayerSocket, Utils.GetSurvivorWinMessage());
                    ServerTransmitter.Separator(connectedPlayer.PlayerSocket);
                }
                Logger().AddNewLogEntry(Utils.TimesUp());
                Logger().AddNewLogEntry(Utils.GetSurvivorWinMessage());
                LogMatchResult(true);
                Game.ResetMatch();
            }
            else if (aliveMonsters > 0 && aliveSurvivors == 0)
            {
                foreach (GamePlayer connectedPlayer in Party)
                {
                    ServerTransmitter.Separator(connectedPlayer.PlayerSocket);
                    ServerTransmitter.Send(connectedPlayer.PlayerSocket, Utils.GetNoWinnersMessage());
                    ServerTransmitter.Separator(connectedPlayer.PlayerSocket);
                }
                Logger().AddNewLogEntry(Utils.TimesUp());
                Logger().AddNewLogEntry(Utils.GetNoWinnersMessage());
                Game.ResetMatch();
            }
        }

        public static bool IsActiveMatch()
        {
            return ActiveMatch;
        }

        public static void AssignRole(string role, string nickname)
        {
            GamePlayer gp = Party.Find(p => p.Nickname == nickname);
            gp.AssignRole(role);
        }

        public static void TryEnter()
        {
            if (!IsActiveMatch())
            {
                throw new NotActiveMatch();
            }
            else if (IsActiveMatch() && CurrentPlayersNumber > 64)
            {
                throw new MaxNumberOfPlayers();
            }
        }

        public static string GetNicknameBySocket(Socket socket1)
        {
            foreach (GamePlayer gp in Party)
            {
                if (EqualsSocket(gp.PlayerSocket, socket1))
                {
                    return gp.Nickname;
                }
            }
            return "";
        }

        public static bool EqualsSocket(Socket socket1, Socket Socket2)
        {
            string ip1 = ((IPEndPoint)socket1.RemoteEndPoint).Address.ToString();
            string port1 = ((IPEndPoint)socket1.RemoteEndPoint).Port.ToString();

            string ip2 = ((IPEndPoint)Socket2.RemoteEndPoint).Address.ToString();
            string port2 = ((IPEndPoint)Socket2.RemoteEndPoint).Port.ToString();

            return ip1 == ip2 && port1 == port2;
        }

        public static void AddPlayerToMatch(string nickname)
        {
            GamePlayer gp = Party.Find(p => p.Nickname == nickname);
            lock (objGamePlayers)
            {
                Tuple<int, int> playerSpot = AssignPlayerSpot();
                gp.AssignSpot(playerSpot.Item1, playerSpot.Item2);
                Matrix[playerSpot.Item1, playerSpot.Item2] = gp;
                GamePlayers.Add(gp.Nickname, gp);
                CurrentPlayersNumber++;
            }
            InspectCloserPlayers(nickname);
            Logger().AddNewLogEntry(Utils.PlayerEntered(gp));
            MatchPlayers.Add(new PlayerStats() { Nickname = gp.Nickname, Role = gp.Role });
        }

        public static bool IsCurrentlyPlayingMatch(Socket socket)
        {
            List<GamePlayer> players = GamePlayers.Values.ToList();
            foreach (GamePlayer gp in players)
            {
                if (EqualsSocket(socket, gp.PlayerSocket))
                {
                    return true;
                }
            }
            return false;
        }

        public static Tuple<int, int> AssignPlayerSpot()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int column = 0; column < 8; column++)
                {
                    if (Matrix[row, column] == null)
                    {
                        return Tuple.Create(row, column);
                    }
                }
            }
            return Tuple.Create(-1, -1);
        }

        public static void InspectCloserPlayers(string nickname)
        {
            if (IsActiveMatch())
            {
                GamePlayer gp = GamePlayers[nickname];
                Utils.ShowPlayerStatus(gp);

                List<GamePlayer> closerPlayers = GetCloserPlayers(gp);
                foreach (GamePlayer gpToInspect in closerPlayers)
                {
                    Utils.ShowCloserPlayerStatus(gp, gpToInspect);
                }
            }
        }

        private static bool IsEmptySpot(int row, int column)
        {
            return Matrix[row, column] == null;
        }

        private static bool IsSamePlayer(int row1, int column1, int row2, int column2)
        {
            return row1 == row2 && column1 == column2;
        }

        private static bool ValidIndex(int row, int column)
        {
            bool isRowValid = row >= 0 && row <= 7;
            bool isColumnValid = column >= 0 && column <= 7;
            return isRowValid && isColumnValid;
        }
    }
}
