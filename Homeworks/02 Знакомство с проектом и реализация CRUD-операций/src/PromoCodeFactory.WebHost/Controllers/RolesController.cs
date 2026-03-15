using Microsoft.AspNetCore.Mvc;
using PromoCodeFactory.WebHost.Mapping;
using PromoCodeFactory.WebHost.Models;

namespace PromoCodeFactory.WebHost.Controllers;

/// <summary>
/// Роли сотрудников
/// </summary>
public class RolesController(IRepository<Role> rolesRepository) : BaseController
{
    /// <summary>
    /// Получить все доступные роли сотрудников
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RoleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<RoleResponse>>> Get(CancellationToken ct)
    {
        var roles = await rolesRepository.GetAll(ct);

        var rolesModels = roles.Select(Mapper.ToRoleResponse).ToList();

        return Ok(rolesModels);
    }

    /// <summary>
    /// Получить данные роли по Id
    /// </summary>
    /// <param name="id">Параметр поиска - ID</param>
    /// <returns>Роль c определенным ID</returns>
    /// <response code="200">Запрос на вывод роли по ID успешно выполнен</response>
    /// <response code="404">Роль по данному ID не найдена</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoleResponse>> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var role = await rolesRepository.GetById(id, ct);

        if (role == null)
            return NotFound($"Роль с ID - {id} не найдена");

        var roleModel = Mapper.ToRoleResponse(role);

        return Ok(roleModel);

    }


    /// <summary>
    /// Создать роль
    /// </summary>
    /// <param name="request">Данные для создания роли</param>
    /// <returns>Новая роль</returns>
    /// <response code="201">Запрос на создание роли успешно выполнен</response>
    /// <response code="400">Ошибка при создание роли</response>
    [HttpPost]
    [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RoleResponse>> Create([FromBody] RoleCreateRequest request, CancellationToken ct)
    {

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var role = Mapper.ToRole(request);

        await rolesRepository.Add(role, ct);

        var response = Mapper.ToRoleResponse(role);

        return CreatedAtAction(
            nameof(GetById),
            new { id = role.Id },
            response);

    }

}
