namespace ConditionPredictor.Web.ProgramExtensions
{
    public static class ConfigurationExtensions
    {
        public static string GetRequiredValue(this IConfiguration config, string key)
        {
            var value = config.GetValue<string>(key);
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidOperationException(
                    $"Configuration value '{key}' is missing or empty."
                );
            return value;
        }
    }
}
