using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BookApi.Models
{
  public class Author
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    [MaxLength(100, ErrorMessage ="first name cannot be more thab 100 chacaters")]
    public string FirstName { get; set; }
    [Required]
    [MaxLength(200, ErrorMessage = "last name cannot be more thab 200 chacaters")]
    public string Lastname { get; set; }

    public virtual Country Country { get; set; }
    public virtual ICollection<BookAuthor> BookAuthors { get; set; }
  }
}
