using BookApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookApi.Services
{
  public interface IAuthorRepository
  {
    ICollection<Author> GetAuthors();
    Author GetAuthor(int authorId);
    ICollection<Author> GetAuthorsOfABook(int bookId);
    ICollection<Book> GetBooksByAuthor(int authorid);
    bool AuthorExists(int authorId);

    bool CreateAuthor(Author author);
    bool UpdateAuthor(Author author);
    bool DeleteAuthor(Author author);
    bool Save();
  }
}
