using MassTransit;
using Pcf.Contracts;
using Pcf.GivingToCustomer.Core.Services.Interfaces;
using System.Threading.Tasks;

namespace Pcf.GivingToCustomer.WebHost.Consumers;

public class PromoCodeReceivedConsumer : IConsumer<PromoCodeReceived>
{
    private readonly IGivePromoCodeService _service;

    public PromoCodeReceivedConsumer(IGivePromoCodeService service)
    {
        _service = service;
    }

    public async Task Consume(ConsumeContext<PromoCodeReceived> context)
    {
        var message = context.Message;
        await _service.GiveAsync(message.PromoCodeId, message.PartnerId, message.PromoCode,
            message.ServiceInfo, message.PreferenceId, message.BeginDate, message.EndDate);
    }
}