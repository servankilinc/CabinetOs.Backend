using System.Text.Json;
using System.Text.Json.Serialization;

namespace CabinetOs.Core.Utils;

public static class ApiJsonOptions
{
    public static readonly JsonSerializerOptions ApiJson = CreateApiJson();

    public static void Apply(JsonSerializerOptions options)
    {
        // DTO/response ozellikleri camelCase.
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

        // Sozluk ANAHTARLARI donusturulmez - ProblemDetails.errors'in PascalCase kalmasinin sebebi budur. Bilerek null.
        options.DictionaryKeyPolicy = null;

        // Gelen govde PascalCase de olsa baglanir.
        options.PropertyNameCaseInsensitive = true;

        // Web varsayilani sayilari string'ten de okuyabilir ("1" -> 1). 
        options.NumberHandling = JsonNumberHandling.AllowReadingFromString;

        // null alanlar govdede kalir
        options.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
    }

    private static JsonSerializerOptions CreateApiJson()
    {
        // Web varsayilani ile baslat. Bu, camelCase ozellik adlari ve sayilari string'ten okuyabilme gibi ayarlari icerir.
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        Apply(options);
        return options;
    }
}
