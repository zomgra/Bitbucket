using Bitbucket.Models;
using Bitbucket.Responces;
using Bitbucket.Services;
using Microsoft.AspNetCore.Mvc;
using Prometheus;
using System.Diagnostics;

namespace Bitbucket.Controllers.V1;

[ApiController]
[Route("api/v{version:apiVersion}/shipments/")]
[ApiVersion("1.0")]
public class ShipmentsController : ControllerBase
{
    private readonly BloomFilterService _bloomFilterService;
    private readonly PrometheusService _prometheusService;
    private readonly ILogger<ShipmentsController> _logger;

    public ShipmentsController(
        BloomFilterService bloomFilterService,
        ILogger<ShipmentsController> logger,
        PrometheusService prometheusService)
    {
        _bloomFilterService = bloomFilterService;
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
            var barcodeExist = await _bloomFilterService.Contains(shimpentId, cancellationToken);
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
        var massive = new List<Shipment>();
        var counter = _prometheusService.CreateDurationOperation();
        var stopwatcher = Stopwatch.StartNew();

        for (int i = 0; i < quantity; i++)
        {
            _logger.LogInformation("Added {i}", i);
            var shipment = await _bloomFilterService.Create(cancellationToken);
            massive.Add(shipment);
        }
        stopwatcher.Stop();
        counter.IncTo(stopwatcher.ElapsedMilliseconds);
        return Created(Url.ActionLink(nameof(AddShipment)), new ShipmentResponse { Value = massive });

    }
}