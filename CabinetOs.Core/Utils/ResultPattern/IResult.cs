using System.Diagnostics.CodeAnalysis;

namespace CabinetOs.Core.Utils.ResultPattern
{
    public interface IResult
    {

        [MemberNotNullWhen(false, nameof(Error))]
        bool IsSuccess { get; }
        Error? Error { get; }
        string Message { get; set; }
    }
}