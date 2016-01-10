using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ApplicationWebClient.Controllers.Api
{
    public class RecordsController : ApiController
    {
        [HttpGet, Route("api/records")]
        public HttpResponseMessage Get([FromUri]string[] channels, [FromUri]char span)
        {
            DIS.QueryTimeSpan datespan = (span == 'm' ? DIS.QueryTimeSpan.LastMonth : span == 'w' ? DIS.QueryTimeSpan.LastWeek : DIS.QueryTimeSpan.LastHours);
            try
            {
                using (var svc = new DIS.DataIntegrationServiceClient())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, svc.GetRecordsByChannelAndDatespan(channels, datespan));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = ex.Message });
            }
        }

        [HttpGet, Route("api/records")]
        public HttpResponseMessage Get([FromUri]String minDate, [FromUri]String maxDate, [FromUri]String group)
        {
            var dateMin = DateTime.ParseExact(minDate, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            var dateMax = DateTime.ParseExact(maxDate, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            Dictionary<String, DIS.QueryGroupFunction> options = new Dictionary<string, DIS.QueryGroupFunction>{ 
                {"avg", DIS.QueryGroupFunction.Average},
                {"min", DIS.QueryGroupFunction.Min},
                {"max", DIS.QueryGroupFunction.Max} 
            };
            try
            {
                using (var svc = new DIS.DataIntegrationServiceClient())
                {
                    return Request.CreateResponse(HttpStatusCode.OK, svc.GetRecordsByDataRange(dateMin, dateMax, options[group]));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = ex.Message });
            }
        }
    }
}
