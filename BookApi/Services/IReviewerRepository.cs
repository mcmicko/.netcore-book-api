using BookApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookApi.Services
{
  public interface IReviewerRepository
  {
    ICollection<Reviewer> GetReviewers();
    Reviewer GetReviewer(int reviewerId);
    ICollection<Review> GetReviewsByReviewer(int reviewerId);
    Reviewer GetReviewerOfReviewer(int reviewId);
    bool ReviwererExists(int reviewerId);

    bool CreateReviewer(Reviewer reviewer);
    bool UpdateReviewer(Reviewer reviewer);
    bool DeleteReviewer(Reviewer reviewer);
    bool Save();
  }
}
