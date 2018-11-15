using LogServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PlayerCRUDWebApi.Models
{
    public class PlayerStatsModel : Model<PlayerStats, PlayerStatsModel>
    {
        public string Nickname { get; set; }
        public string Role { get; set; }
        public int KillScore { get; set; }
        public DateTime Date { get; set; }

        public bool IsWinner { get; set; }

        public PlayerStatsModel() { }

        public PlayerStatsModel(PlayerStats entity)
        {
            SetModel(entity);
        }

        public override PlayerStats ToEntity() => new PlayerStats()
        {
            Nickname = this.Nickname,
            Role = this.Role,
            KillScore = this.KillScore,
            Date = this.Date,
            IsWinner = this.IsWinner
        };

        protected override PlayerStatsModel SetModel(PlayerStats entity)
        {
            Nickname = entity.Nickname;
            Role = entity.Role;
            KillScore = entity.KillScore;
            Date = entity.Date;
            IsWinner = entity.IsWinner;
            return this;
        }
    }
}