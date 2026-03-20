using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PromoCodeFactory.WebHost.Mapping;
using PromoCodeFactory.WebHost.Models;

namespace PromoCodeFactory.WebHost.Controllers;

/// <summary>
/// Сотрудники
/// </summary>
public class EmployeesController(
    IRepository<Employee> employeeRepository,
    IRepository<Role> roleRepository
    ) : BaseController
{
    /// <summary>
    /// Получить данные всех сотрудников
    /// </summary>
    /// <returns>Список сотрудников</returns>
    /// <response code="200">Запрос на вывод всех сотрудников успешно выполнен</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EmployeeShortResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EmployeeShortResponse>>> Get(CancellationToken ct)
    {
        var employees = await employeeRepository.GetAll(ct);

        var employeesModels = employees.Select(Mapper.ToEmployeeShortResponse).ToList();

        return Ok(employeesModels);
    }

    /// <summary>
    /// Получить данные сотрудника по Id
    /// </summary>
    /// <param name="id">Параметр поиска - ID</param>
    /// <returns>Cотрудник c определенным ID</returns>
    /// <response code="200">Запрос на вывод сотрудника по ID успешно выполнен</response>
    /// <response code="404">Сотрудник по данному ID не найден</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeResponse>> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var employees = await employeeRepository.GetById(id, ct);

        if (employees == null)
            return NotFound($"Сотрудник с ID - {id} не найден");

        var employeesModel = Mapper.ToEmployeeResponse(employees);

        return Ok(employeesModel);

    }

    /// <summary>
    /// Создать сотрудника
    /// </summary>
    /// <param name="request">Данные для создания сотрудника</param>
    /// <returns>Новый сотрудник</returns>
    /// <response code="201">Запрос на создание сотрудника успешно выполнен</response>
    /// <response code="400">Ошибка при создание сотрудника</response>
    [HttpPost]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EmployeeResponse>> Create([FromBody] EmployeeCreateRequest request, CancellationToken ct)
    {

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var role = await roleRepository.GetById(request.RoleId, ct);

        if (role == null)
        {
            return BadRequest("Роль сотрудника не указана");
        }

        var employee = Mapper.ToEmployee(request, role);

        await employeeRepository.Add(employee, ct);

        var response = Mapper.ToEmployeeResponse(employee);

        return CreatedAtAction(
            nameof(GetById),
            new { id = employee.Id },
            response);

    }

    /// <summary>
    /// Обновить сотрудника
    /// </summary>
    /// <param name="id">ID для изменения определенного сотрудника</param>
    /// <param name="request">Данные для изменения сотрудника</param>
    /// <returns>Сотрудник с изменнеными данными</returns>
    /// <response code="200">Запрос на изменение данных сотрудника успешно выполнен</response>
    /// <response code="400">Ошибка изменение данных сотрудника</response>
    /// <response code="404">Сотрудник по данному ID не найден</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeResponse>> Update(
        [FromRoute] Guid id,
        [FromBody] EmployeeUpdateRequest request,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var employee = await employeeRepository.GetById(id, ct);

        if (employee == null)
            return NotFound($"Сотрудник с ID - {id} не найден");

        var role = await roleRepository.GetById(request.RoleId, ct);

        if (role == null)
            return BadRequest("Роль сотрудника не указана.");

        employee.FirstName = request.FirstName;
        employee.LastName = request.LastName;
        employee.Email = request.Email;
        employee.Role = role;

        try
        {
            await employeeRepository.Update(employee, ct);
        }
        catch (EntityNotFoundException)
        {
            return NotFound("Сотрудник не найден" );
        }

        return Ok(Mapper.ToEmployeeResponse(employee));


    }

    /// <summary>
    /// Удалить сотрудника
    /// </summary>
    /// <param name="id">ID для удаления определенного сотрудника</param>
    /// <returns>Сотрудник с данным ID удален</returns>
    /// <response code="204">Сотрудник удален</response>
    /// <response code="404">Сотрудник по данному ID не найден</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        try
        {
            await employeeRepository.Delete(id, ct);
            return NoContent();
        }
        catch (EntityNotFoundException)
        {
            return NotFound("Сотрудник не найден");
        }
    }
}
