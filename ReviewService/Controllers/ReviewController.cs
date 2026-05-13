using Microsoft.AspNetCore.Mvc;
using ReviewService.DTOs;
using ReviewService.Services;

namespace ReviewService.Controllers;

[ApiController]
[Route("api/v1/reviews")]
[Produces("application/json")]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _service;

    public ReviewController(IReviewService service)
    {
        _service = service;
    }

    /// <summary>Add a new review for a completed appointment</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddReview([FromBody] AddReviewDto dto)
    {
        try
        {
            var result = await _service.AddReview(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Get all reviews</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var reviews = await _service.GetAllReviews();
        return Ok(reviews);
    }

    /// <summary>Get all reviews for a provider</summary>
    [HttpGet("provider/{providerId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByProvider(int providerId)
    {
        var reviews = await _service.GetByProvider(providerId);
        return Ok(reviews);
    }

    /// <summary>Get average rating for a provider</summary>
    [HttpGet("provider/{providerId}/avgrating")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvgRating(int providerId)
    {
        var result = await _service.GetAvgRating(providerId);
        return Ok(result);
    }

    /// <summary>Get total review count for a provider</summary>
    [HttpGet("provider/{providerId}/count")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCount(int providerId)
    {
        var count = await _service.GetReviewCount(providerId);
        return Ok(new { providerId, count });
    }

    /// <summary>Get all reviews by a patient</summary>
    [HttpGet("patient/{patientId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByPatient(int patientId)
    {
        var reviews = await _service.GetByPatient(patientId);
        return Ok(reviews);
    }

    /// <summary>Get review for a specific appointment</summary>
    [HttpGet("appointment/{appointmentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByAppointment(int appointmentId)
    {
        var review = await _service.GetByAppointment(appointmentId);

        if (review == null)
        {
            return NotFound(new { message = "No review found for this appointment." });
        }

        return Ok(review);
    }

    /// <summary>Update a review</summary>
    [HttpPut("{reviewId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateReview(
        int reviewId,
        [FromBody] UpdateReviewDto dto)
    {
        try
        {
            var result = await _service.UpdateReview(reviewId, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Delete a review — Admin moderation</summary>
    [HttpDelete("{reviewId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReview(int reviewId)
    {
        var result = await _service.DeleteReview(reviewId);

        if (!result)
        {
            return NotFound(new { message = "Review not found." });
        }

        return Ok(new { message = "Review deleted successfully." });
    }
}
