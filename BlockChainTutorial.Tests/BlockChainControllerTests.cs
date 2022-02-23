using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlockChainTutorial.Controllers;
using BlockChainTutorial.Models;
using BlockChainTutorial.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BlockChainTutorial.Tests;

public class BlockChainControllerTests
{
    [Fact]
    public void CanGetInitialChain()
    {
        // Arrange
        var blockChain = new BlockChain();
        var service = new BlockChainService(new Mock<ILogger<BlockChainService>>().Object, new Mock<IHttpService>().Object, blockChain);

        var controller = new BlockChainController(new Mock<ILogger<BlockChainController>>().Object, service);

        // Act
        var response = controller.GetChain();

        // Assert
        var result = response.Should().BeOfType<OkObjectResult>().Subject.Value as List<Block>;

        result.Should().BeEquivalentTo(new List<Block>
        {
            new Block(0, DateTime.UtcNow, new List<Transaction>(), 0, "1")
        }, o => o.Excluding(n => n.Path.EndsWith("TimeStamp")));
    }

    [Fact]
    public void CanCreateNewTransaction()
    {
        // Arrange
        var blockChain = new BlockChain();
        var service = new BlockChainService(new Mock<ILogger<BlockChainService>>().Object, new Mock<IHttpService>().Object, blockChain);

        var controller = new BlockChainController(new Mock<ILogger<BlockChainController>>().Object, service);

        // Act
        var transaction = new Transaction(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), 5);
        var response = controller.CreateTransaction(transaction);

        // Assert
        response.Should().BeOfType<CreatedResult>();

        service.BlockChain.CurrentTransactions.Should().BeEquivalentTo(new List<Transaction> { transaction });
    }

    [Fact]
    public async Task CanMineBlocks()
    {
        // Arrange
        var blockChain = new BlockChain();
        var service = new BlockChainService(new Mock<ILogger<BlockChainService>>().Object, new Mock<IHttpService>().Object, blockChain);

        var controller = new BlockChainController(new Mock<ILogger<BlockChainController>>().Object, service);

        // Act
        var transaction = new Transaction(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), 5);
        service.CreateNewTransaction(transaction);

        var response = await controller.MineAsync();

        // Assert
        var result = response.Should().BeOfType<OkObjectResult>().Subject.Value as Block;

        var expectedSecondBlockTransactions = new List<Transaction> 
        { 
            transaction,
            new Transaction("0", service.NodeId.ToString(), 1)
        };

        result.Should().BeEquivalentTo(
            new Block(2, DateTime.UtcNow, expectedSecondBlockTransactions, 0, "1"),
            o => o.Excluding(n => n.Path.EndsWith("TimeStamp") || n.Path.EndsWith("PreviousHash")));

        service.BlockChain.CurrentTransactions.Should().BeEquivalentTo(new List<Transaction>());

        service.BlockChain.Chain.Should().BeEquivalentTo(new List<Block>
        {
            new Block(0, DateTime.UtcNow, new List<Transaction>(), 0, "1"),
            new Block(2, DateTime.UtcNow, expectedSecondBlockTransactions, 0, string.Empty)
        }, o => o.Excluding(n => n.Path.EndsWith("TimeStamp") || n.Path.EndsWith("PreviousHash")));
    }

    [Fact]
    public void CanRegisterNodes()
    {
        // Arrange
        var blockChain = new BlockChain();
        var httpServiceMock = new Mock<IHttpService>();
        var service = new BlockChainService(new Mock<ILogger<BlockChainService>>().Object, httpServiceMock.Object, blockChain);

        var controller = new BlockChainController(new Mock<ILogger<BlockChainController>>().Object, service);

        // Act
        var random = new Random();
        var nodes = new List<string> 
        { 
            $"https://localhost:{random.Next(1000, 2000)}",
            $"https://localhost:{random.Next(2001, 3000)}",
            $"https://localhost:{random.Next(3001, 4000)}",
            $"https://localhost:{random.Next(4001, 5000)}"
        };

        var response = controller.RegisterNodes(nodes);

        // Assert
        response.Should().BeOfType<CreatedResult>();

        service.BlockChain.Nodes.Should().BeEquivalentTo(new List<Uri>
        {
            new Uri(nodes.First()),
            new Uri(nodes.ElementAt(1)),
            new Uri(nodes.ElementAt(2)),
            new Uri(nodes.Last())
        });
    }

    [Fact]
    public async Task CanResolveConflics()
    {
        // Arrange
        var blockChain = new BlockChain();
        var httpServiceMock = new Mock<IHttpService>();
        var service = new BlockChainService(new Mock<ILogger<BlockChainService>>().Object, httpServiceMock.Object, blockChain);

        var controller = new BlockChainController(new Mock<ILogger<BlockChainController>>().Object, service);

        // Act
        var random = new Random();
        var nodes = new List<string> 
        { 
            $"https://localhost:{random.Next(1000, 2000)}",
            $"https://localhost:{random.Next(2001, 3000)}",
            $"https://localhost:{random.Next(3001, 4000)}",
            $"https://localhost:{random.Next(4001, 5000)}"
        };

        service.RegisterNodes(nodes);
        
        var response = await controller.ResolveNodesAsync();

        // Assert
        var result = response.Should().BeOfType<OkObjectResult>().Subject.Value as List<Block>;

        result.Should().BeEquivalentTo(new List<Block>
        {
            new Block(0, DateTime.UtcNow, new List<Transaction>(), 0, "1")
        }, o => o.Excluding(n => n.Path.EndsWith("TimeStamp")));

        httpServiceMock.Verify(m => m.GetBlocksAsync(It.IsAny<Uri>()), Times.Exactly(4));
    }
}