using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace ApplicationLib.Entities
{
    public class DbContextFactory
    {
        public static DbContext GetContextPerRequest()
        {
            HttpContext httpContext = HttpContext.Current;
            if (httpContext == null)
            {
                return new DbContext();
            }
            else
            {
                int contextId = Thread.CurrentContext.ContextID;
                int hashCode = httpContext.GetHashCode();
                string key = string.Concat(hashCode, contextId);

                DbContext context = httpContext.Items[key] as DbContext;
                if (context == null)
                {
                    context = new DbContext();
                    httpContext.Items[key] = context;
                }

                return context;
            }
        }

        public static void Dispose()
        {
            HttpContext httpContext = HttpContext.Current;
            if (httpContext != null)
            {
                int contextId = Thread.CurrentContext.ContextID;
                int hashCode = httpContext.GetHashCode();
                string key = string.Concat(hashCode, contextId);

                DbContext context = httpContext.Items[key] as DbContext;
                if (context != null)
                {
                    context.Dispose();
                }
            }
        }
    }
}
