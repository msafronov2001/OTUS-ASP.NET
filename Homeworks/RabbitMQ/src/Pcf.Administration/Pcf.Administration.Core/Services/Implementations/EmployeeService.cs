using Pcf.Administration.Core.Abstractions.Repositories;
using Pcf.Administration.Core.Domain.Administration;
using Pcf.Administration.Core.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace Pcf.Administration.Core.Services.Implementations;

public class EmployeeService : IEmployeeService
{
    private readonly IRepository<Employee> _employeeRepository;

    public EmployeeService(IRepository<Employee> employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task<bool> IncrementAppliedPromoCodesAsync(Guid employeeId)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee == null)
            return false;

        employee.AppliedPromocodesCount++;
        await _employeeRepository.UpdateAsync(employee);
        return true;
    }
}