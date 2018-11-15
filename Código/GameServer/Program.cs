using Protocols;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace GameServer
{
    class Program
    {
        private static IPAddress serverIP;
        private static int serverPort;
        public static void Main(string[] args)
        {
            try
            {
                AskIPAndPort();
                Game.InitGame();
                var server = new Socket(
                    AddressFamily.InterNetwork,
                    SocketType.Stream,
                    ProtocolType.Tcp
                    );

                server.Bind(new IPEndPoint(serverIP, serverPort));
                server.Listen(25);
                var startAcceptingClients = new Thread(() => AcceptClients(server));
                var executeServerCommands = new Thread(() => ExecuteCommands());
                startAcceptingClients.Start();
                executeServerCommands.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
        }

        private static void AskIPAndPort()
        {
            bool isIPValid = false;
            bool isPortValid = false;
            while (!isIPValid)
            {
                Console.WriteLine("Write Server IP: (press ENTER for localhost)");
                string msg = Console.ReadLine();
                if (!msg.Equals("")) isIPValid = IPAddress.TryParse(msg, out serverIP);
                else
                {
                    serverIP = IPAddress.Parse("127.0.0.1");
                    isIPValid = true;
                }
            }
            while (!isPortValid)
            {
                Console.WriteLine("Write Server Port: (press ENTER for 6000)");
                string msg = Console.ReadLine();
                if (!msg.Equals("")) isPortValid = Int32.TryParse(msg, out serverPort);
                else
                {
                    serverPort = 6000;
                    isPortValid = true;
                }
            }
            Console.WriteLine(Utils.GetSeparator());
            Console.WriteLine("Server started.");
            Console.WriteLine(Utils.GetSeparator());
        }

        public static void AcceptClients(Socket server)
        {
            while (true)
            {
                Socket socket = server.Accept();
                var clientThread = new Thread(() => ActionParser.Execute(socket));
                clientThread.Start();

                IPEndPoint remoteIpEndPoint = socket.RemoteEndPoint as IPEndPoint;
                IPEndPoint localIpEndPoint = socket.LocalEndPoint as IPEndPoint;
                string msgToServer = "Connection established to: " + remoteIpEndPoint.Address + ":" + remoteIpEndPoint.Port;
                string msgToClient = "Connection established to: " + localIpEndPoint.Address + ":" + localIpEndPoint.Port;
                NotifyServerNewClient(msgToServer);
                NotifySuccessfulConnectionToClient(socket, msgToClient);
            }

        }

        private static void NotifySuccessfulConnectionToClient(Socket socket, string msg)
        {
            ServerTransmitter.Send(socket, msg);
        }

        private static void NotifyServerNewClient(string msg)
        {
            Console.WriteLine(Utils.GetSeparator());
            Console.WriteLine(msg);
            Console.WriteLine(Utils.GetSeparator());
        }

        public static void ExecuteCommands()
        {
            while (true)
            {
                var command = Console.ReadLine();
                ActionParser.ExecuteCommand(command);
            }
        }

    }
}
