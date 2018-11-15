using System;

namespace PlayerCRUDServiceInterfaces
{
    [Serializable]
    public class Player
    {
        public Guid Id { get; set; }
        public string Nickname { get; set; }
        public string Avatar { get; set; }

        public Player()
        {
            Id = Guid.NewGuid();
        }
    }
}
