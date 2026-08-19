using CabinetOs.Core.Utils.Datatable;
using CabinetOs.Core.Utils.DynamicQuery;

namespace CabinetOs.Core.BaseRequestModels;

public class DynamicDatatableRequest : DatatableRequest
{
    public Filter? Filter { get; set; }
    public IEnumerable<Sort>? Sorts { get; set; }
}