using LogServiceInterfaces;
using System;
using System.Collections.Generic;
using System.Web.Configuration;
using System.Web.Http;

namespace PlayerCRUDWebApi.Controllers
{
    public class LogsController : ApiController
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
        public IHttpActionResult LastMatch()
        {
            List<string> logs = LogService().GetGameLog();
            return Json(logs);
        } 
    }
}
