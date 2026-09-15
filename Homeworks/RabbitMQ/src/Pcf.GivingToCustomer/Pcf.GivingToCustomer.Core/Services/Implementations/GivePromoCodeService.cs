using Pcf.GivingToCustomer.Core.Abstractions.Repositories;
using Pcf.GivingToCustomer.Core.Domain;
using Pcf.GivingToCustomer.Core.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pcf.GivingToCustomer.Core.Services.Implementations;

public class GivePromoCodeService : IGivePromoCodeService
{
    private readonly IRepository<PromoCode> _promoCodesRepository;
    private readonly IRepository<Preference> _preferencesRepository;
    private readonly IRepository<Customer> _customersRepository;

    public GivePromoCodeService(IRepository<PromoCode> promoCodesRepository,
        IRepository<Preference> preferencesRepository, IRepository<Customer> customersRepository)
    {
        _promoCodesRepository = promoCodesRepository;
        _preferencesRepository = preferencesRepository;
        _customersRepository = customersRepository;
    }

    public async Task<bool> GiveAsync(Guid promoCodeId, Guid partnerId, string code, string serviceInfo,
        Guid preferenceId, DateTime beginDate, DateTime endDate)
    {
        var preference = await _preferencesRepository.GetByIdAsync(preferenceId);
        if (preference == null)
            return false;

        var customers = await _customersRepository.GetWhere(c =>
            c.Preferences.Any(x => x.Preference.Id == preference.Id));

        var promoCode = new PromoCode
        {
            Id = promoCodeId,
            PartnerId = partnerId,
            Code = code,
            ServiceInfo = serviceInfo,
            BeginDate = beginDate,
            EndDate = endDate,
            Preference = preference,
            PreferenceId = preference.Id,
            Customers = customers.Select(customer => new PromoCodeCustomer
            {
                CustomerId = customer.Id,
                Customer = customer,
                PromoCodeId = promoCodeId
            }).ToList()
        };

        foreach (var relation in promoCode.Customers)
            relation.PromoCode = promoCode;

        await _promoCodesRepository.AddAsync(promoCode);
        return true;
    }
}