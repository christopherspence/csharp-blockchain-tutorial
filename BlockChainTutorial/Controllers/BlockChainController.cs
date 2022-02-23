using BlockChainTutorial.Models;
using BlockChainTutorial.Services;
using Microsoft.AspNetCore.Mvc;

namespace BlockChainTutorial.Controllers;

[ApiController]
[Route("[controller]")]
public class BlockChainController : ControllerBase
{
    private readonly ILogger<BlockChainController> logger;
    private readonly IBlockChainService service;

    public BlockChainController(ILogger<BlockChainController> logger, IBlockChainService service)
    {
        this.logger = logger;
        this.service = service;
    }

    [HttpGet("/chain")]
    public IActionResult GetChain()
    {
        return Ok(service.GetChain());
    }

    [HttpGet("/mine")]
    public async Task<IActionResult> MineAsync()
    {
        return Ok(await service.MineAsync());
    }

    [HttpPost("/transactions/new")]
    public IActionResult CreateTransaction(Transaction transaction)
    {
        var index = service.CreateNewTransaction(transaction);
        return Created(string.Empty, $"Transaction will be added to block {index}");
    }

    [HttpPost("/nodes/register")]
    public IActionResult RegisterNodes(List<string> nodes)
    {
        if (nodes == null || nodes.Count == 0)
        {
            return BadRequest();
        }

        service.RegisterNodes(nodes);

        return Created(string.Empty, null);
    }

    [HttpGet("/nodes/resolve")]
    public async Task<IActionResult> ResolveNodesAsync()
    {
        await service.ResolveConflictsAsync();

        return Ok(service.GetChain());
    }
}
