using ConditionPredictor.ApiService.Services;

namespace ConditionPredictor.ApiService.ProgramExtensions
{
    public static class ServicesExtension
    {
        public static IServiceCollection SetupServices(this IServiceCollection services) 
        {
            services.AddScoped<EvaluatorService, EvaluatorService>();
            services.AddScoped<PubMedService, PubMedService>();
            return services;
        }

    }
}
