using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TaskMonitor.Utils
{
    public class JsonUtil
    {
        private static readonly JsonSerializerOptions _serializerOptions =
            new()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = JavaScriptEncoder.Default,
                PropertyNameCaseInsensitive = true,
            };

        public static TValue? Desserializar<TValue>([StringSyntax("Json")] string json) =>
            JsonSerializer.Deserialize<TValue>(json, _serializerOptions);

        public static string Serializar<TValue>(TValue value) =>
            JsonSerializer.Serialize(value, _serializerOptions);
    }
}
