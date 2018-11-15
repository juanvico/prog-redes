using Protocols;
using static GameServer.GamePlayer;

namespace GameServer
{
    public class Utils
    {
        public static string GetSeparator()
        {
            return "_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ ";
        }
        public static string ToLwr(string cmd)
        {
            return cmd.ToLower();
        }

        public static void CheckMovementCmd(string cmd)
        {
            string[] arr = cmd.Split(' ');
            if (!(arr.Length == 2 || arr.Length == 3))
            {
                throw new IncorrectMoveCmdEx();
            }
            if (arr.Length == 2)
            {
                if (!ValidDirectionCmd(arr[1]))
                {
                    throw new IncorrectMoveCmdEx();
                }
            }
            else if (arr.Length == 3)
            {
                if (!(ValidDirectionCmd(arr[1]) && ValidDirectionCmd(arr[2])))
                {
                    throw new IncorrectMoveCmdEx();
                }
            }
        }

        public static string GetWaitMessageForLoser()
        {
            return "You lost this match. Wait until it finishes.";
        }

        private static bool ValidDirectionCmd(string v)
        {
            var cmd = ToLwr(v);
            return (cmd == "u" || cmd == "d" || cmd == "l" || cmd == "r" || cmd == "ul" || cmd == "ur" || cmd == "dl" || cmd == "dr");
        }

        public static string[] ParseMoves(string cmd)
        {
            string[] arr = cmd.Split(' ');
            if (arr.Length == 2)
            {
                return new string[] { arr[1] };
            }
            else if (arr.Length == 3)
            {
                return new string[] { arr[1], arr[2] };
            }
            return new string[0];
        }

        public static string GetMatchAvailableCmds()
        {
            return "<attack> - <move * *>";
        }

        public static PlayerSpot Move(PlayerSpot spot, string cmd)
        {
            string[] arr = ParseMoves(cmd);
            PlayerSpot newSpot = Evaluate(arr[0], spot);
            if (arr.Length == 2)
            {
                return Evaluate(arr[1], newSpot);
            }
            return newSpot;
        }

        private static PlayerSpot Evaluate(string dir, PlayerSpot spot)
        {
            PlayerSpot newSpot = new PlayerSpot(-1, -1); ;
            if (dir == "u")
            {
                newSpot = new PlayerSpot(spot.Row - 1, spot.Column);
            }
            else if (dir == "d")
            {
                newSpot = new PlayerSpot(spot.Row + 1, spot.Column);
            }
            else if (dir == "l")
            {
                newSpot = new PlayerSpot(spot.Row, spot.Column - 1);
            }
            else if (dir == "r")
            {
                newSpot = new PlayerSpot(spot.Row, spot.Column + 1);
            }
            else if (dir == "ul")
            {
                newSpot = new PlayerSpot(spot.Row - 1, spot.Column - 1);
            }
            else if (dir == "ur")
            {
                newSpot = new PlayerSpot(spot.Row - 1, spot.Column + 1);
            }
            else if (dir == "dl")
            {
                newSpot = new PlayerSpot(spot.Row + 1, spot.Column - 1);
            }
            else if (dir == "dr")
            {
                newSpot = new PlayerSpot(spot.Row + 1, spot.Column + 1);
            }
            return newSpot;
        }

        public static string MoveMessage(GamePlayer gp, PlayerSpot oldSpot, PlayerSpot newSpot)
        {
            return gp.Role + " (" + gp.Nickname + ") moved from r." + oldSpot.Row + " c." + oldSpot.Column +
                " to r." + newSpot.Row + " c." + newSpot.Column;
        }

        public static string GetDamageStatus(GamePlayer gp, GamePlayer playerToAttack)
        {
            return gp.Nickname + " damaged "+ playerToAttack.Nickname +" with " + gp.Damage + " life points.";
        }

        public static string GetAttackerKillStatus(GamePlayer playerToAttack)
        {
            return "You killed " + playerToAttack.Nickname + ".";
        }

        public static string GetKillStatus(GamePlayer gp, GamePlayer playerToAttack)
        {
            return gp.Nickname + " killed " + playerToAttack.Nickname + "."; ;
        }

        public static string GetAttackedKillStatus(GamePlayer gp)
        {
            return gp.Nickname + " killed you.";
        }

        public static string GetAttackedDamageStatus(GamePlayer gp, GamePlayer playerToAttack)
        {
            return gp.Nickname + " damaged you with " + gp.Damage + " life points. You got "
                        + playerToAttack.Life + " remaining life points.";
        }

        public static string GetAttackerDamageStatus(GamePlayer gp, GamePlayer playerToAttack)
        {
            return "You damaged " + gp.Damage + " life points to " +
                    playerToAttack.Nickname + " ( " + playerToAttack.Life + " life remaining )";

        }

        public static void ShowCloserPlayerStatus(GamePlayer gp, GamePlayer gpToInspect)
        {
            string msg = gpToInspect.Role + " (" + gpToInspect.Nickname + ") r." +
                gpToInspect.Spot.Row + " c." + gpToInspect.Spot.Column + " : " +
                gpToInspect.Life + " life points.";
            ServerTransmitter.Send(gp.PlayerSocket, msg);
        }

        public static string GetWinnerMessage(GamePlayer winner)
        {
            return winner.Nickname + " (" + winner.Role + ") WON THE GAME";
        }

        public static string GetSurvivorWinMessage()
        {
            return "SURVIVORS WIN";
        }

        public static void ShowPlayerStatus(GamePlayer gp)
        {
            string msg = "You: r." + gp.Spot.Row + " c." + gp.Spot.Column + " : " + gp.Life + " life points.";
            ServerTransmitter.Send(gp.PlayerSocket, msg);
        }

        public static string GetNoWinnersMessage()
        {
            return "NOBODY WINS";
        }

        public static string GetServerAvailableCmdsAfterEndMatch()
        {
            return "Match Ended. " + GetServerAvailableCmds();
        }

        public static string GetServerAvailableCmds()
        {
            return "Available commands: <startgame> - <registeredplayers> - <connectedplayers>";
        }

        public static string TimesUp()
        {
            return "Time's up!";
        }

        public static string PlayerEntered(GamePlayer gp)
        {
            return gp.Role + " (" + gp.Nickname + ") joined the game.";
        }
    }
}
