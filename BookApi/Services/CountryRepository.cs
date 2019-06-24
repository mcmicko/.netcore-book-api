using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookApi.Models;

namespace BookApi.Services
{
  public class CountryRepository : ICountryRepository
  {
    private BookDbContext _countryContext;
    public CountryRepository(BookDbContext countryContext)
    {
      _countryContext = countryContext;
    }

    public bool CountryExists(int countryId)
    {
      return _countryContext.Countries.Any(c => c.Id == countryId);
    }

    public ICollection<Author> GetAuthorsFromACountry(int countryId)
    {
      return _countryContext.Authors.Where(c => c.Id == countryId).ToList();
    }

    public ICollection<Country> GetCountries()
    {
      return _countryContext.Countries.OrderBy(c => c.Name).ToList();
    }

    public Country GetCountry(int countryId)
    {
      return _countryContext.Countries.Where(c => c.Id == countryId).FirstOrDefault();
    }

    public Country GetCountryOfAnAuthor(int authorId)
    {
      return _countryContext.Authors.Where(a => a.Id == authorId).Select(c => c.Country).FirstOrDefault();
    }

    public bool IsDuplicateCountryName(int countryId, string countryName)
    {
      var country = _countryContext.Countries.Where(b => b.Name.Trim().ToUpper() == countryName.Trim().ToUpper() && b.Id != countryId).FirstOrDefault();

      return country == null ? false : true;
    }

    public bool Save()
    {
      var saved = _countryContext.SaveChanges();
      return saved >= 0 ? true : false;
    }

    public bool UpdateCountry(Country country)
    {
      _countryContext.Update(country);
      return Save();
    }

    public bool CreateCountry(Country country)
    {
     _countryContext.AddAsync(country);
      return Save();
    }

    public bool DeleteCountry(Country country)
    {
      _countryContext.Remove(country);
      return Save();
    }
  }
}
