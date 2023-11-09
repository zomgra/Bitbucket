using Bitbucket.Data;
using Bitbucket.Models;
using Bitbucket.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text.Json.Serialization;
namespace Bitbucket.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class ShipmentsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly BarCodeGenerator _generator;

    public ShipmentsController(AppDbContext context, 
        BarCodeGenerator generator)
    {
        _context = context;
        _generator = generator;
    }

    [HttpGet]
    public async Task<IActionResult> CheckBarcode(string barcode)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        if (string.IsNullOrEmpty(barcode) || barcode.Length < 13 || barcode.Length > 25)
        {
            return BadRequest($"Invalid barcode length or format");
        }
        var existingShipment = await _context.Shipments.FirstOrDefaultAsync(s => s.Barcode == barcode);

        stopwatch.Stop();
        if(existingShipment != null)
        {
            return Ok($"{barcode} as barcode already used. Request Speed: {stopwatch.ElapsedMilliseconds} ms");
        }
        return NotFound($"Barcode {barcode} is not in use. Request Speed: {stopwatch.ElapsedMilliseconds} ms");
    }

    [HttpPost]
    public async Task<IActionResult> AddShipment()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        var barcode = _generator.GenerateBarCode(2, 2);
        var chipment = new Shipment(barcode);

        await _context.AddAsync(chipment);
        await _context.SaveChangesAsync();
        stopwatch.Stop();

        return Ok(JsonConvert.SerializeObject(chipment) + $"Request Speed: {stopwatch.ElapsedMilliseconds} ms");
    }

}