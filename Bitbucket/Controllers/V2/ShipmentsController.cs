using Bitbucket.Models;
using Bitbucket.Responces;
using Bitbucket.Services;
using Bitbucket.Services.Fabrics;
using Microsoft.AspNetCore.Mvc;
using Prometheus;

namespace Bitbucket.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/shipments/")]
[ApiVersion("2.0")]
public class ShipmentsController : ControllerBase
{
    private readonly BloomFilterServiceFactory _bloomFilterFactory;
    private readonly PrometheusService _prometheusService;
    private readonly BarCodeGenerator _barCodeGenerator;
    private readonly ILogger<ShipmentsController> _logger;

    public ShipmentsController(
        BloomFilterServiceFactory bloomFilterFactory,
        ILogger<ShipmentsController> logger,
        PrometheusService prometheusService,
        BarCodeGenerator barCodeGenerator)
    {
        _bloomFilterFactory = bloomFilterFactory;
        _logger = logger;
        _prometheusService = prometheusService;
        _barCodeGenerator = barCodeGenerator;
    }

    [HttpGet("{shimpentId}")]
    public async Task<IActionResult> CheckBarcodeBloom(string shimpentId,
        CancellationToken cancellationToken)
    {
        using (_prometheusService.CreateDurationOperation("duartion_executing_operation_checkbarcode", "How long time executing action").NewTimer())
        {
            var shipmentRepository = _bloomFilterFactory.CreateRepository();
            var barcodeExist = await shipmentRepository.Contains(shimpentId, cancellationToken);
            if (barcodeExist)
            {
                return Ok(new ShipmentResponse { Value = barcodeExist });
            }
            return NotFound(new DomainError { Message = $"Shipments with barcode: {shimpentId} - not found in DB" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> AddShipment([FromQuery] int quantity, CancellationToken cancellationToken)
    {
        using (_prometheusService.CreateDurationOperation("duartion_executing_operation_add_shipments").NewTimer())
        {

            var massive = new List<Shipment>();
            var shipmentRepository = _bloomFilterFactory.CreateRepository();

            for (int i = 0; i < quantity; i++)
            {
                var shipment = new Shipment(_barCodeGenerator.GenerateBarCode(2, 2));
                await shipmentRepository.Add(shipment, cancellationToken);
                massive.Add(shipment);
            }
            return Created(Url.ActionLink(nameof(AddShipment)), new ShipmentResponse { Value = massive });
        }
    }
}