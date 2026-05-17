using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using ScheduleService.Controllers;
using ScheduleService.DTOs;
using ScheduleService.Entities;
using ScheduleService.Services;

namespace MediBook.Tests.ScheduleService;

[TestFixture]
public class ScheduleControllerTests
{
    private Mock<IScheduleService> _mockService;
    private ScheduleController _controller;

    [SetUp]
    public void Setup()
    {
        _mockService = new Mock<IScheduleService>();
        _controller = new ScheduleController(_mockService.Object);
    }

    [Test]
    public async Task GetById_ReturnsOk_WhenSlotExists()
    {
        // Arrange
        int slotId = 1;
        var slot = new AvailabilitySlot { SlotId = slotId, ProviderId = 1, Date = DateTime.UtcNow };
        _mockService.Setup(s => s.GetSlotById(slotId)).ReturnsAsync(slot);

        // Act
        var result = await _controller.GetById(slotId);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult, Is.Not.Null);
        Assert.That(okResult.Value, Is.EqualTo(slot));
    }

    [Test]
    public async Task GetById_ReturnsNotFound_WhenSlotDoesNotExist()
    {
        // Arrange
        int slotId = 99;
        _mockService.Setup(s => s.GetSlotById(slotId)).ReturnsAsync((AvailabilitySlot)null);

        // Act
        var result = await _controller.GetById(slotId);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task AddSlot_ReturnsOk_WithAddedSlot()
    {
        // Arrange
        var dto = new AddSlotDto { ProviderId = 1, Date = DateTime.UtcNow, DurationMinutes = 30 };
        var addedSlot = new AvailabilitySlot { SlotId = 1, ProviderId = 1 };
        _mockService.Setup(s => s.AddSlot(dto)).ReturnsAsync(addedSlot);

        // Act
        var result = await _controller.AddSlot(dto);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        var okResult = result as OkObjectResult;
        Assert.That(okResult.Value, Is.EqualTo(addedSlot));
    }

    [Test]
    public async Task Book_ReturnsOk_WhenBookingIsSuccessful()
    {
        // Arrange
        int slotId = 1;
        _mockService.Setup(s => s.BookSlot(slotId)).ReturnsAsync(true);

        // Act
        var result = await _controller.Book(slotId);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task Book_ReturnsNotFound_WhenSlotDoesNotExist()
    {
        // Arrange
        int slotId = 99;
        _mockService.Setup(s => s.BookSlot(slotId)).ReturnsAsync(false);

        // Act
        var result = await _controller.Book(slotId);

        // Assert
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task Book_ReturnsBadRequest_WhenExceptionIsThrown()
    {
        // Arrange
        int slotId = 1;
        _mockService.Setup(s => s.BookSlot(slotId)).ThrowsAsync(new InvalidOperationException("Slot is already booked."));

        // Act
        var result = await _controller.Book(slotId);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        var badRequestResult = result as BadRequestObjectResult;
        Assert.That(badRequestResult.Value.ToString(), Contains.Substring("Slot is already booked."));
    }
}
