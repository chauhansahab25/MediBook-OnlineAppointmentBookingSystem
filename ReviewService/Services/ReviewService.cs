using ReviewService.DTOs;
using ReviewService.Entities;
using ReviewService.Repositories;

namespace ReviewService.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _repo;

    public ReviewService(IReviewRepository repo)
    {
        _repo = repo;
    }

    // ── Add Review ────────────────────────────────────────────────────────────

    public async Task<ReviewResponseDto> AddReview(AddReviewDto dto)
    {
        // Enforce one review per appointment
        bool exists = await _repo.ExistsByAppointmentId(dto.AppointmentId);

        if (exists)
        {
            throw new InvalidOperationException(
                "A review already exists for this appointment.");
        }

        // Validate rating range
        if (dto.Rating < 1 || dto.Rating > 5)
        {
            throw new ArgumentException("Rating must be between 1 and 5.");
        }

        var review = new Review
        {
            AppointmentId = dto.AppointmentId,
            PatientId = dto.PatientId,
            ProviderId = dto.ProviderId,
            Rating = dto.Rating,
            Comment = dto.Comment,
            IsAnonymous = dto.IsAnonymous,
            IsVerified = false,
            ReviewDate = DateTime.UtcNow
        };

        var saved = await _repo.Add(review);
        return MapToResponse(saved);
    }

    // ── Get By Provider ───────────────────────────────────────────────────────

    public async Task<List<ReviewResponseDto>> GetByProvider(int providerId)
    {
        var reviews = await _repo.FindByProviderId(providerId);
        return reviews.Select(MapToResponse).ToList();
    }

    // ── Get By Patient ────────────────────────────────────────────────────────

    public async Task<List<ReviewResponseDto>> GetByPatient(int patientId)
    {
        var reviews = await _repo.FindByPatientId(patientId);
        return reviews.Select(MapToResponse).ToList();
    }

    // ── Get By Appointment ────────────────────────────────────────────────────

    public async Task<ReviewResponseDto?> GetByAppointment(int appointmentId)
    {
        var review = await _repo.FindByAppointmentId(appointmentId);

        if (review == null)
        {
            return null;
        }

        return MapToResponse(review);
    }

    // ── Update Review ─────────────────────────────────────────────────────────

    public async Task<ReviewResponseDto> UpdateReview(int reviewId, UpdateReviewDto dto)
    {
        var review = await _repo.FindById(reviewId);

        if (review == null)
        {
            throw new KeyNotFoundException("Review not found.");
        }

        // Validate rating range
        if (dto.Rating < 1 || dto.Rating > 5)
        {
            throw new ArgumentException("Rating must be between 1 and 5.");
        }

        review.Rating = dto.Rating;
        review.Comment = dto.Comment;
        review.IsAnonymous = dto.IsAnonymous;

        var updated = await _repo.Update(review);
        return MapToResponse(updated);
    }

    // ── Delete Review (Admin Moderation) ──────────────────────────────────────

    public async Task<bool> DeleteReview(int reviewId)
    {
        return await _repo.DeleteByReviewId(reviewId);
    }

    // ── Get Average Rating ────────────────────────────────────────────────────

    public async Task<AvgRatingResponseDto> GetAvgRating(int providerId)
    {
        double avg = await _repo.AvgRatingByProviderId(providerId);
        int count = await _repo.CountByProviderId(providerId);

        return new AvgRatingResponseDto
        {
            ProviderId = providerId,
            AverageRating = avg,
            TotalReviews = count
        };
    }

    // ── Get Review Count ──────────────────────────────────────────────────────

    public async Task<int> GetReviewCount(int providerId)
    {
        return await _repo.CountByProviderId(providerId);
    }

    // ── Get All Reviews ───────────────────────────────────────────────────────

    public async Task<List<ReviewResponseDto>> GetAllReviews()
    {
        var reviews = await _repo.GetAll();
        return reviews.Select(MapToResponse).ToList();
    }

    // ── Private Helpers ───────────────────────────────────────────────────────

    private ReviewResponseDto MapToResponse(Review r)
    {
        return new ReviewResponseDto
        {
            ReviewId = r.ReviewId,
            AppointmentId = r.AppointmentId,

            // Hide patient identity if anonymous
            PatientId = r.IsAnonymous ? null : r.PatientId,

            ProviderId = r.ProviderId,
            Rating = r.Rating,
            Comment = r.Comment,
            ReviewDate = r.ReviewDate,
            IsVerified = r.IsVerified,
            IsAnonymous = r.IsAnonymous
        };
    }
}
