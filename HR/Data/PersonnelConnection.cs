using System;
using System.IO;
using System.Text.Json;

namespace HR.Data
{
    public static class PersonnelConnection
    {
        public const string EnvironmentVariable = "PERSONNEL_CONNECTION";

        public const string Fallback =
            "Server=.;Database=Personnel;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

        public static string Resolve()
        {
            var fromEnvironment = Environment.GetEnvironmentVariable(EnvironmentVariable);
            if (!string.IsNullOrWhiteSpace(fromEnvironment))
                return fromEnvironment.Trim();

            var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (!File.Exists(path))
                return Fallback;

            try
            {
                using var document = JsonDocument.Parse(File.ReadAllText(path));
                if (document.RootElement.TryGetProperty("ConnectionStrings", out var section) &&
                    section.TryGetProperty("Personnel", out var value))
                {
                    var text = value.GetString();
                    if (!string.IsNullOrWhiteSpace(text))
                        return text.Trim();
                }
            }
            catch (JsonException)
            {
                return Fallback;
            }
            catch (IOException)
            {
                return Fallback;
            }

            return Fallback;
        }
    }
}
