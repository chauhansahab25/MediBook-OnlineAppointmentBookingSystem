namespace ReviewService.DTOs;

public class AddReviewDto
{
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public int ProviderId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public bool IsAnonymous { get; set; } = false;
}

public class UpdateReviewDto
{
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public bool IsAnonymous { get; set; } = false;
}

public class ReviewResponseDto
{
    public int ReviewId { get; set; }
    public int AppointmentId { get; set; }

    // Show PatientId or hide it if anonymous
    public int? PatientId { get; set; }
    public int ProviderId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime ReviewDate { get; set; }
    public bool IsVerified { get; set; }
    public bool IsAnonymous { get; set; }
}

public class AvgRatingResponseDto
{
    public int ProviderId { get; set; }
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
} 
