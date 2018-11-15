using LogServiceInterfaces;
using PlayerCRUDWebApi.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Configuration;
using System.Web.Http;

namespace PlayerCRUDWebApi.Controllers
{
    public class RankingsController : ApiController
    {
        private ILogService LogService()
        {
            return (ILogService)Activator.GetObject(
                                        typeof(ILogService),
                                        "tcp://" + WebConfigurationManager.AppSettings["LogServiceHostIP"] +
                                        ":" + WebConfigurationManager.AppSettings["LogServiceHostPort"]
                                        + "/" + WebConfigurationManager.AppSettings["LogServiceHostName"]);
        }

        [HttpGet]
        public IHttpActionResult Get()
        {
            List<PlayerStatsModel> ret = new List<PlayerStatsModel>();
            foreach (PlayerStats p in LogService().TopTenScores())
            {
                ret.Add(PlayerStatsModel.ToModel(p));
            }

            return Json(ret);
        }
    }
}
