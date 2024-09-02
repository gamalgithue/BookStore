using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Bulky.Models
{
    public class Category
    {

        [Key]
        public int Id { get; set; }


        [Required(ErrorMessage ="Category Name is Required")]
        [DisplayName("Category Name")]
        [MinLength(3)]
        
        public string Name { get; set; }

        [Required]
        [DisplayName("Display Order")]
        [Range(1,100,ErrorMessage ="Range between 1 and 100")]
        public int DisplayOrder { get; set; }
    }
}
