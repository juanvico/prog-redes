using PlayerCRUDServiceInterfaces;
using PlayerCRUDWebApi.Models;
using System;
using System.Collections.Generic;
using System.Web.Configuration;
using System.Web.Http;

namespace PlayerCRUDWebApi.Controllers
{
    public class PlayersController : ApiController
    {
        private IPlayerCRUDService PlayerService()
        {
            return (IPlayerCRUDService)Activator.GetObject(
                                        typeof(IPlayerCRUDService),
                                        "tcp://" + WebConfigurationManager.AppSettings["PlayerCRUDServiceHostIP"] +
                                        ":" + WebConfigurationManager.AppSettings["PlayerCRUDServiceHostPort"]
                                        + "/" + WebConfigurationManager.AppSettings["PlayerCRUDServiceHostName"]);
        }

        [HttpGet]
        public IHttpActionResult Get()
        {
            List<PlayerModel> ret = new List<PlayerModel>();
            foreach (Player p in PlayerService().GetPlayers())
            {
                ret.Add(PlayerModel.ToModel(p));
            }
            return Json(ret);
        }

        [HttpGet]
        public IHttpActionResult Get(Guid id)
        {
            if (!PlayerService().Exists(id))
            {
                return NotFound();
            }
            return Json(PlayerService().Get(id));
        }

        [HttpPost]
        public IHttpActionResult Post([FromBody]PlayerModel model)
        {
            Player player = PlayerService().Add(PlayerModel.ToEntity(model));
            return Json(new { success = true });
        }

        [HttpPut]
        public IHttpActionResult Put(Guid id, [FromBody]PlayerModel model)
        {
            if (!PlayerService().Exists(id))
            {
                return NotFound();
            }
            PlayerService().Update(id, PlayerModel.ToEntity(model));
            return Json(new { success = true });
        }

        [HttpDelete]
        public IHttpActionResult Delete(Guid id)
        {
            PlayerService().Delete(id);
            return Json(new { success = true });
        }
    }
}
