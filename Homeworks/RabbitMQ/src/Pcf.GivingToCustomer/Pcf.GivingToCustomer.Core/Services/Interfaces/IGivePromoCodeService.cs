using System;
using System.Threading.Tasks;

namespace Pcf.GivingToCustomer.Core.Services.Interfaces;

public interface IGivePromoCodeService
{
    Task<bool> GiveAsync(Guid promoCodeId, Guid partnerId, string code, string serviceInfo,
        Guid preferenceId, DateTime beginDate, DateTime endDate);
}