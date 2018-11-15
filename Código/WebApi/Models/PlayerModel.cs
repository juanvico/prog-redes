using PlayerCRUDServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PlayerCRUDWebApi.Models
{
    public class PlayerModel : Model<Player, PlayerModel>
    {
        public Guid Id { get; set; }
        public string Nickname { get; set; }
        public string Avatar { get; set; }

        public PlayerModel() { }

        public PlayerModel(Player entity)
        {
            SetModel(entity);
        }

        public override Player ToEntity() => new Player()
        {
            Id = this.Id,
            Nickname = this.Nickname,
            Avatar = this.Avatar
        };

        protected override PlayerModel SetModel(Player entity)
        {
            Id = entity.Id;
            Nickname = entity.Nickname;
            Avatar = entity.Avatar;
            return this;
        }
    }
}