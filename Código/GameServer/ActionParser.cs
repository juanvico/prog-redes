using PlayerCRUDServiceInterfaces;
using Protocols;
using System;
using System.Configuration;
using System.Net;
using System.Net.Sockets;

namespace GameServer
{
    public class ActionParser
    {
        public static IPlayerCRUDService Players()
        {
            return (IPlayerCRUDService)Activator.GetObject(
                                        typeof(IPlayerCRUDService),
                                        "tcp://" + ConfigurationManager.AppSettings["PlayerCRUDServiceHostIP"] +
                                        ":" + ConfigurationManager.AppSettings["PlayerCRUDServiceHostPort"]
                                        + "/" + ConfigurationManager.AppSettings["PlayerCRUDServiceHostName"]);
        }
        public static void Execute(Socket socket)
        {
            bool loop = true;
            while (loop)
            {
                string command = ServerTransmitter.Receive(socket);
                var cmd = Utils.ToLwr(command);

                if (Game.IsDeadPlayer(socket))
                {
                    ServerTransmitter.Send(socket, Utils.GetWaitMessageForLoser());
                }
                else if (!Game.IsCurrentlyPlayingMatch(socket))
                {
                    loop = LetExecuteClientAction(socket, cmd);
                }
                else if (Game.IsCurrentlyPlayingMatch(socket))
                {
                    LetExecuteGameAction(socket, cmd);
                }
            }
        }

        private static bool LetExecuteClientAction(Socket socket, string cmd)
        {
            if (cmd.Equals("newplayer"))
            {
                string nick = ServerTransmitter.Receive(socket);
                string avatar = ServerTransmitter.Receive(socket);
                try
                {
                    Player player = new Player
                    {
                        Nickname = nick,
                        Avatar = avatar
                    };
                    Players().Add(player);
                    NotifyClientRegisteredPlayer(socket);
                }
                catch (NicknameInUseEx ex)
                {
                    ServerTransmitter.Send(socket, ex.Message);
                }
            }
            else if (cmd.Equals("connect"))
            {
                string nick = ServerTransmitter.Receive(socket);
                try
                {
                    if (!Players().ExistsByNickname(nick))
                    {
                        throw new NotExistingPlayer();
                    }
                    
                    Game.ConnectPlayerToParty(GamePlayer.Create(socket, nick));
                    ServerTransmitter.Send(socket, "Player connected to the game.");
                }
                catch (ConnectedNicknameInUseEx ex)
                {
                    ServerTransmitter.Send(socket, ex.Message);
                }
                catch (NotExistingPlayer ex)
                {
                    ServerTransmitter.Send(socket, ex.Message);
                }
            }
            else if (cmd.Equals("enter"))
            {
                string role = ServerTransmitter.Receive(socket);
                try
                {
                    Game.TryEnter();
                    string nickname = Game.GetNicknameBySocket(socket);
                    Game.CheckIfPlayerIsConnected(nickname);
                    Game.AssignRole(role, nickname);
                    Game.AddPlayerToMatch(nickname);
                    ServerTransmitter.Send(socket, "Logged in to match correctely. Start to play.");
                }
                catch (NotActiveMatch ex)
                {
                    ServerTransmitter.Send(socket, ex.Message);
                }
                catch (MaxNumberOfPlayers ex)
                {
                    ServerTransmitter.Send(socket, ex.Message);
                }
                catch (NotConnectedPlayerEx ex)
                {
                    ServerTransmitter.Send(socket, ex.Message);
                }
            }
            else if (cmd.Equals("exit"))
            {
                Game.ExitClient(socket);
                IPEndPoint remoteIpEndPoint = socket.RemoteEndPoint as IPEndPoint;
                Console.WriteLine("Cliente " + remoteIpEndPoint.Address + ":" + remoteIpEndPoint.Port + " cerrado.");
                socket.Close();
                return false;
            }
            return true;
        }

        private static void LetExecuteGameAction(Socket socket, string cmd)
        {
            if (cmd.Equals("attack"))
            {
                Game.Attack(socket);
            }
            else if (cmd.StartsWith("move "))
            {
                try
                {
                    Utils.CheckMovementCmd(cmd);
                    Game.Move(socket, cmd);
                }
                catch (IncorrectMoveCmdEx ex)
                {
                    ServerTransmitter.Send(socket, ex.Message);
                }
                catch (ExistsPlayerForMoveEx ex)
                {
                    ServerTransmitter.Send(socket, ex.Message);
                }
            }
            else
            {
                ServerTransmitter.Send(socket, Utils.GetMatchAvailableCmds());
            }
        }

        public static string GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssffff");
        }

        private static void NotifyClientRegisteredPlayer(Socket socket)
        {
            ServerTransmitter.Separator(socket);
            ServerTransmitter.Send(socket, "Player registered.");
            ServerTransmitter.Separator(socket);
        }

        public static void ExecuteCommand(string cmd)
        {
            var command = Utils.ToLwr(cmd);

            if (Game.IsActiveMatch())
            {
                Console.WriteLine("Can't execute other commands while game in process.");
            }
            else if (command.Equals("startgame"))
            {
                Game.StartGame();
                Console.WriteLine("GAME STARTED!");
            }
            else if (cmd.Equals("registeredplayers"))
            {
                Console.WriteLine("REGISTERED PLAYERS:");
                
                foreach (Player p in Players().GetPlayers())
                {
                    Console.WriteLine(p.Nickname + " (" + p.Avatar + ")");
                }
            }
            else if (cmd.Equals("connectedplayers"))
            {
                Game.ListAllConnectedPlayers();
            }
            else
            {
                Console.WriteLine(Utils.GetServerAvailableCmds());
            }
        }
    }
}
