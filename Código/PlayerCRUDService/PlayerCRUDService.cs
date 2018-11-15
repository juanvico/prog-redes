using PlayerCRUDServiceInterfaces;
using System;
using System.Collections.Generic;

namespace PlayerCRUDService
{
    public class PlayerCRUDService : MarshalByRefObject, IPlayerCRUDService
    {
        private static List<Player> Players = new List<Player>();
        static readonly object objPlayers = new object();

        public Player Add(Player player)
        {
            lock (objPlayers)
            {
                if (Players.Exists(pl => pl.Nickname == player.Nickname))
                {
                    throw new NicknameInUseEx();
                }
                Players.Add(new Player() { Nickname = player.Nickname, Avatar = player.Avatar });
            }
            return player;
        }

        public Player Get(Guid id)
        {
            return Players.Find(p => p.Id == id);
        }

        public Player Update(Guid id, Player updatedPlayer)
        {
            Player player = Players.Find(p => p.Id == id);

            if (Players.Exists(pl => pl.Nickname == updatedPlayer.Nickname && pl.Id != player.Id))
            {
                throw new NicknameInUseEx();
            }

            player.Nickname = updatedPlayer.Nickname;
            player.Avatar = updatedPlayer.Avatar;

            return player;
        }

        public List<Player> GetPlayers()
        {
            return Players;
        }

        public void Delete(Guid id)
        {
            Players.Remove(Get(id));
        }

        public bool Exists(Guid id)
        {
            return Players.Exists(p => p.Id == id);
        }

        public bool ExistsByNickname(string nickname)
        {
            return Players.Exists(p => p.Nickname == nickname);
        }
    }
}
