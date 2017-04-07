using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace System.EventSourcing.AspNetCore.Kafka
{
    class FakeHttpReponseFeature : HttpResponseFeature, IHttpResponseFeature
    {
        public override void OnCompleted(Func<object, Task> callback, object state)
        {
            
        }

        public override void OnStarting(Func<object, Task> callback, object state)
        {
            
        }
    }
}
