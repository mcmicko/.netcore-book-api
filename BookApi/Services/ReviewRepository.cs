using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookApi.Models;

namespace BookApi.Services
{
  public class ReviewRepository : IReviewRepository
  {
    private BookDbContext _reviewContext;
    public ReviewRepository(BookDbContext reviewContext)
    {
      _reviewContext = reviewContext;
    }


    public Book GetBookOfAReview(int reviewId)
    {
      var bookId = _reviewContext.Reviews.Where(r => r.Id == reviewId).Select(b => b.Book.Id).FirstOrDefault();
      return _reviewContext.Books.Where(b => b.Id == bookId).FirstOrDefault();
    }

    public ICollection<Review> GetReviews()
    {
      return _reviewContext.Reviews.OrderBy(r => r.Rating).ToList();
    }

    public ICollection<Review> GetReviewsOfABook(int bookId)
    {
      return _reviewContext.Reviews.Where(b => b.Book.Id == bookId).ToList();
    }

    public Review GetReview(int reviewId)
    {
      return _reviewContext.Reviews.Where(r => r.Id == reviewId).FirstOrDefault();
    }

    public bool ReviewExists(int reviewId)
    {
      return _reviewContext.Reviews.Any(r => r.Id == reviewId);
    }

    public bool CreateReview(Review review)
    {
      _reviewContext.Add(review);
      return Save();
    }

    public bool UpdatedReview(Review review)
    {
      _reviewContext.Update(review);
      return Save();
    }

    public bool DeleteReview(Review review)
    {
      _reviewContext.Remove(review);
      return Save();
    }

    public bool Save()
    {
      var saved = _reviewContext.SaveChanges();
      return saved >= 0 ? true : false;
    }

    public bool DeleteReviews(List<Review> reviews)
    {
      _reviewContext.RemoveRange(reviews);
      return Save();
    }
  }
}
