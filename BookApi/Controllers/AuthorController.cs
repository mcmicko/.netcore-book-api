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
  public class AuthorController : Controller
  {
    private IAuthorRepository _authorRepository;
    private IBookRepository _bookRepository;
    private ICountryRepository _countryRepository;
    public AuthorController(IAuthorRepository authorRepository, IBookRepository bookRepository,
        ICountryRepository countryRepository)
    {
      _authorRepository = authorRepository;
      _bookRepository = bookRepository;
      _countryRepository = countryRepository;
    }


    [HttpGet]
    public IActionResult GetAuthors()
    {
      var authors = _authorRepository.GetAuthors();

      if (!ModelState.IsValid)
        return BadRequest();

      var authorDto = new List<AuthorDto>();

      foreach(var author in authors)
      {
        authorDto.Add(new AuthorDto
        {
          Id = author.Id,
          FirstName = author.FirstName,
          LastName = author.Lastname,

        });
      }
      return Ok(authorDto);
    }


    [HttpGet("{authorId}", Name = "GetAuthor")]
    [ProducesResponseType(200, Type = typeof(AuthorDto))]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public IActionResult GetAuthor(int authorId)
    {
      if (!_authorRepository.AuthorExists(authorId))
        return NotFound();

      var author = _authorRepository.GetAuthor(authorId);

      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      var authorDto = new AuthorDto()
      {
        Id = author.Id,
        FirstName = author.FirstName,
        LastName = author.Lastname
      };

      return Ok(authorDto);
    }


    [HttpGet("{authorId}/books")]
    [ProducesResponseType(200, Type = typeof(AuthorDto))]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public IActionResult GetBookByAuthor(int authorId)
    {
      if (!_authorRepository.AuthorExists(authorId))
        return NotFound();

      var books = _authorRepository.GetBooksByAuthor(authorId);

      if (!ModelState.IsValid)
        return BadRequest();

      var booksDto = new List<BookDto>();
      foreach(var book in books)
      {
        booksDto.Add(new BookDto
        {
          Id = book.Id,
          Title = book.Title,
          Isbn = book.Isbn,
          DatePublished = book.DatePublished
        });
      }
      return Ok(booksDto);
    }


    [HttpGet("books/{bookId}")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<AuthorDto>))]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public IActionResult GetAuthorsOfABook(int bookId)
    {
      var authors = _authorRepository.GetAuthorsOfABook(bookId);

      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      var authorDto = new List<AuthorDto>();
      foreach(var author in authors)
      {
        authorDto.Add(new AuthorDto
        {
          Id = author.Id,
          FirstName = author.FirstName,
          LastName = author.Lastname
        });
      }

      return Ok(authorDto);
    }


    //POST
    [HttpPost]
    public IActionResult CreateAuthor([FromBody]Author authorToCreate)
    {
      if (authorToCreate == null)
        return BadRequest(ModelState);
      if (!_countryRepository.CountryExists(authorToCreate.Country.Id))
      {
        ModelState.AddModelError("", "Country doesnt exist");
        return StatusCode(404, ModelState);
      }
      authorToCreate.Country = _countryRepository.GetCountry(authorToCreate.Country.Id);

      if (!ModelState.IsValid)
        return BadRequest(ModelState);
      if(!_authorRepository.CreateAuthor(authorToCreate))
      {
        ModelState.AddModelError("", $"Something went wrong saving the author {authorToCreate.FirstName}");
      }

      return CreatedAtRoute("GetAuthor", new { authorId = authorToCreate.Id }, authorToCreate);
    }


    //PUT
    [HttpPut("{authorId}")]
    public IActionResult UpdateAuthors(int authorId, [FromBody]Author authorToUpdate)
    {
      if (authorToUpdate == null)
        return BadRequest(ModelState);

      if (authorId != authorToUpdate.Id)
        return BadRequest(ModelState);

      if (!_authorRepository.AuthorExists(authorId))
        ModelState.AddModelError("", "Author doesnt exist");

      if (!_countryRepository.CountryExists(authorToUpdate.Country.Id))
        ModelState.AddModelError("", "Country doesnt exist");

      if (!ModelState.IsValid)
        return StatusCode(404, ModelState);

      authorToUpdate.Country = _countryRepository.GetCountry(authorToUpdate.Country.Id);

      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      if(!_authorRepository.UpdateAuthor(authorToUpdate))
      {
        ModelState.AddModelError("", $"Something went wrong updateing the athor" +
          $"{authorToUpdate.FirstName} {authorToUpdate.Lastname}");
        return StatusCode(500, ModelState);
      }

      return NoContent();
    }


    [HttpDelete("{authorId}")]
    public IActionResult DeleteAuthor(int authorId)
    {
      if (!_authorRepository.AuthorExists(authorId))
        return NotFound();

      var authorToDelete = _authorRepository.GetAuthor(authorId);

      if(_authorRepository.GetBooksByAuthor(authorId).Count() > 0)
      {
        ModelState.AddModelError("", $"Author {authorToDelete.FirstName} {authorToDelete.Lastname} cannot delete because it is associated with at least one book");
        return StatusCode(409, ModelState);
      }
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      if(!_authorRepository.DeleteAuthor(authorToDelete))
      {
        ModelState.AddModelError("", $"Something went wrong deleting {authorToDelete.FirstName} {authorToDelete.Lastname}");
        return StatusCode(500, ModelState);
      }

      return NoContent();
    }
  }
}
