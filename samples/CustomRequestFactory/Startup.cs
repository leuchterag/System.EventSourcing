using Confluent.Kafka;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using SharpCompress.Archives.Tar;
using System.Collections.Generic;
using System.EventSourcing.AspNetCore.Hosting;
using System.EventSourcing.AspNetCore.Kafka;
using System.IO;
using System.Linq;
using System.Net.Http;

namespace CustomRequestFactory
{
    public class Startup
    {
        static HttpRequestFeature MultipartRequestFactory(Message<string, byte[]> msg, string subject, string action)
        {
            var boundary = $"----{ Path.GetRandomFileName()}";
            var headers = new HeaderDictionary {  };

            var requestStream = new MemoryStream();

            using (var content = new MultipartFormDataContent(boundary))
            using (var evntCtntStrm = new MemoryStream(msg.Value))
            using (var tarBall = TarArchive.Open(evntCtntStrm))
            {
                tarBall.Entries.Count(); // THIS IS STUPID BUT APPARENTLY NECESSARY - DEAL WITH IT!!!
                foreach (var entry in tarBall.Entries)
                {
                    content.Add(new StreamContent(entry.OpenEntryStream()), entry.Key);
                }


                content.CopyToAsync(requestStream);
                requestStream.Seek(0, SeekOrigin.Begin);

                foreach (var header in content.Headers)
                {
                    headers.Add(header.Key, new StringValues(header.Value.ToArray()));
                }
            }

            var requestFeature = new HttpRequestFeature
            {
                Method = "PUT",
                Path = $"/v1/events/{subject}.{action}",
                Body = requestStream,
                Protocol = "http",
                Scheme = "http",
                Headers = headers
            };
            return requestFeature;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore()
                .AddJsonFormatters();

            // Add Kafka server and settings
            services.UseKafka(
                x =>
                {
                    x.BootstrapServers = new[] { "kafka:9092" };
                    x.Topics = new[] { "compiler.jobs" };
                    x.ConsumerGroup = "services.sample_customrequestfactory";
                    x.DefaultTopicConfig = new Dictionary<string, object>()
                    {
                        { "auto.offset.reset", "earliest" }
                    };
                },
                MultipartRequestFactory
                );

            services.UseEventSourcing()
                .WithProjection<CompilationJobCompleted>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseMvc();
        }
    }
}