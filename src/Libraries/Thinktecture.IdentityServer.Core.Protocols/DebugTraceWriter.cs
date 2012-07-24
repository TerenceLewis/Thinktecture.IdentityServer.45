using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Tracing;

namespace Thinktecture.IdentityServer.Core.Protocols
{
    public class DebugTraceWriter : ITraceWriter
    {
        //public void Trace(HttpRequestMessage request, string category, TraceLevel level,
        //    Action<TraceRecord> traceAction)
        //{
        //    TraceRecord rec = new TraceRecord(request, category, level);
        //    //traceAction(rec);
        //    WriteTrace(rec);
        //}

        //// This method will be removed from the ITraceWriter interface.
        //public bool IsEnabled(string category, TraceLevel level)
        //{
        //    return true;
        //}

        //protected void WriteTrace(TraceRecord rec)
        //{
        //    var message = string.Format("{0};{1};{2}",
        //        rec.Operator, rec.Operation, rec.Message);

        //    System.Diagnostics.Debug.WriteLine(message);
        //}

        public static void Enable(HttpConfiguration config)
        {
            config.Services.Replace(typeof(ITraceWriter), new DebugTraceWriter());
        }

        public bool IsEnabled(string category, TraceLevel level)
        {
            return true;
        }

        public void Trace(HttpRequestMessage request, string category, TraceLevel level, Action<TraceRecord> traceAction)
        {
            TraceRecord rec = new TraceRecord(request, category, level);
            traceAction(rec);
            WriteTrace(rec);
        }

        protected void WriteTrace(TraceRecord rec)
        {
            var message = string.Format("{0};{1};{2}",
                rec.Operator, rec.Operation, rec.Message);
            System.Diagnostics.Trace.WriteLine(message, rec.Category);
        }
    }
}
