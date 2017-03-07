﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace System.EventSourcing.AspNetCore.Kafka
{
    public static class WebHostBuilderExtension
    {
        public static IWebHostBuilder UseKafka(this IWebHostBuilder subject)
        {
            subject.ConfigureServices(svc =>
            {
                svc.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
                svc.AddTransient<IHttpContextFactory, HttpContextFactory>();
                svc.AddSingleton<IServer, KafkaListener>();
            });

            return subject;
        }
    }
}
