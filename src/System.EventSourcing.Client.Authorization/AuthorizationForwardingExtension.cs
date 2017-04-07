using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.EventSourcing.Authorization;
using System.Linq;
using System.Threading.Tasks;

namespace System.EventSourcing.Client.Authorization
{
    public static class AuthorizationForwardingExtension
    {
        public static EventClient UseAuthorizationForwarding(this EventClient subject, IServiceProvider serviceProvider)
        {
            var reqestSvc = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            subject.UseParser(
                (evt, type, evnt) =>
                {
                    if (reqestSvc.HttpContext != null
                        && reqestSvc.HttpContext.User != null )
                    {
                        var sub = reqestSvc.HttpContext.User.Claims.FirstOrDefault(x => x.Type == "sub");
                        if(sub != null)
                        {
                            evnt.Tags.Add(AuthrorizationTags.Subscriber, sub.Value);
                        }
                    }
                    return Task.CompletedTask;
                });

            return subject;
        }
    }
}
