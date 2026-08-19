using CabinetOs.Core.Utils.DynamicQuery;
using CabinetOs.Core.Utils.Pagination;

namespace CabinetOs.Core.BaseRequestModels;

public class DynamicPaginationRequest : PaginationRequest
{
    public Filter? Filter { get; set; }
    public IEnumerable<Sort>? Sorts { get; set; }
}
