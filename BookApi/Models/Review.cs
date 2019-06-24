using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BookApi.Models
{
  public class Review
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    [StringLength(200, MinimumLength = 10, ErrorMessage ="must be between 10 - 200")]
    public string Headline { get; set; }
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "must be between 10 - 2000")]
    public string ReviewText { get; set; }
    [Range(1,5, ErrorMessage ="rating must be between 1 and 5 stars")]
    public int Rating { get; set; }

    public virtual Reviewer Reviewer { get; set; }
    public virtual Book Book { get; set; }
  }
}
