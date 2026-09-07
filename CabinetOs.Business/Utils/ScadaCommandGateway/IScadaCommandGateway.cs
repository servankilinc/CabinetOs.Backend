using CabinetOs.Model.Dtos.Scada.Commands;

namespace CabinetOs.Business.Utils.ScadaCommandGateway;

public interface IScadaCommandGateway
{
    Task<ScadaCommandResponse> SendAsync(string baseUrl, ScadaCommandEnvelope envelope, TimeSpan timeout);
}


