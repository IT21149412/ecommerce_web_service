/*
 * ------------------------------------------------------------------------------
 * File:       VendorReviewController.cs
 * Description: 
 *             This file defines the VendorReviewController class, which provides 
 *             API endpoints for managing vendor reviews. It allows customers to 
 *             add, update, and fetch reviews for vendors. The controller also 
 *             provides functionality to calculate and retrieve vendor ratings, 
 *             and restricts access based on user roles.
 * ------------------------------------------------------------------------------
 */

using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;

[ApiController]
[Route("api/vendor-review")]
public class VendorReviewController : ControllerBase
{
    private readonly VendorReviewService _reviewService;
    private readonly UserService _userService;

    public VendorReviewController(VendorReviewService reviewService, UserService userService)
    {
        _reviewService = reviewService;
        _userService = userService;
    }

    // Add a new review.
    [HttpPost("{vendorId}/add")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult> AddVendorReview(string vendorId, [FromBody] VendorReview review)
    {
        var vendor = await _userService.GetUserByIdAsync(vendorId);
        if (vendor == null || vendor.Role != "Vendor")
        {
            return BadRequest("Vendor not found or user is not a vendor.");
        }

        var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
        if (string.IsNullOrEmpty(customerId))
        {
            return Unauthorized("Customer ID not found.");
        }

        review.VendorId = vendorId;
        review.CustomerId = customerId;

        await _reviewService.AddReviewAsync(review);
        return Ok("Review added successfully");
    }

    // Get vendor's average rating.
    [HttpGet("{vendorId}/average-rating")]
    [AllowAnonymous]
    public async Task<ActionResult> GetVendorAverageRating(string vendorId)
    {
        var vendor = await _userService.GetUserByIdAsync(vendorId);
        if (vendor == null || vendor.Role != "Vendor")
        {
            return NotFound("Vendor not found.");
        }

        var averageRating = await _reviewService.GetAverageRatingAsync(vendorId);
        return Ok(new { AverageRating = averageRating });
    }

    // Update a review comment.
    [HttpPut("{reviewId}/update-comment")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult> UpdateVendorComment(string reviewId, [FromBody] UpdateCommentModel updateCommentModel)
    {
        var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
        var review = await _reviewService.GetReviewByIdAsync(reviewId);

        if (review == null || review.CustomerId != customerId)
        {
            return Unauthorized("You can only update your own comments.");
        }

        await _reviewService.UpdateReviewAsync(reviewId, updateCommentModel.NewComment);
        return Ok("Comment updated successfully.");
    }

    // Get all vendor reviews.
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult> GetAllVendorReviews()
    {
        var reviews = await _reviewService.GetAllReviewsAsync();
        var vendorReviewsWithUsers = new List<object>();

        foreach (var review in reviews)
        {
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

    // Get reviews by vendor ID.
    [HttpGet("{vendorId}")]
    [Authorize(Roles = "Administrator, CSR, Vendor, Customer")]
    public async Task<ActionResult> GetReviewsByVendorId(string vendorId)
    {
        var vendor = await _userService.GetUserByIdAsync(vendorId);
        if (vendor == null || vendor.Role != "Vendor")
        {
            return NotFound("Vendor not found or user is not a vendor.");
        }

        var reviews = await _reviewService.GetReviewsByVendorIdAsync(vendorId);
        if (reviews == null || reviews.Count == 0)
        {
            return NotFound("No reviews found for this vendor.");
        }

        return Ok(reviews);
    }
}
