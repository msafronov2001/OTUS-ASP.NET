using MassTransit;
using Pcf.Administration.Core.Services.Interfaces;
using Pcf.Contracts;
using System.Threading.Tasks;

namespace Pcf.Administration.WebHost.Consumers;

public class PromoCodeReceivedConsumer : IConsumer<PromoCodeReceived>
{
    private readonly IEmployeeService _employeeService;

    public PromoCodeReceivedConsumer(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    public async Task Consume(ConsumeContext<PromoCodeReceived> context)
    {
        if (context.Message.PartnerManagerId.HasValue)
            await _employeeService.IncrementAppliedPromoCodesAsync(context.Message.PartnerManagerId.Value);
    }
}