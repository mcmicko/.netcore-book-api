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
  public class CategoriesController : Controller
  {
    private ICategoryRepository _categoryRepository;
    private IBookRepository _bookRepository;

    public CategoriesController(ICategoryRepository categoryRepository, IBookRepository bookRepository)
    {
      _categoryRepository = categoryRepository;
      _bookRepository = bookRepository;
    }



    [HttpGet]//GET
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<CategoryDto>))]
    public IActionResult GetCategories()
    {
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      var categories = _categoryRepository.GetCategories().ToList();

      var categoriesDto = new List<CategoryDto>();
      foreach (var category in categories)
      {
        categoriesDto.Add(new CategoryDto
        {
          Id = category.Id,
          Name = category.Name
        });
      }

      return Ok(categoriesDto);
    }


    //api/categories/categoryId
    [HttpGet("{categoryId}", Name = "GetCategory")]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(200, Type = typeof(CategoryDto))]
    public IActionResult GetCategory(int categoryId)
    {
      if (!_categoryRepository.CategoryExists(categoryId))
        return NotFound();

      var category = _categoryRepository.GetCategory(categoryId);

      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      var categoryDto = new CategoryDto()
      {
        Id = category.Id,
        Name = category.Name
      };

      return Ok(categoryDto);
    }


    //api/categories/books/countryId
    [HttpGet("books/{bookId}")]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(200, Type = typeof(CategoryDto))]
    public IActionResult GetAllCategoriesForABook(int bookId)
    {
      //to do  - validate the author exists
      if (!_bookRepository.BookExists(bookId))
        return BadRequest();

      var categories = _categoryRepository.GetAllCategoriesForBook(bookId);

      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      var categoriesDto = new List<CategoryDto>();
      foreach(var category in categories)
      {
        categoriesDto.Add(new CategoryDto()
        {
          Id = category.Id,
          Name = category.Name
        });
      }

      return Ok(categoriesDto);
    }


    //to do getAuthorsFromACountry
    //api/categories/categoryI/books
    [HttpGet("{categoryId}/books")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<BookDto>))]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public IActionResult GetAllBooksForCategory(int categoryId)
    {
      if (!_categoryRepository.CategoryExists(categoryId))
        return NotFound();

      var books = _categoryRepository.GetAllBooksForCategory(categoryId);

      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      var booksDto = new List<BookDto>();

      foreach (var book in books)
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


    //POST
    [HttpPost]
    public IActionResult CreateCategory([FromBody]Category categoryToCreate)
    {
      if (categoryToCreate == null)
        return BadRequest(ModelState);

      var category = _categoryRepository.GetCategories()
        .Where(c => c.Name.Trim().ToUpper() == categoryToCreate.Name.Trim().ToUpper())
        .FirstOrDefault();

      if(category != null)
      {
        ModelState.AddModelError("", $"Category {categoryToCreate.Name} already exists");
        return StatusCode(422, ModelState);
      }
      if (!ModelState.IsValid)
        return BadRequest(ModelState);
      if(!_categoryRepository.CreateCategory(categoryToCreate))
      {
        ModelState.AddModelError("", $"Something went wrong saving {categoryToCreate.Name}");
        return StatusCode(500, ModelState);
      }

      return CreatedAtRoute("GetCategory", new { categoryId = categoryToCreate.Id }, categoryToCreate);
    }


    //PUT
    [HttpPut("{categoryId}")]
    public IActionResult UpdateCategory(int categoryId, [FromBody]Category updatedCategoryInfo)
    {
      if (updatedCategoryInfo == null)
        return BadRequest(ModelState);
      if (categoryId != updatedCategoryInfo.Id)
        return BadRequest(ModelState);
      if (!_categoryRepository.CategoryExists(categoryId))
        return NotFound();
      if(_categoryRepository.IsDuplicateCategoryName(categoryId, updatedCategoryInfo.Name))
      {
        ModelState.AddModelError("", $"Something went wrong updating {updatedCategoryInfo.Name}");
        return StatusCode(500, ModelState);
      }
      if (!ModelState.IsValid)
        return BadRequest();
      if(!_categoryRepository.UpdateCategory(updatedCategoryInfo))
      {
        ModelState.AddModelError("", $"Something went wrong updating {updatedCategoryInfo.Name}");
      }

      return NoContent();
    }


    //DELETE
    [HttpDelete("{categoryId}")]
    public IActionResult DeleteCategory(int categoryId)
    {
      if (!_categoryRepository.CategoryExists(categoryId))
        return NotFound();

      var categoryToDelete = _categoryRepository.GetCategory(categoryId);

      if(_categoryRepository.GetAllBooksForCategory(categoryId).Count() > 0)
      {
        ModelState.AddModelError("", $"Category {categoryToDelete.Name}" + "cannot be deleted beacuse it is used by at least one book");
        return StatusCode(409, ModelState);
      }
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      if(!_categoryRepository.DeleteCategory(categoryToDelete))
      {
        ModelState.AddModelError("", $"Something went wrong deleting {categoryToDelete.Name}");
        return StatusCode(500, ModelState);
      }

      return NoContent();
    }
  }
}
