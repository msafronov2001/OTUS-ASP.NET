using Microsoft.AspNetCore.Mvc;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Mapping;
using PromoCodeFactory.WebHost.Models.PromoCodes;

namespace PromoCodeFactory.WebHost.Controllers;

/// <summary>
/// Промокоды
/// </summary>
public class PromoCodesController : BaseController
{
    private readonly IRepository<CustomerPromoCode> _customerPromoCodeRepository;
    private readonly IRepository<Customer> _customerRepository;
    private readonly IRepository<Employee> _employeeRepository;
    private readonly IRepository<PromoCode> _promoCodeRepository;
    private readonly IRepository<Preference> _preferenceRepository;

    public PromoCodesController(IRepository<CustomerPromoCode> customerPromoCodeRepository, IRepository<Employee> employeeRepository,
        IRepository<PromoCode> promoCodeRepository, IRepository<Preference> preferenceRepository, IRepository<Customer> customerRepository)
    {
        _customerPromoCodeRepository = customerPromoCodeRepository;
        _employeeRepository = employeeRepository;
        _promoCodeRepository = promoCodeRepository;
        _preferenceRepository = preferenceRepository;
        _customerRepository = customerRepository;
    }
    /// <summary>
    /// Получить все промокоды
    /// </summary>
    /// <returns>Список промокодов</returns>
    /// <response code="200">Запрос на вывод всех промокодов успешно выполнен</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PromoCodeShortResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PromoCodeShortResponse>>> Get(CancellationToken ct)
    {
        var promoCode = await _promoCodeRepository.GetAll(true, ct);
        var promoCodeModel = promoCode.Select(PromoCodesMapper.ToPromoCodeShortResponse).ToList();
        return Ok(promoCodeModel);
    }

    /// <summary>
    /// Получить промокод по id
    /// </summary>
    /// <param name="id">Параметр поиска - ID</param>
    /// <returns>Промокод c определенным ID</returns>
    /// <response code="200">Запрос на вывод промокода по ID успешно выполнен</response>
    /// <response code="404">Промокод по данному ID не найден</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PromoCodeShortResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PromoCodeShortResponse>> GetById(Guid id, CancellationToken ct)
    {
        var promoCode = await _promoCodeRepository.GetById(id, true, ct);
        if (promoCode == null)
            return NotFound($"Промокод с ID - {id} не найден");

        var promoCodeMolel = PromoCodesMapper.ToPromoCodeShortResponse(promoCode);

        return Ok(promoCodeMolel);
    }

    /// <summary>
    /// Создать промокод и выдать его клиентам с указанным предпочтением
    /// </summary>
    /// <returns>Новый промокод</returns>
    /// <response code="201">Запрос на создание промокода успешно выполнен</response>
    /// <response code="400">Ошибка при создание промокода</response>
    /// <response code="404">Ошибка при создание промокода,данные не найдены</response>
    [HttpPost]
    [ProducesResponseType(typeof(PromoCodeShortResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PromoCodeShortResponse>> Create(PromoCodeCreateRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var partnerManager = await _employeeRepository.GetById(request.PartnerManagerId, true, ct);
        if (partnerManager == null)
            return NotFound($"Менеджер партнеров с ID - '{request.PartnerManagerId}' не найден.");

        var preference = await _preferenceRepository.GetById(request.PreferenceId, false, ct);
        if (preference == null)
            return NotFound($"Предпочтения с ID -'{request.PreferenceId}' не найдены.");

        if (request.EndDate < request.BeginDate)
            return BadRequest("Дата окончания должна быть больше либо равна дате начала.");

        var promoCode = new PromoCode
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            ServiceInfo = request.ServiceInfo,
            PartnerName = request.PartnerName,
            BeginDate = request.BeginDate,
            EndDate = request.EndDate,
            PartnerManager = partnerManager,
            Preference = preference
        };

        await _promoCodeRepository.Add(promoCode, ct);

        var customers = await _customerRepository.GetWhere(
            c => c.Preferences.Any(p => p.Id == request.PreferenceId),
            withIncludes: false,
            ct: ct);

        foreach (var customer in customers)
        {
            var link = new CustomerPromoCode
            {
                Id = Guid.NewGuid(),
                CustomerId = customer.Id,
                PromoCodeId = promoCode.Id,
                CreatedAt = DateTimeOffset.UtcNow,
                AppliedAt = null
            };

            await _customerPromoCodeRepository.Add(link, ct);
        }

        return CreatedAtAction(nameof(GetById), new { id = promoCode.Id }, PromoCodesMapper.ToPromoCodeShortResponse(promoCode));
    }

    /// <summary>
    /// Применить промокод (отметить, что клиент использовал промокод)
    /// </summary>
    [HttpPost("{id:guid}/apply")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Apply(
        [FromRoute] Guid id,
        [FromBody] PromoCodeApplyRequest request,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // промокод существует
        var promoCode = await _promoCodeRepository.GetById(id, false,  ct);
        if (promoCode == null)
            return NotFound($"Промокод с ID - '{id}' не найден.");

        // клиент существует
        var customer = await _customerRepository.GetById(request.CustomerId, false, ct);
        if (customer == null)
            return NotFound($"Клиент с ID - '{request.CustomerId}' не найден.");

        var links = await _customerPromoCodeRepository.GetWhere(
            c => c.CustomerId == request.CustomerId && c.PromoCodeId == id,
            false, ct);

        var link = links.FirstOrDefault();
        if (link == null)
            return NotFound($"Клиента(ID - '{request.CustomerId}') с таким промокодом не найдено");

        if (link.AppliedAt != null)
            return BadRequest("Промо код уже был применен к данному пользователю.");

        link.AppliedAt = DateTimeOffset.UtcNow;

        try
        {
            await _customerPromoCodeRepository.Update(link, ct);
        }
        catch (EntityNotFoundException)
        {
            return NotFound();
        }

        return NoContent();
    }
}
