using Bitbucket.Models;
using Bitbucket.Responces;
using Bitbucket.Services;
using Microsoft.AspNetCore.Mvc;
using Prometheus;
using Swashbuckle.AspNetCore.Annotations;
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
    [HttpGet()]
    [MapToApiVersion("1.0")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Problem on the server", Type = typeof(DomainError))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Shipment with barcode not found in DB using bloom filter", Type = typeof(ShipmentResponse))]
    [SwaggerResponse(StatusCodes.Status200OK, "Shipment with barcode found in DB using bloom filter", Type = typeof(ShipmentResponse))]
    public async Task<IActionResult> CheckBarcodeBloom([FromQuery]string barcode,
        CancellationToken cancellationToken)
    {
        using (_prometheusService.CreateDurationOperation().NewTimer())
        {
            var barcodeExist = await _bloomFilterService.Contains(barcode, cancellationToken);

            if (barcodeExist)
            {
                return Ok(new ShipmentResponse {Value = barcodeExist});
            }
            return NotFound(new DomainError { Message = $"Shipments with barcode: {barcode} - not found in DB"});
        }
    }

    /// <summary>
    /// Add shipment to db using bloom filter
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Problem on the server", Type = typeof(DomainError))]
    [SwaggerResponse(StatusCodes.Status201Created, "Success adding shipment with bloom filter", Type = typeof(ShipmentResponse))]
    public async Task<IActionResult> AddShipment([FromQuery] int quantity, CancellationToken cancellationToken)
    {
        var massive = new List<Shipment>();
        using (_prometheusService.CreateDurationOperation().NewTimer())
        {
            for (int i = 0; i < quantity; i++)
            {
                _logger.LogInformation("Added {i}", i);
                var shipment = await _bloomFilterService.Create(cancellationToken);
                massive.Add(shipment);
            }
            return Created(Url.ActionLink(nameof(AddShipment)), new ShipmentResponse { Value = massive });
        }
    }
}