using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models
{
    public class Article
    {
        [Key]
        public int Id { get; set; }

        [StringLength(255, MinimumLength = 5, ErrorMessage = "{0} phải dài từ {2} tới {1} ký tự")]
        [Required(ErrorMessage = "{0} phải nhập")]
        [Column(TypeName = "varchar(255)")]  
        [DisplayName("Tiêu đề")]
        public string Title { get; set; }

        [DataType(DataType.Date)]
        [Required(ErrorMessage = "{0} phải nhập")]
        [DisplayName("Ngày tạo")]
        public DateTime Created { get; set; }

        [Column(TypeName = "text")]  
        [DisplayName("Nội dung")]
        public string Content { get; set; }
    }
}
