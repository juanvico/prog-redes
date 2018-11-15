using LogServiceInterfaces;
using PlayerCRUDWebApi.Models;
using System;
using System.Collections.Generic;
using System.Web.Configuration;
using System.Web.Http;

namespace PlayerCRUDWebApi.Controllers
{
    public class StatsController : ApiController
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
            List<MatchModel> ret = new List<MatchModel>();

            foreach (Match m in LogService().GetMatchesStats())
            {
                ret.Add(MatchModel.ToModel(m));
            }
            
            return Json(ret);
        }
    }
}
