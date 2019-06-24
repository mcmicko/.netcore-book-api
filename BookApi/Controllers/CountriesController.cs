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
  [Route("api/[controller]")]
  [ApiController]
  public class CountriesController : Controller
  {
    private ICountryRepository _countryRepository;
    private IAuthorRepository _authorRepository;

    public CountriesController(ICountryRepository countryRepository, IAuthorRepository authorRepository)
    {
      _countryRepository = countryRepository;
      _authorRepository = authorRepository;
    }

    [HttpGet]//GET
    [ProducesResponseType(400)]
    [ProducesResponseType(200, Type = typeof(IEnumerable<CountryDto>))]
    public IActionResult GetCountries()
    {
      if(!ModelState.IsValid)
          return BadRequest(ModelState);

      var countries = _countryRepository.GetCountries().ToList();

      var countriesDto = new List<CountryDto>();
      foreach(var country in countries)
      {
        countriesDto.Add(new CountryDto
        {
          Id = country.Id,
          Name = country.Name
        });
      }

      return Ok(countriesDto);
    }

    //api/countries/countryId
    [HttpGet("{countryId}", Name = "GetCountry")]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(200, Type = typeof(CountryDto))]
    public IActionResult GetCountry(int countryId)
    {
      if (!_countryRepository.CountryExists(countryId))
        return NotFound();

      var country = _countryRepository.GetCountry(countryId);

      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      var countryDto = new CountryDto()
      {
        Id = country.Id,
        Name = country.Name
      };

      return Ok(countryDto);
    }


    //api/countries/authors/countryId
    [HttpGet("authors/{authorId}")]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(200, Type = typeof(CountryDto))]
    public IActionResult GetCountryOfAnAuthor(int authorId)
    {
      //to do  - validate the author exists
      if (!_authorRepository.AuthorExists(authorId))
        return NotFound();

      var country = _countryRepository.GetCountryOfAnAuthor(authorId);

      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      var countryDto = new CountryDto()
      {
        Id = country.Id,
        Name = country.Name
      };

      return Ok(countryDto);
    }

    //to do getAuthorsFromACountry
    //api/countries/countryId/authors
    [HttpGet("{countryId}/authors")]
    [ProducesResponseType(200, Type = typeof(IEnumerable<AuthorDto>))]
    [ProducesResponseType(404)]
    [ProducesResponseType(400)]
    public IActionResult GetAuthorsFromACountry(int countryId)
    {
      if (!_countryRepository.CountryExists(countryId))
        return NotFound();

      var authors = _countryRepository.GetAuthorsFromACountry(countryId);

      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      var authorsDto = new List<AuthorDto>();

      foreach(var author in authors)
      {
        authorsDto.Add(new AuthorDto
        {
          Id = author.Id,
          FirstName = author.FirstName,
          LastName = author.Lastname
        });
      }
      return Ok(authorsDto);
    }

    // POST api/countries
    [HttpPost("")]
    public IActionResult CreateCountry([FromBody]Country countryToCreate)
    {
      if (countryToCreate == null)
        return BadRequest(ModelState);

      var country = _countryRepository.GetCountries().Where(c => c.Name.Trim().ToUpper() == countryToCreate.Name.Trim().ToUpper()).FirstOrDefault();

      if(country != null)
      {
        ModelState.AddModelError("", $"Country {countryToCreate.Name} already exists");
        return StatusCode(422, $"Country {countryToCreate.Name} already exists");
      }
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      if(!_countryRepository.CreateCountry(countryToCreate))
      {
        ModelState.AddModelError("", $"Country {countryToCreate.Name}");
        return StatusCode(500, ModelState);
      }

      return CreatedAtRoute("GetCountry", new { countryId = countryToCreate.Id }, countryToCreate);
    }

    //DELETE
    //api/countries/countryId
    [HttpPut("{countryId}")]
    public IActionResult UpdateCountry(int countryId, [FromBody]Country updateCountryInfo)
    {
      if (updateCountryInfo == null)
        return BadRequest(ModelState);

      if (countryId != updateCountryInfo.Id)
        return BadRequest(ModelState);

      if (!_countryRepository.CountryExists(countryId))
        return NotFound();

      if(_countryRepository.IsDuplicateCountryName(countryId, updateCountryInfo.Name))
      {
        ModelState.AddModelError("", $"Country {updateCountryInfo.Name} already exists");
        return StatusCode(422, ModelState);
      }

      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      if(!_countryRepository.UpdateCountry(updateCountryInfo))
      {
        ModelState.AddModelError("", $"Something went wrong updating {updateCountryInfo}");
        return StatusCode(500, ModelState);
      }

      return NoContent();
    }

    //DELETE
    [HttpDelete("{countryId}")]
    [ProducesResponseType(204)] //no content
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    [ProducesResponseType(500)]
    public IActionResult DeleteCountry(int countryId)
    {
      if (!_countryRepository.CountryExists(countryId))
        return NotFound();

      var countryToDelete = _countryRepository.GetCountry(countryId);

      if (_countryRepository.GetAuthorsFromACountry(countryId).Count() > 0)
      {
        ModelState.AddModelError("", $"Country {countryToDelete.Name} " +
                                      "cannot be deleted because it is used by at least one author");
        return StatusCode(409, ModelState);
      }

      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      if (!_countryRepository.DeleteCountry(countryToDelete))
      {
        ModelState.AddModelError("", $"Something went wrong deleting {countryToDelete.Name}");
        return StatusCode(500, ModelState);
      }

      return NoContent();
    }
  }
}