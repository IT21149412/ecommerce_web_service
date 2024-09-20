using MongoDB.Driver;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

public class VendorReviewService
{
    private readonly IMongoCollection<VendorReview> _reviews;

    public VendorReviewService(MongoDbContext context)
    {
        _reviews = context.VendorReviews;
    }

    // Add a review for a vendor
    public async Task AddReviewAsync(VendorReview review)
    {
        await _reviews.InsertOneAsync(review);
    }

    // Calculate the average rating for a vendor
    public async Task<double> GetAverageRatingAsync(string vendorId)
    {
        var reviews = await _reviews.Find(r => r.VendorId == vendorId).ToListAsync();
        if (reviews.Count == 0)
        {
            return 0; // No reviews yet
        }

        double averageRating = reviews.Average(r => r.Rating);
        return averageRating;
    }

    // Get reviews for a vendor
    public async Task<List<VendorReview>> GetReviewsByVendorIdAsync(string vendorId)
    {
        return await _reviews.Find(r => r.VendorId == vendorId).ToListAsync();
    }

    // Get a specific review by ID
    public async Task<VendorReview?> GetReviewByIdAsync(string reviewId)
    {
        return await _reviews.Find(review => review.Id == reviewId).FirstOrDefaultAsync();
    }

    // Update the review comment
    public async Task UpdateReviewAsync(string reviewId, string newComment)
    {
        var update = Builders<VendorReview>.Update.Set(r => r.Comment, newComment);
        await _reviews.UpdateOneAsync(review => review.Id == reviewId, update);
    }
}
