using ReviewService.DTOs;
using ReviewService.Entities;
using ReviewService.Repositories;

namespace ReviewService.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _repo;
    private readonly IHttpClientFactory _httpClientFactory;

    public ReviewService(IReviewRepository repo, IHttpClientFactory httpClientFactory)
    {
        _repo = repo;
        _httpClientFactory = httpClientFactory;
    }

    // ── Add Review ────────────────────────────────────────────────────────────

    public async Task<ReviewResponseDto> AddReview(AddReviewDto dto)
    {
        // Check if provider is verified before adding review
        bool isVerified = await CheckProviderVerification(dto.ProviderId);

        if (!isVerified)
        {
            throw new InvalidOperationException("Provider is not verified by admin. Cannot add reviews for unverified providers.");
        }

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
        await TriggerProviderRatingUpdate(dto.ProviderId);
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
        await TriggerProviderRatingUpdate(review.ProviderId);
        return MapToResponse(updated);
    }

    // ── Delete Review (Admin Moderation) ──────────────────────────────────────

    public async Task<bool> DeleteReview(int reviewId)
    {
        var review = await _repo.FindById(reviewId);
        if (review == null) return false;

        var result = await _repo.DeleteByReviewId(reviewId);
        if (result)
        {
            await TriggerProviderRatingUpdate(review.ProviderId);
        }
        return result;
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
        var enriched = new List<ReviewResponseDto>();
        foreach (var r in reviews)
        {
            var dto = MapToResponse(r);
            await EnrichReviewData(dto);
            // Ensure we never return null for ProviderName to avoid frontend ID fallback
            if (string.IsNullOrEmpty(dto.ProviderName))
                dto.ProviderName = $"Provider #{dto.ProviderId}";
            enriched.Add(dto);
        }
        return enriched;
    }
 
    // ── Private Helpers ───────────────────────────────────────────────────────
 
    private async Task EnrichReviewData(ReviewResponseDto dto)
    {
        try
        {
            if (!dto.IsAnonymous && dto.PatientId.HasValue)
            {
                var authClient = _httpClientFactory.CreateClient("AuthService");
                var patientResponse = await authClient.GetAsync($"/api/v1/auth/users/{dto.PatientId}");
                if (patientResponse.IsSuccessStatusCode)
                {
                    var patientJson = await patientResponse.Content.ReadAsStringAsync();
                    var patient = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(patientJson);
                    dto.PatientName = patient.GetProperty("fullName").GetString();
                }
            }
            else if (dto.IsAnonymous)
            {
                dto.PatientName = "Anonymous Patient";
            }
 
            var providerClient = _httpClientFactory.CreateClient("ProviderService");
            var providerResponse = await providerClient.GetAsync($"/api/v1/providers/{dto.ProviderId}");
            if (providerResponse.IsSuccessStatusCode)
            {
                var providerJson = await providerResponse.Content.ReadAsStringAsync();
                var provider = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(providerJson);
                
                dto.ProviderName = provider.GetProperty("fullName").GetString();
            }
            else
            {
                dto.ProviderName = "Unknown Provider";
            }
        }
        catch (Exception ex)
        { 
            Console.WriteLine($"[ReviewService] Error enriching review {dto.ReviewId}: {ex.Message}");
            if (string.IsNullOrEmpty(dto.ProviderName))
                dto.ProviderName = "Error Fetching Name";
        }
    }

    // Calls Provider-Service to check if provider is verified
    private async Task<bool> CheckProviderVerification(int providerId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ProviderService");
            var response = await client.GetAsync($"/api/v1/providers/{providerId}/verification-status");

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = System.Text.Json.JsonSerializer.Deserialize<VerificationResponse>(content);
            return result?.isVerified ?? false;
        }
        catch
        {
            // If Provider-Service is not reachable, allow review (fail-open)
            return true;
        }
    }

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
    private async Task TriggerProviderRatingUpdate(int providerId)
    {
        try
        {
            var ratingData = await GetAvgRating(providerId);
            var client = _httpClientFactory.CreateClient("ProviderService");
            
            // Assuming Provider-Service has an endpoint to update rating
            // The endpoint is Put /api/v1/providers/{id}/rating (based on ProviderService.cs)
            await client.PutAsync($"/api/v1/providers/{providerId}/rating?rating={ratingData.AverageRating}", null);
        }
        catch
        {
            // Fail silently, background update
        }
    }
}
