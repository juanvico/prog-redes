using LogServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PlayerCRUDWebApi.Models
{
    public class MatchModel : Model<Match, MatchModel>
    {
        public List<PlayerStatsModel> PlayerStats { get; set; }
        public DateTime Date { get; set; }

        public MatchModel() { }

        public MatchModel(Match entity)
        {
            SetModel(entity);
        }

        public override Match ToEntity() => new Match()
        {
            Date = this.Date,
            PlayerStats = this.PlayerStats.ConvertAll(ps => ps.ToEntity())
        };

        protected override MatchModel SetModel(Match entity)
        {
            Date = entity.Date;
            PlayerStats = entity.PlayerStats.ConvertAll(ps => new PlayerStatsModel(ps));
            return this;
        }
    }
}