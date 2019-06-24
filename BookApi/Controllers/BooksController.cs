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
  [Route("api/[controller]")]
  public class BooksController : Controller
  {
    private IBookRepository _booksRepository;
    private IAuthorRepository _authorRepository;
    private ICategoryRepository _categoryRepository;
    private IReviewRepository _reviewRepository;

    public BooksController(IBookRepository booksRepository, IAuthorRepository authorRepository, ICategoryRepository categoryRepository, IReviewRepository reviewRepository)
    {
      _booksRepository = booksRepository;
      _authorRepository = authorRepository;
      _categoryRepository = categoryRepository;
      _reviewRepository = reviewRepository;
    }


    [HttpGet]
    public IActionResult GetBooks()
    {
      var books = _booksRepository.GetBooks();

      if (!ModelState.IsValid)
        return BadRequest();

      var booksDto = new List<BookDto>();

      foreach(var book in books)
      {
        booksDto.Add(new BookDto()
        {
          Id = book.Id,
          Title = book.Title,
          Isbn = book.Isbn,
          DatePublished = book.DatePublished
        });
      }
      return Ok(booksDto);
    }


    [HttpGet("{bookId}", Name = "GetBook")]
    public IActionResult GetBook(int bookId)
    {
      if (!_booksRepository.BookExists(bookId))
        return NotFound();

      var book = _booksRepository.GetBook(bookId);

      if (!ModelState.IsValid)
        return BadRequest();

      var bookDto = new BookDto()
      {
        Id = book.Id,
        Title = book.Title,
        Isbn = book.Isbn,
        DatePublished = book.DatePublished
      };

      return Ok(bookDto);
    }
    //api/books/isbn/bookIsbn
    [HttpGet("ISBN/{bookIsbn}")]
    public IActionResult GetBook(string bookIsbn)
    {
      if (!_booksRepository.BookExists(bookIsbn))
        return NotFound();

      var book = _booksRepository.GetBook(bookIsbn);

      if (!ModelState.IsValid)
        return BadRequest();

      var bookDto = new BookDto()
      {
        Id = book.Id,
        Title = book.Title,
        Isbn = book.Isbn,
        DatePublished = book.DatePublished
      };

      return Ok(bookDto);
    }

    [HttpGet("{bookId}/rating")]
    [ProducesResponseType(200, Type = typeof(decimal))]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public IActionResult GetBookRating(int bookId)
    {
      if (!_booksRepository.BookExists(bookId))
        return NotFound();

      var rating = _booksRepository.GetBookRating(bookId);

      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      return Ok(rating);
    }

    //POST
    [HttpPost]
    public IActionResult CreateBook([FromQuery] List<int> authId, [FromQuery] List<int> catId,
      [FromBody]Book bookToCreate)
    {
      var statusCode = ValidateBook(authId, catId, bookToCreate);
      if (!ModelState.IsValid)
        return StatusCode(statusCode.StatusCode);
      if(!_booksRepository.CreateBook(authId, catId, bookToCreate))
      {
        ModelState.AddModelError("", $"Something went wrong saving the book {bookToCreate.Title}");
        return StatusCode(500, ModelState);
      }
      return CreatedAtRoute("GetBook", new { bookId = bookToCreate.Id }, bookToCreate);
    }


    //PUT
    [HttpPut("{bookId}")]
    public IActionResult UpdateBook(int bookId, [FromQuery]List<int> authId, [FromQuery]List<int> catId, [FromBody]Book bookToUpdate)
    {
      var statusCode = ValidateBook(authId, catId, bookToUpdate);
      if (bookId != bookToUpdate.Id)
        return BadRequest();
      if (!_booksRepository.BookExists(bookId))
        return NotFound();
      if (!ModelState.IsValid)
        return StatusCode(statusCode.StatusCode);
      if(!_booksRepository.UpdateBook(authId, catId, bookToUpdate))
      {
        ModelState.AddModelError("", $"Something went wrong updating the book {bookToUpdate}");
        return StatusCode(500, ModelState);
      }
      return NoContent();
    }


    //DELETE
    [HttpDelete("{bookId}")]
    public IActionResult DeleteBook(int bookId)
    {
      if (!_booksRepository.BookExists(bookId))
        return NotFound();

      var reviewsToDelete = _reviewRepository.GetReviewsOfABook(bookId);
      var bookToDelete = _booksRepository.GetBook(bookId);

      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      if (!_reviewRepository.DeleteReviews(reviewsToDelete.ToList()))
      {
        ModelState.AddModelError("", $"Something went wrong deleting reviews");
        return StatusCode(500, ModelState);
      }

      if (!_booksRepository.DeleteBook(bookToDelete))
      {
        ModelState.AddModelError("", $"Something went wrong deleting book {bookToDelete.Title}");
        return StatusCode(500, ModelState);
      }

      return NoContent();
    }


    //VALIDATION
    private StatusCodeResult ValidateBook(List<int>authId, List<int>catId, Book book)
    {
      if(book == null || authId.Count() <= 0 || catId.Count() <= 0)
      {
        ModelState.AddModelError("", $"missing book, author, or category");
      }
      if(_booksRepository.IsDuplicateIsbn(book.Id, book.Isbn))
      {
        ModelState.AddModelError("", $"Duplicate ISBN");
        return StatusCode(422);
      }

      foreach(var id in authId)
      {
        if(!_authorRepository.AuthorExists(id))
        {
          ModelState.AddModelError("", "Author not found");
          return StatusCode(404);
        }
      }
      foreach (var id in catId)
      {
        if (!_categoryRepository.CategoryExists(id))
        {
          ModelState.AddModelError("", "Category not found");
          return StatusCode(404);
        }
      }

      if(!ModelState.IsValid)
      {
        ModelState.AddModelError("", "Critical error");
        return BadRequest();
      }

      return NoContent();
    }
  }
}
