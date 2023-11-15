using Bitbucket.Models;
using Bitbucket.Responces;
using Bitbucket.Services;
using Microsoft.AspNetCore.Mvc;

namespace Bitbucket.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/shipments/")]
[ApiVersion("2.0")]
public class ShipmentsController : ControllerBase
{
    private readonly ShipmentService _shipmentService;
    private readonly ILogger<ShipmentsController> _logger;

    public ShipmentsController(
        ShipmentService shipmentService,
        ILogger<ShipmentsController> logger)
    {
        _shipmentService = shipmentService;
        _logger = logger;
    }
    [HttpGet()]
    [MapToApiVersion("2.0")]
    public async Task<IActionResult> CheckBarcode(string barcode,
        CancellationToken cancellationToken)
    {

        var barcodeExist = await _shipmentService.Contains(barcode, cancellationToken);

        if (barcodeExist)
        {
            return Ok(new ShipmentResponse { Value = true });
        }
        return NotFound(new ShipmentResponse { Value = false });
    }

    [MapToApiVersion("2.0")]
    [HttpPost()]
    public async Task<IActionResult> AddShipment([FromQuery]int quantity, CancellationToken cancellationToken)
    {
        var massive = new List<Shipment>();
        for (int i = 0; i < quantity; i++)
        {
            _logger.LogInformation("Added {i}", i);
            var shipment = await _shipmentService.Create(cancellationToken);
            massive.Add(shipment);
        }
        return Created(Url.ActionLink(nameof(AddShipment)), new ShipmentResponse { Value = massive});
    }
}