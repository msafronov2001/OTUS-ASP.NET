using System;
using System.Threading.Tasks;

namespace Pcf.Administration.Core.Services.Interfaces;

public interface IEmployeeService
{
    Task<bool> IncrementAppliedPromoCodesAsync(Guid employeeId);
}