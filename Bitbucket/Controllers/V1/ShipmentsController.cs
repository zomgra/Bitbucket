using Bitbucket.Models;
using Bitbucket.Responces;
using Bitbucket.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Diagnostics;
namespace Bitbucket.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/shipments")]
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
    /// Check barcode in DB with Bloom Filter
    /// </summary>
    /// <param name="barcode">Barcode for check</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Have barcode in Data Base</returns>
    [HttpGet("check/bloom")]
    [MapToApiVersion("1.0")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Problem on the server", Type = typeof(ShipmentResponse))]
    public async Task<IActionResult> CheckBarcodeBloom(string barcode,
        CancellationToken cancellationToken)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var barcodeExist = await _bloomFilterService.Contains(barcode, cancellationToken);

        stopwatch.Stop();
        if (barcodeExist)
        {
            return Ok(new ShipmentResponse { Time = stopwatch.ElapsedMilliseconds, Value = true });
        }
        return NotFound(new ShipmentResponse { Time = stopwatch.ElapsedMilliseconds, Value = false });
    }

    [HttpPost("add/bloom")]
    [MapToApiVersion("1.0")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Problem on the server", Type = typeof(ShipmentResponse))]
    public async Task<IActionResult> AddShipmentBloom(CancellationToken cancellationToken)
    {
        Stopwatch stopwatch = new();
        stopwatch.Start();
        var shipment = new Shipment();
        await _bloomFilterService.Add(shipment, cancellationToken);

        stopwatch.Stop();

        return Ok(new ShipmentResponse { Time = stopwatch.ElapsedMilliseconds, Value = shipment });
    }

    [HttpPost("add-many/bloom")]
    [MapToApiVersion("1.0")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Problem on the server", Type = typeof(ShipmentResponse))]

    public async Task<IActionResult> AddTestShipmentsBloom(int count,
        CancellationToken cancellationToken)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        for (int i = 0; i < count; i++)
        {
            _logger.LogInformation("Added {i}", i);
            await _bloomFilterService.Add(new Shipment(), cancellationToken);
        }

        stopwatch.Stop();
        return Ok(new ShipmentResponse { Value = true, Time = stopwatch.ElapsedMilliseconds });
    }
}