using Microsoft.EntityFrameworkCore;
using ReviewService.Data;
using ReviewService.Entities;

namespace ReviewService.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly AppDbContext _context;

    public ReviewRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Review?> FindById(int reviewId)
    {
        return await _context.Reviews.FindAsync(reviewId);
    }

    public async Task<List<Review>> FindByProviderId(int providerId)
    {
        return await _context.Reviews
            .Where(r => r.ProviderId == providerId)
            .OrderByDescending(r => r.ReviewDate)
            .ToListAsync();
    }

    public async Task<List<Review>> FindByPatientId(int patientId)
    {
        return await _context.Reviews
            .Where(r => r.PatientId == patientId)
            .OrderByDescending(r => r.ReviewDate)
            .ToListAsync();
    }

    public async Task<Review?> FindByAppointmentId(int appointmentId)
    {
        return await _context.Reviews
            .FirstOrDefaultAsync(r => r.AppointmentId == appointmentId);
    }

    public async Task<double> AvgRatingByProviderId(int providerId)
    {
        var ratings = await _context.Reviews
            .Where(r => r.ProviderId == providerId)
            .Select(r => r.Rating)
            .ToListAsync();

        if (ratings.Count == 0)
        {
            return 0;
        }

        return Math.Round(ratings.Average(), 2);
    }

    public async Task<List<Review>> FindByRating(int rating)
    {
        return await _context.Reviews
            .Where(r => r.Rating == rating)
            .OrderByDescending(r => r.ReviewDate)
            .ToListAsync();
    }

    public async Task<int> CountByProviderId(int providerId)
    {
        return await _context.Reviews
            .CountAsync(r => r.ProviderId == providerId);
    }

    public async Task<bool> ExistsByAppointmentId(int appointmentId)
    {
        return await _context.Reviews
            .AnyAsync(r => r.AppointmentId == appointmentId);
    }

    public async Task<bool> DeleteByReviewId(int reviewId)
    {
        var review = await _context.Reviews.FindAsync(reviewId);

        if (review == null)
        {
            return false;
        }

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Review>> GetAll()
    {
        return await _context.Reviews
            .OrderByDescending(r => r.ReviewDate)
            .ToListAsync();
    }

    public async Task<Review> Add(Review review)
    {
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();
        return review;
    }

    public async Task<Review> Update(Review review)
    {
        _context.Reviews.Update(review);
        await _context.SaveChangesAsync();
        return review;
    }
}
