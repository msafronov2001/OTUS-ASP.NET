using Microsoft.AspNetCore.Mvc;
using PromoCodeFactory.Core.Domain.PromoCodeManagement;
using PromoCodeFactory.WebHost.Mapping;
using PromoCodeFactory.WebHost.Models.Preferences;

namespace PromoCodeFactory.WebHost.Controllers;

/// <summary>
/// Предпочтения
/// </summary>
public class PreferencesController : BaseController
{
    private readonly IRepository<Preference> _preferenceRepository;

    public PreferencesController(IRepository<Preference> preferenceRepository)
    {

        _preferenceRepository = preferenceRepository;

    }
    /// <summary>
    /// Получить все доступные предпочтения
    /// </summary>
    /// <returns>Список предпочтении</returns>
    /// <response code="200">Запрос на вывод всех предпочтений успешно выполнен</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PreferenceShortResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PreferenceShortResponse>>> Get(CancellationToken ct)
    {
        var preferences = await _preferenceRepository.GetAll(true, ct);
        var preferenceModel = preferences.Select(PreferencesMapper.ToPreferenceShortResponse).ToList();
        return Ok(preferenceModel);
    }
}
