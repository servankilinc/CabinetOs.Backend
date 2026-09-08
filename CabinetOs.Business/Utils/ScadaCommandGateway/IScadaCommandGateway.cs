using CabinetOs.Model.Dtos.Scada.Commands;

namespace CabinetOs.Business.Utils.ScadaCommandGateway;

public interface IScadaCommandGateway
{
    /// <summary>
    /// Named client. Program.cs'te <c>AddHttpClient("http_client_scada")</c> ile kayitli;
    /// yeni <c>HttpClient</c> kurmak yerine fabrika kullanmanin sebebi soket
    /// tuketimi degil DNS: uzun omurlu tek bir <c>HttpClient</c>, SCADA'nin IP'si
    /// degistiginde eski adrese baglanmaya devam eder.
    /// </summary>
    public const string HttpClientName = "http_client_scada";
    Task<ScadaCommandResponse> SendAsync(string baseUrl, ScadaCommandEnvelope envelope, TimeSpan timeout);
}


