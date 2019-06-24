using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookApi.Models;

namespace BookApi.Services
{
  public class BookRepository : IBookRepository
  {
    private BookDbContext _bookContext;
    public BookRepository(BookDbContext bookContext)
    {
      _bookContext = bookContext;
    }


    public bool BookExists(int bookId)
    {
      return _bookContext.Books.Any(b => b.Id == bookId);
    }

    public bool BookExists(string bookIsbn)
    {
      return _bookContext.Books.Any(b => b.Isbn == bookIsbn);
    }

    public bool CreateBook(List<int> authorsId, List<int> categoriesId, Book book)
    {
      var authors = _bookContext.Authors.Where(a => authorsId.Contains(a.Id)).ToList();
      var categories = _bookContext.Categories.Where(a => categoriesId.Contains(a.Id)).ToList();

      foreach(var author in authors)
      {
        var bookAuthor = new BookAuthor()
        {
          Author = author,
          Book = book
        };
        _bookContext.Add(bookAuthor);
      }
      foreach(var category in categories)
      {
        var bookCategory = new BookCategory()
        {
          Category = category,
          Book = book
        };
        _bookContext.Add(bookCategory);
      }

      _bookContext.Add(book);

      return Save();
    }

    public bool DeleteBook(Book book)
    {
      _bookContext.Remove(book);
      return Save();
    }

    public Book GetBook(int bookId)
    {
      return _bookContext.Books.Where(b => b.Id == bookId).FirstOrDefault();
    }

    public Book GetBook(string bookIsbn)
    {
      return _bookContext.Books.Where(b => b.Isbn == bookIsbn).FirstOrDefault();
    }

    public decimal GetBookRating(int bookId)
    {
      var reviews = _bookContext.Reviews.Where(r => r.Book.Id == bookId);
      if (reviews.Count() <= 0)
        return 0;

      return ((decimal)reviews.Sum(r => r.Rating) / reviews.Count());
    }

    public ICollection<Book> GetBooks()
    {
      return _bookContext.Books.OrderBy(b => b.Title).ToList();
    }

    public bool IsDuplicateIsbn(int bookId, string bookIsbn)
    {
      var book = _bookContext.Books.Where(b => b.Isbn.Trim().ToUpper() == bookIsbn.Trim().ToUpper()
                                          && b.Id != bookId).FirstOrDefault();
      return book == null ? false : true;
    }

    public bool Save()
    {
      var saved = _bookContext.SaveChanges();
      return saved >= 0 ? true : false;
    }

    public bool UpdateBook(List<int> authorsId, List<int> categoriesId, Book book)
    {
      var authors = _bookContext.Authors.Where(a => authorsId.Contains(a.Id)).ToList();
      var categories = _bookContext.Categories.Where(c => categoriesId.Contains(c.Id)).ToList();

      var bookAuthorsToDelete = _bookContext.BookAuthors.Where(b => b.BookId == book.Id);
      var bookCategoriesToDelete = _bookContext.BookCategories.Where(b => b.BookId == book.Id);

      _bookContext.RemoveRange(bookAuthorsToDelete);
      _bookContext.RemoveRange(bookCategoriesToDelete);

      foreach (var author in authors)
      {
        var bookAuthor = new BookAuthor()
        {
          Author = author,
          Book = book
        };
        _bookContext.Add(bookAuthor);
      }
      foreach(var category in categories)
      {
        var bookCategories = new BookCategory()
        {
          Category = category,
          Book = book
        };
        _bookContext.Add(bookCategories);
      }

      _bookContext.Update(book);

      return Save();
    }
  }
}
