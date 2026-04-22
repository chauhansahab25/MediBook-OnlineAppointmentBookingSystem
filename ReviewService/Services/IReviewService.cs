using ReviewService.DTOs;

namespace ReviewService.Services;

public interface IReviewService
{
    Task<ReviewResponseDto> AddReview(AddReviewDto dto);

    Task<List<ReviewResponseDto>> GetByProvider(int providerId);

    Task<List<ReviewResponseDto>> GetByPatient(int patientId);

    Task<ReviewResponseDto?> GetByAppointment(int appointmentId);

    Task<ReviewResponseDto> UpdateReview(int reviewId, UpdateReviewDto dto);

    Task<bool> DeleteReview(int reviewId);

    Task<AvgRatingResponseDto> GetAvgRating(int providerId);

    Task<int> GetReviewCount(int providerId);

    Task<List<ReviewResponseDto>> GetAllReviews();
} 
