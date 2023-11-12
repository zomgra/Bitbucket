using Bitbucket.Models;
using Bitbucket.Responces;
using Bitbucket.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Diagnostics;
namespace Bitbucket.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/shipments")]
[ApiVersion("2.0")]
public class ShipmentsController : ControllerBase
{
    private readonly BloomFilterService _bloomFilterService;
    private readonly ShipmentService _shipmentService;
    private readonly ILogger<ShipmentsController> _logger;

    public ShipmentsController(BloomFilterService bloomFilterService,
        ShipmentService shipmentService,
        ILogger<ShipmentsController> logger)
    {
        _shipmentService = shipmentService;
        _bloomFilterService = bloomFilterService;
        _logger = logger;
    }

    /// <summary>
    /// Check barcode in DB with out Bloom Filter
    /// </summary>
    /// <param name="barcode">Barcode for check</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Have barcode in Data Base</returns>
    [HttpGet("check")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Problem on the server", Type = typeof(ShipmentResponse))]
    [MapToApiVersion("2.0")]
    public async Task<IActionResult> CheckBarcode(string barcode,
        CancellationToken cancellationToken)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        var barcodeExist = await _shipmentService.Contains(barcode, cancellationToken);

        stopwatch.Stop();
        if (barcodeExist)
        {
            return Ok(new ShipmentResponse { Time = stopwatch.ElapsedMilliseconds, Value = true });
        }
        return NotFound(new ShipmentResponse { Time = stopwatch.ElapsedMilliseconds, Value = false });
    }
    [MapToApiVersion("2.0")]
    [HttpPost("add")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Problem on the server", Type = typeof(ShipmentResponse))]
    public async Task<IActionResult> AddShipment(CancellationToken cancellationToken)
    {
        Stopwatch stopwatch = new();
        stopwatch.Start();
        var shipment = new Shipment();
        await _shipmentService.Add(shipment, cancellationToken);

        stopwatch.Stop();

        return Ok(new ShipmentResponse { Time = stopwatch.ElapsedMilliseconds, Value = shipment });
    }

    [HttpPost("add-many")]
    [MapToApiVersion("2.0")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Problem on the server", Type = typeof(ShipmentResponse))]
    public async Task<IActionResult> AddTestShipments(int count,
        CancellationToken cancellationToken)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        for (int i = 0; i < count; i++)
        {
            _logger.LogInformation("Added {i}", i);
            await _shipmentService.Add(new Shipment(), cancellationToken);
        }

        stopwatch.Stop();
        return Ok(new ShipmentResponse { Value = true, Time = stopwatch.ElapsedMilliseconds });
    }
}