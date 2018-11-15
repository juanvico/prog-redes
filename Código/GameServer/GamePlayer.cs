using System.Net.Sockets;

namespace GameServer
{
    public class GamePlayer
    {
        public Socket PlayerSocket { get; set; }
        public string Nickname { get; set; }
        public string Role { get; set; }
        public int Life { get; set; }
        public int Damage { get; set; }

        public bool IsAlive { get { return Life > 0; } }

        public PlayerSpot Spot;
        public struct PlayerSpot
        {
            public int Row;
            public int Column;
            public PlayerSpot(int row, int column)
            {
                Row = row;
                Column = column;
            }
        }


        public static GamePlayer Create(Socket playerSocket, string nickname)
        {
            GamePlayer gp = new GamePlayer()
            {
                PlayerSocket = playerSocket,
                Nickname = nickname,
                Spot = new PlayerSpot(-1, -1)
        };
            return gp;
        }

        public void AssignRole(string role)
        {
            this.Role = role;
            if (IsMonster())
            {
                Life = 100;
                Damage = 10;
            }
            else
            {
                Life = 20;
                Damage = 5;
            }
        }

        public void AssignSpot(int row, int column)
        {
            Spot = new PlayerSpot(row, column);
        }

        public void Attack(GamePlayer playerToAttack)
        {
            playerToAttack.ReceiveDamage(this.Damage);
        }

        public void ReceiveDamage(int damage)
        {
            this.Life -= damage;
        }

        public bool IsMonster()
        {
            return Role.Equals("monster");
        }

        public bool IsSurvivor()
        {
            return Role.Equals("survivor");
        }

        public void Reset()
        {
            if (IsMonster())
            {
                Life = 100;
            }
            else
            {
                Life = 20;
            }
            Spot = new PlayerSpot(-1, -1);
        }
    }
}