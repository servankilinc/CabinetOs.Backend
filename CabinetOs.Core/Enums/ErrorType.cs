using System.ComponentModel;

namespace CabinetOs.Core.Enums;

public enum ErrorType : int
{
    Failure = 100,
    NotFound = 200,
    Validation = 300,
    Forbidden = 400
}
