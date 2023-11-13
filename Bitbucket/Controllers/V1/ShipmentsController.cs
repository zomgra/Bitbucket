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
[Produces("application/json")]
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
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Problem on the server", Type = typeof(DomainErrorResponse))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Shipment with barcode not found in DB using bloom filter", Type = typeof(ShipmentResponse))]
    [SwaggerResponse(StatusCodes.Status200OK, "Shipment with barcode found in DB using  bloom filter", Type = typeof(ShipmentResponse))]
    public async Task<IActionResult> CheckBarcodeBloom(string barcode,
        CancellationToken cancellationToken)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var barcodeExist = await _bloomFilterService.Contains(barcode, cancellationToken);

        stopwatch.Stop();
        if (barcodeExist)
        {
            return Ok(new ShipmentResponse {Time = stopwatch.ElapsedMilliseconds, Value = barcodeExist});
        }
        return NotFound(new ShipmentResponse { Time = stopwatch.ElapsedMilliseconds, Value = false });
    }

    /// <summary>
    /// Add shipment to db using bloom filter
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("add/bloom")]
    [MapToApiVersion("1.0")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Problem on the server", Type = typeof(DomainErrorResponse))]
    [SwaggerResponse(StatusCodes.Status201Created, "Success adding shipment with bloom filter", Type = typeof(ShipmentResponse))]
    public async Task<IActionResult> AddShipmentBloom(CancellationToken cancellationToken)
    {
        Stopwatch stopwatch = new();
        stopwatch.Start();
        var shipment = new Shipment();
        await _bloomFilterService.Add(shipment, cancellationToken);

        stopwatch.Stop();

        return Created(Url.ActionLink(nameof(AddShipmentBloom)), new ShipmentResponse { Time = stopwatch.ElapsedMilliseconds, Value = shipment });
    }

    /// <summary>
    /// Create test shipments
    /// </summary>
    /// <param name="count">Count to create test shipment
    /// !recommended number 100000!</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost("add-many/bloom")]
    [MapToApiVersion("1.0")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Problem on the server", Type = typeof(DomainErrorResponse))]
    [SwaggerResponse(StatusCodes.Status200OK, "Success adding many shipments with bloom filter", Type = typeof(ShipmentResponse))]
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