using ReviewService.Entities;

namespace ReviewService.Repositories;

public interface IReviewRepository
{
    Task<Review?> FindById(int reviewId);

    Task<List<Review>> FindByProviderId(int providerId);

    Task<List<Review>> FindByPatientId(int patientId);

    Task<Review?> FindByAppointmentId(int appointmentId);

    Task<double> AvgRatingByProviderId(int providerId);

    Task<List<Review>> FindByRating(int rating);

    Task<int> CountByProviderId(int providerId);

    Task<bool> ExistsByAppointmentId(int appointmentId);

    Task<bool> DeleteByReviewId(int reviewId);

    Task<List<Review>> GetAll();

    Task<Review> Add(Review review);

    Task<Review> Update(Review review);
} 
