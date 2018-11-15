using System;
using System.Collections.Generic;

namespace PlayerCRUDServiceInterfaces
{
    public interface IPlayerCRUDService
    {
        Player Add(Player player);
        
        Player Get(Guid id);
        
        Player Update(Guid id, Player updatedPlayer);
        
        List<Player> GetPlayers();
        
        void Delete(Guid id);
        
        bool Exists(Guid id);
     
        bool ExistsByNickname(string nickname);
    }
}
