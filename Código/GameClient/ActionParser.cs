using Protocols;
using System;
using System.Net.Sockets;
using System.Threading;

namespace GameClient
{
    public static class ActionParser
    {
        public static void Execute(Socket socket)
        {
            while (true)
            {
                string cmd = Console.ReadLine();
                cmd = Utils.ToLwr(cmd);

                if (cmd.Equals("exit"))
                {
                    ClientTransmitter.Send(socket, "exit");
                    Environment.Exit(0);
                }
                else if (cmd.Equals("newplayer"))
                {
                    ClientTransmitter.Send(socket, "newplayer");
                    NewPlayerAction(socket);
                }
                else if (cmd.Equals("connect"))
                {
                    ClientTransmitter.Send(socket, "connect");
                    ConnectPlayerToGame(socket);
                }
                else if (cmd.Equals("enter"))
                {
                    ClientTransmitter.Send(socket, "enter");
                    TryEnterGame(socket);
                }
                else if (cmd.Equals("attack"))
                {
                    ClientTransmitter.Send(socket, "attack");
                }
                else if (cmd.StartsWith("move"))
                {
                    ClientTransmitter.Send(socket, cmd);
                }
                else
                {
                    Console.WriteLine(Utils.GetClientAvailableCmds());
                }
            }
        }

        private static void TryEnterGame(Socket socket)
        {
            bool validRole = false;
            string role = "";
            while (!validRole)
            {
                Console.WriteLine("Choose your role (MONSTER or SURVIVOR):");
                role = Console.ReadLine();
                role = Utils.ToLwr(role);
                if (role.Equals("monster") || role.Equals("survivor"))
                {
                    validRole = true;
                }
            }

            ClientTransmitter.Send(socket, role);
        }

        private static void ConnectPlayerToGame(Socket socket)
        {
            Console.WriteLine("Insert nickname:");
            string nickname = Console.ReadLine();
            ClientTransmitter.Send(socket, nickname);
        }

        private static void NewPlayerAction(Socket socket)
        {
            bool[] requestedInfo = new bool[2];
            while (true)
            {
                if (requestedInfo[0] == false)
                {
                    Console.WriteLine("Insert nickname:");
                    string name = Console.ReadLine();
                    ClientTransmitter.Send(socket, name);
                    requestedInfo[0] = true;
                }
                else
                {
                    TrySendImage(socket);
                    break;
                }
            }
        }

        private static void TrySendImage(Socket socket)
        {
            bool isFileValid = false;
            while (!isFileValid)
            {
                Console.WriteLine("Insert avatar file name: (press ENTER to ignore)");
                Console.WriteLine("Example: avatar.jpg (File must be placed in application directory)");
                string fileName = Console.ReadLine();

                if (!fileName.Equals(""))
                {
                    isFileValid = ImageReader.FileExists(fileName);
                    if (isFileValid)
                    {
                        ClientTransmitter.Send(socket, ImageReader.GetBase64Image(fileName));
                    }
                }
                else
                {
                    ClientTransmitter.Send(socket, "default");
                    isFileValid = true;
                }
            }
        }
    }
}