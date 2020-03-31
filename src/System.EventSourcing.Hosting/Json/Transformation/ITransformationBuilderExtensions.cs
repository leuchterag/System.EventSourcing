using System.EventSourcing.Hosting.Transformation;
using System.Text.RegularExpressions;

namespace System.EventSourcing.Hosting.Json.Transformation
{
    public static class ITransformationBuilderExtensions
    {
        public static ITransformationBuilder<TContext> KeysMatchingRegex<TContext>(
            this ITransformationMiddlewareBuilder<TContext> transformationBuilder,
            string regexpr)
            where TContext : StringJObjectContext
        {
            var regex = new Regex(regexpr, RegexOptions.Compiled);
            return transformationBuilder.KeysMatching((ctx) => regex.IsMatch(ctx.Key));
        }
    }
}