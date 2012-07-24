using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Tracing;

namespace Thinktecture.IdentityServer.Protocols
{
    public class DebugTraceWriter : ITraceWriter
    {
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
