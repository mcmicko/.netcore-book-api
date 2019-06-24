using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookApi.Services;
using BookApi.Dtos;
using BookApi.Models;

namespace BookApi.Controllers
{
  [ApiController]
  [Route("api/[controller]")]
  public class ReviewsController : Controller
  {
    private IReviewRepository _reviewRepository;
    private IReviewerRepository _reviewerRepository;
    private IBookRepository _bookRepository;

    public ReviewsController(IReviewRepository reviewRepository, IReviewerRepository reviewerRepository, IBookRepository bookRepository)
    {
      _reviewRepository = reviewRepository;
      _reviewerRepository = reviewerRepository;
      _bookRepository = bookRepository;
    }


    [HttpGet]//api/reviews
    [ProducesResponseType(200, Type = typeof(IEnumerable<ReviewDto>))]
    [ProducesResponseType(400)]
    public IActionResult GetReviews()
    {
      var reviews = _reviewRepository.GetReviews();

      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      var reviewsDto = new List<ReviewDto>();

      foreach(var review in reviews)
      {
        reviewsDto.Add(new ReviewDto
        {
          Id = review.Id,
          Headline = review.Headline,
          Rating = review.Rating,
          ReviewText = review.ReviewText
        });
      }

      return Ok(reviewsDto);
    }

    //api/reviews/reviewId
    [HttpGet("{reviewId}", Name = "GetReview")]
    [ProducesResponseType(200, Type = typeof(ReviewDto))]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public IActionResult GetReview(int reviewId)
    {
      if (!_reviewRepository.ReviewExists(reviewId))
        return NotFound();

      var review = _reviewRepository.GetReview(reviewId);

      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      var reviewDto = new ReviewDto()
      {
        Id = review.Id,
        Headline = review.Headline,
        Rating = review.Rating,
        ReviewText = review.ReviewText
      };

      return Ok(reviewDto);
    }


    //api/reviews/books/bookId
    [HttpGet("books/{bookId}")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<ReviewDto>))]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public IActionResult GetReviewsForABook(int bookId)
    {
      //to do - validate book
      if (!_bookRepository.BookExists(bookId))
        return NotFound();

      var reviews = _reviewRepository.GetReviewsOfABook(bookId);

      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      var reviewsDto = new List<ReviewDto>();
      foreach (var reviewer in reviews)
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
    [HttpGet("{reviewId}/book")]//api/reviewers/revieId/book
    [ProducesResponseType(200, Type = typeof(BookDto))]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public IActionResult GetBookOfAReview(int reviewId)
    {
      if (!_reviewRepository.ReviewExists(reviewId))
        return NotFound();

      var book = _reviewRepository.GetBookOfAReview(reviewId);

      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      var bookDto = new BookDto()
      {
        Id = book.Id,
        Title = book.Title,
        Isbn = book.Isbn,
        DatePublished = book.DatePublished
      };

      return Ok(bookDto);
    }

    //POST
    [HttpPost]
    public IActionResult CreateReview([FromBody]Review reviewToCreate)
    {
      if (reviewToCreate == null)
        return NotFound();
      if (!_reviewerRepository.ReviwererExists(reviewToCreate.Reviewer.Id))
        ModelState.AddModelError("", "Reviewer doesn't exists");
      if (!_bookRepository.BookExists(reviewToCreate.Book.Id))
        ModelState.AddModelError("", "Book doesnt exists");
      if (!ModelState.IsValid)
        return StatusCode(404, ModelState);
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      reviewToCreate.Book = _bookRepository.GetBook(reviewToCreate.Book.Id);
      reviewToCreate.Reviewer = _reviewerRepository.GetReviewer(reviewToCreate.Reviewer.Id);

      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      if(!_reviewRepository.CreateReview(reviewToCreate))
      {
        ModelState.AddModelError("", $"Something went wrong saving the review");
        return StatusCode(500, ModelState);
      }

      return CreatedAtRoute("GetReview", new { reviewId = reviewToCreate.Id }, reviewToCreate);
    }

    //PUT
    [HttpPut("{reviewId}")]
    public IActionResult UpdateReview(int reviewId, [FromBody]Review reviewToUpdate)
    {
      if (reviewToUpdate == null)
        return BadRequest();

      if (reviewId != reviewToUpdate.Id)
        return BadRequest(ModelState);

      if (!_reviewRepository.ReviewExists(reviewId))
        ModelState.AddModelError("", "Review doesn't exist");

      if (!_reviewerRepository.ReviwererExists(reviewToUpdate.Reviewer.Id))
        ModelState.AddModelError("", "Reviewer doesn't exist");

      if (!_bookRepository.BookExists(reviewToUpdate.Book.Id))
        ModelState.AddModelError("", "Book doesn't exist");

      if (!ModelState.IsValid)
        return StatusCode(404, ModelState);

      reviewToUpdate.Book = _bookRepository.GetBook(reviewToUpdate.Book.Id);
      reviewToUpdate.Reviewer = _reviewerRepository.GetReviewer(reviewToUpdate.Reviewer.Id);

      if (!ModelState.IsValid)
        return BadRequest();

      if(!_reviewRepository.UpdatedReview(reviewToUpdate))
      {
        ModelState.AddModelError("", $"Something went wrong updating the review");
        return StatusCode(500, ModelState);
      }

      return NoContent();
    }


    //DELETE
    [HttpDelete("{reviewId}")]
    public IActionResult DeleteReview(int reviewId)
    {
      if (!_reviewRepository.ReviewExists(reviewId))
        return NotFound();

      var reviewToDelete = _reviewRepository.GetReview(reviewId);

      if(!_reviewRepository.DeleteReview(reviewToDelete))
      {
        ModelState.AddModelError("", $"Something went wrong deleting review");
        return StatusCode(500, ModelState);
      }

      return NoContent();
    }
  }
}
