using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Mapping;
using PromoCodeFactory.WebHost.Models.Customers;

namespace PromoCodeFactory.WebHost.Controllers;

/// <summary>
/// Клиенты
/// </summary>
public class CustomersController : BaseController
{
    private readonly IRepository<Customer> _customerRepository;
    private readonly IRepository<Preference> _preferenceRepository;
    private readonly IRepository<PromoCode> _promoCodeRepository;

    public CustomersController(IRepository<Customer> customerRepository, IRepository<Preference> preferenceRepository,
        IRepository<PromoCode> promoCodeRepository)
    {
        _customerRepository = customerRepository;
        _preferenceRepository = preferenceRepository;
        _promoCodeRepository = promoCodeRepository;
    }

    /// <summary>
    /// Получить данные всех клиентов
    /// </summary>
    /// <response code="200">Запрос на вывод всех клиентов успешно выполнен</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CustomerShortResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CustomerShortResponse>>> Get(CancellationToken ct)
    {
        var customers = await _customerRepository.GetAll(true,ct);
        var customersModel = customers.Select(CustomersMapper.ToCustomerShortResponse).ToList();
        return Ok(customersModel);
    }

    /// <summary>
    /// Получить данные клиента по Id
    /// </summary>
    /// <returns>Клиент c определенным ID</returns>
    /// <param name="id">Параметр поиска - ID</param>
    /// <response code="200">Запрос на вывод клиента по ID успешно выполнен</response>
    /// <response code="404">Клиент по данному ID не найден</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerResponse>> GetById(Guid id, CancellationToken ct)
    {
        var customer = await _customerRepository.GetById(id,true,ct);

        if (customer == null)
            return NotFound($"Клиент с ID - {id} не найден");

        var promoCodeIds = customer.CustomerPromoCodes.Select(pmc => pmc.PromoCodeId).Distinct().ToList();

        var promoCodes = promoCodeIds.Count == 0
            ? Array.Empty<PromoCode>()
            : await _promoCodeRepository.GetByRangeId(promoCodeIds, withIncludes: true, ct: ct);

        var promoCodeById = promoCodes.ToDictionary(pmc => pmc.Id, pmc => pmc);

        var promoCodeResponses = customer.CustomerPromoCodes
            .Where(link => promoCodeById.ContainsKey(link.PromoCodeId))
            .Select(link => CustomerPromoCodesMapper.ToCustomerPromoCodeResponse(promoCodeById[link.PromoCodeId], link))
            .ToList();

        var response = new CustomerResponse(
            customer.Id,
            customer.FirstName,
            customer.LastName,
            customer.Email,
            customer.Preferences.Select(PreferencesMapper.ToPreferenceShortResponse).ToList(),
            promoCodeResponses);

        return Ok(response);
    }

    /// <summary>
    /// Создать клиента
    /// </summary>
    /// <param name="request">Данные для создания клиента</param>
    /// <returns>Новый клиент</returns>
    /// <response code="201">Запрос на создание клиента успешно выполнен</response>
    /// <response code="400">Ошибка при создание клиента</response>
    [HttpPost]
    [ProducesResponseType(typeof(CustomerShortResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CustomerShortResponse>> Create([FromBody] CustomerCreateRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var preferences = await _preferenceRepository.GetByRangeId(request.PreferenceIds, false, ct);
        if (preferences.Count != request.PreferenceIds.Distinct().Count())
            return BadRequest($"Предпочтения с ID - {request.PreferenceIds} не найдены");

        var customer = CustomersMapper.ToCustomer(request, preferences);
 
        try
        {
            await _customerRepository.Add(customer, ct);
        }
        catch (DbUpdateException ex)
        {
            var innerException = ex.InnerException?.Message.ToLower() ?? string.Empty;

            if (innerException.Contains("duplicate") ||
                innerException.Contains("unique"))
            {
                return BadRequest($"Запись с такими данными уже существует - {customer.Id}, {customer.Preferences}");
            }

        }

        var response = CustomersMapper.ToCustomerShortResponse(customer);

        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, response);
    }

    /// <summary>
    /// Обновить клиента
    /// </summary>
    /// <param name="id">ID для изменения определенного клиента</param>
    /// <param name="request">Данные для изменения клиента</param>
    /// <returns>Клиент с измененными данными</returns>
    /// <response code="200">Запрос на изменение данных клиента успешно выполнен</response>
    /// <response code="400">Ошибка изменение данных клиента</response>
    /// <response code="404">Клиент по данному ID не найден</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CustomerShortResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerShortResponse>> Update(
        [FromRoute] Guid id,
        [FromBody] CustomerUpdateRequest request,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var customer = await _customerRepository.GetById(id,true,ct);
        if (customer == null)
            return NotFound($"Клиент с ID - {id} не найден");

        var preferences = await _preferenceRepository.GetByRangeId(request.PreferenceIds,false, ct);
        if (preferences.Count != request.PreferenceIds.Distinct().Count())
            return BadRequest($"Предпочтения с ID - {request.PreferenceIds} не найдены");

        CustomersMapper.ApplyUpdates(customer, request, preferences);

        try
        {
            await _customerRepository.Update(customer, ct);
        }
        catch (EntityNotFoundException)
        {
            return NotFound();
        }

        return Ok(CustomersMapper.ToCustomerShortResponse(customer));
    }

    /// <summary>
    /// Удалить клиента
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            await _customerRepository.Delete(id, ct);
            return NoContent();
        }
        catch (EntityNotFoundException)
        {
            return NotFound();
        }
    }
}
