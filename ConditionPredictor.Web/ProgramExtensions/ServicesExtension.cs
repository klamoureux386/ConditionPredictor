using ConditionPredictor.Web.Services;
using Microsoft.Extensions.Configuration;
using System.Runtime.Intrinsics.Arm;

namespace ConditionPredictor.Web.ProgramExtensions
{
    public static class ServicesExtension
    {
        public static IServiceCollection SetupServices(this IServiceCollection services) 
        {
            services.AddScoped<CTakesService, CTakesService>();
            services.AddScoped<EvaluatorService, EvaluatorService>();
            services.AddScoped<PubMedService, PubMedService>();

            return services;
        }

    }
}
