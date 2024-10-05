using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/vendor-review")]
public class VendorReviewController : ControllerBase
{
    private readonly VendorReviewService _reviewService;
    private readonly UserService _userService;  // To validate vendors from the User collection

    public VendorReviewController(VendorReviewService reviewService, UserService userService)
    {
        _reviewService = reviewService;
        _userService = userService;
    }

    // POST: api/vendor-review/{vendorId}/add
    [HttpPost("{vendorId}/add")]
    [Authorize(Roles = "Customer")] // Only customers can add reviews
    public async Task<ActionResult> AddVendorReview(string vendorId, [FromBody] VendorReview review)
    {
        // Validate that the vendor exists in the User collection and has the Vendor role
        var vendor = await _userService.GetUserByIdAsync(vendorId);
        if (vendor == null || vendor.Role != "Vendor")
        {
            return BadRequest("Vendor not found or user is not a vendor.");
        }

        var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the customer ID from the token
        if (string.IsNullOrEmpty(customerId))
        {
            return Unauthorized("Customer ID not found.");
        }

        // Set vendor and customer information
        review.VendorId = vendorId;
        review.CustomerId = customerId;

        await _reviewService.AddReviewAsync(review);
        return Ok("Review added successfully");
    }

    // GET: api/vendor-review/{vendorId}/average-rating
    [HttpGet("{vendorId}/average-rating")]
    [AllowAnonymous]
    public async Task<ActionResult> GetVendorAverageRating(string vendorId)
    {
        // Validate that the vendor exists
        var vendor = await _userService.GetUserByIdAsync(vendorId);
        if (vendor == null || vendor.Role != "Vendor")
        {
            return NotFound("Vendor not found.");
        }

        var averageRating = await _reviewService.GetAverageRatingAsync(vendorId);
        return Ok(new { AverageRating = averageRating });
    }

    // PUT: api/vendor-review/{reviewId}/update-comment
    [HttpPut("{reviewId}/update-comment")]
    [Authorize(Roles = "Customer")] // Only customers can modify their comments
    public async Task<ActionResult> UpdateVendorComment(string reviewId, [FromBody] UpdateCommentModel updateCommentModel)
    {
        var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get the customer ID from the token

        // Retrieve the review by the ID
        var review = await _reviewService.GetReviewByIdAsync(reviewId);
        
        // Ensure the review exists and belongs to the authenticated customer
        if (review == null || review.CustomerId != customerId)
        {
            return Unauthorized("You can only update your own comments.");
        }

        // Update the review's comment with the new comment provided in the request body
        await _reviewService.UpdateReviewAsync(reviewId, updateCommentModel.NewComment);
        
        return Ok("Comment updated successfully.");
    }

    // GET: api/vendor-review
    [HttpGet]
    [AllowAnonymous] // This allows anyone to fetch the vendor reviews, or restrict based on admin only
    public async Task<ActionResult> GetAllVendorReviews()
    {
        var reviews = await _reviewService.GetAllReviewsAsync();
        var vendorReviewsWithUsers = new List<object>();

        foreach (var review in reviews)
        {
            // Fetch vendor details
            var vendor = await _userService.GetUserByIdAsync(review.VendorId);
            if (vendor != null && vendor.Role == "Vendor")
            {
                vendorReviewsWithUsers.Add(new
                {
                    VendorName = vendor.Name,
                    VendorId = vendor.Id,
                    Review = review
                });
            }
        }

        return Ok(vendorReviewsWithUsers);
    }


}
