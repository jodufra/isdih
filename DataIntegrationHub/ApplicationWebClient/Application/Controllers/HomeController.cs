using ApplicationWebClient.Application.Others;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ApplicationWebClient.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "IS - Data Integration";

            return View();
        }

        public ActionResult Statistics(String[] channels, Char span)
        {
            DIS.QueryTimeSpan datespan = (span == 'm' ? DIS.QueryTimeSpan.LastMonth : span == 'w' ? DIS.QueryTimeSpan.LastWeek : DIS.QueryTimeSpan.LastHours);
            
            using (var svc = new ApplicationWebClient.DIS.DataIntegrationServiceClient())
            {
                var records = svc.GetRecordsByChannelAndDatespan(channels, datespan);
                using (var excel = ExcelHandler.CreateStatistics(channels, datespan, records))
                {
                    var stream = new MemoryStream(excel.GetAsByteArray());
                    var type = @"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    var filename = ("Statistics " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString()).Replace(":", "_").Replace(" ", "_");
                    return File(stream, type, filename);
                }
            }
        }

    }

}
