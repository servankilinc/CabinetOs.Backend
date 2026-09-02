using CabinetOs.Core.Model;

namespace CabinetOs.Model.Dtos.Common;

/// <summary>
/// Canvas uzerinde tek bir nokta. Birim, <c>Device.CoordinateX/Y</c> ile AYNI
/// koordinat uzayidir (React Flow'un akis koordinatlari) — piksel degil, ekran koordinati degil.
/// </summary>
public class PointDto : IDto
{
    public double X { get; set; }
    public double Y { get; set; }
}
