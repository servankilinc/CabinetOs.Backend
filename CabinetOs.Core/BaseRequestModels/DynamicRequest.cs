using CabinetOs.Core.Utils.DynamicQuery;

namespace CabinetOs.Core.BaseRequestModels;

public class DynamicRequest
{
    public Filter? Filter { get; set; }
    public IEnumerable<Sort>? Sorts { get; set; }
}
