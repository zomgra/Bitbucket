using Bitbucket.Models;
using Bitbucket.Responces;
using Bitbucket.Services;
using Bitbucket.Services.Fabrics;
using Microsoft.AspNetCore.Mvc;
using Prometheus;
using System.Diagnostics;

namespace Bitbucket.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/shipments/")]
[ApiVersion("1.0")]
public class ShipmentsController : ControllerBase
{
    private readonly BloomFilterServiceFactory _bloomFilterFactory;
    private readonly PrometheusService _prometheusService;
    private readonly ILogger<ShipmentsController> _logger;

    public ShipmentsController(
        BloomFilterServiceFactory bloomFilterFactory,
        ILogger<ShipmentsController> logger,
        PrometheusService prometheusService)
    {
        _bloomFilterFactory = bloomFilterFactory;
        _logger = logger;
        _prometheusService = prometheusService;
    }
    /// <summary>
    /// Check barcode in DB with Bloom Filter
    /// </summary>
    /// <param name="barcode">Barcode for check</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Have barcode in Data Base</returns>
    [HttpGet]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> CheckBarcodeBloom([FromQuery] string shimpentId,
        CancellationToken cancellationToken)
    {
        using (_prometheusService.CreateDurationOperation().NewTimer())
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

    /// <summary>
    /// Add shipment to db using bloom filter
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> AddShipment([FromQuery] int quantity, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Blabla");
        _logger.LogError("Blabla");
        var massive = new List<Shipment>();
        var shipmentRepository = _bloomFilterFactory.CreateRepository();
        
        for (int i = 0; i < quantity; i++)
        {
            var shipment = await shipmentRepository.Create(cancellationToken);
            massive.Add(shipment);
        }
        return Created(Url.ActionLink(nameof(AddShipment)), new ShipmentResponse { Value = massive });

    }
}