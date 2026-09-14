namespace Pcf.Contracts;

public record PromoCodeReceived(
    Guid PromoCodeId,
    Guid PartnerId,
    string PromoCode,
    string ServiceInfo,
    Guid PreferenceId,
    DateTime BeginDate,
    DateTime EndDate,
    Guid? PartnerManagerId);