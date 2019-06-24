using BookApi.Dtos;
using BookApi.Models;
using BookApi.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookApi.Controllers
{
  [ApiController]
  [Route("/api/[controller]")]
  public class ReviewersController : Controller
  {
    private IReviewerRepository _reviewerRepository;
    private IReviewRepository _reviewRepository;

    public ReviewersController(IReviewerRepository reviewerRepository, IReviewRepository reviewRepository)
    {
      _reviewerRepository = reviewerRepository;
      _reviewRepository = reviewRepository;
    }


    [HttpGet]
    [ProducesResponseType(200, Type = typeof(IEnumerable<ReviewerDto>))]
    [ProducesResponseType(400)]
    public IActionResult GetReviewers()
    {
      var reviwers = _reviewerRepository.GetReviewers().ToList();

      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      var reviewersDto = new List<ReviewerDto>();

      foreach(var reviewer in reviwers)
      {
        reviewersDto.Add(new ReviewerDto
        {
          Id = reviewer.Id,
          FirstName = reviewer.FirstName,
          LastName = reviewer.LastName
        });
      }

      return Ok(reviewersDto);
    }


    //get api/reviewers/:id
    [HttpGet("{reviewerId}", Name = "GetReviewer")]
    [ProducesResponseType(200, Type = typeof(ReviewerDto))]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public IActionResult GerReviewer(int reviewerId)
    {
      if (!_reviewerRepository.ReviwererExists(reviewerId))
        return NotFound();

      var reviewer = _reviewerRepository.GetReviewer(reviewerId);

      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      var reviewerDto = new ReviewerDto()
      {
        Id = reviewer.Id,
        FirstName = reviewer.FirstName,
        LastName = reviewer.LastName
      };

      return Ok(reviewerDto);
    }

    //api/reviewers/reviewerId/reviews
    [HttpGet("{reviewerId}/reviews")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<ReviewDto>))]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public IActionResult GetReviewsByReviewer(int reviewerId)
    {
      if (!_reviewerRepository.ReviwererExists(reviewerId))
        return NotFound();

      var reviews = _reviewerRepository.GetReviewsByReviewer(reviewerId);

      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      var reviewsDto = new List<ReviewDto>();
      foreach(var reviewer in reviews)
      {
        reviewsDto.Add(new ReviewDto()
        {
          Id = reviewer.Id,
          Headline = reviewer.Headline,
          Rating = reviewer.Rating,
          ReviewText = reviewer.ReviewText
        });
      }
      return Ok(reviewsDto);
    }

    //to do - need to test it after we implement IReview reo
    [HttpGet("{reviewId}/reviewer")]//api/reviewers/revieId/reviewer
    [ProducesResponseType(200, Type = typeof(ReviewerDto))]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public IActionResult GetReviewerOfReview(int reviewId)
    {
      if (!_reviewRepository.ReviewExists(reviewId))
        return NotFound();

      var reviewer = _reviewerRepository.GetReviewerOfReviewer(reviewId);

      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      var reviewerDto = new ReviewerDto()
      {
        Id = reviewer.Id,
        FirstName = reviewer.FirstName,
        LastName = reviewer.LastName
      };

      return Ok(reviewerDto);
    }


    //POST
    [HttpPost]
    public IActionResult CreateReviewer([FromBody]Reviewer reviewerToCreate)
    {
      if (reviewerToCreate == null)
        return BadRequest(ModelState);
      if (!ModelState.IsValid)
        return BadRequest(ModelState);
      if (!_reviewerRepository.CreateReviewer(reviewerToCreate))
      {
        ModelState.AddModelError("", $"Something went wrong saving {reviewerToCreate.FirstName} {reviewerToCreate.LastName}");
        return StatusCode(500, ModelState);
      }

      return CreatedAtRoute("GetReviewer", new { reviewerId = reviewerToCreate.Id }, reviewerToCreate);
    }


    //PUT
    [HttpPut("{reviewerId}")]
    public IActionResult UpdateReviewer(int reviewerId, [FromBody]Reviewer updatedReviewerInfo)
    {
      if (updatedReviewerInfo == null)
        return BadRequest(ModelState);

      if (reviewerId != updatedReviewerInfo.Id)
        return BadRequest(ModelState);

      if (!_reviewerRepository.ReviwererExists(reviewerId))
        return NotFound();

      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      if (!_reviewerRepository.UpdateReviewer(updatedReviewerInfo))
      {
        ModelState.AddModelError("", $"Something went wrong updating " +
                                    $"{updatedReviewerInfo.FirstName} {updatedReviewerInfo.LastName}");
        return StatusCode(500, ModelState);
      }

      return NoContent();
    }

    //DELETE
    [HttpDelete("{reviewerId}")]
    public IActionResult DeleteReviewers(int reviewerId)
    {
      if (!_reviewerRepository.ReviwererExists(reviewerId))
        return NotFound();

      var reviewerToDelete = _reviewerRepository.GetReviewer(reviewerId);
      var reviewsToDelete = _reviewerRepository.GetReviewsByReviewer(reviewerId);

      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      if (!_reviewerRepository.DeleteReviewer(reviewerToDelete))
      {
        ModelState.AddModelError("", $"Something went wrong deleting " +
                                    $"{reviewerToDelete.FirstName} {reviewerToDelete.LastName}");
        return StatusCode(500, ModelState);
      }

      if (!_reviewRepository.DeleteReviews(reviewsToDelete.ToList()))
      {
        ModelState.AddModelError("", $"Something went wrong deleting reviews by" +
                                    $"{reviewerToDelete.FirstName} {reviewerToDelete.LastName}");
        return StatusCode(500, ModelState);
      }

      return NoContent();
    }
  }
}
