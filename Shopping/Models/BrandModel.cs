using System.ComponentModel.DataAnnotations;

namespace Shopping.Models
{
    public class BrandModel
    {
		[Key]
		public int Id { get; set; }
		[Required, MinLength(4, ErrorMessage = "Yêu cầu nhập Tên Thương Hiệu")]
		public string Name { get; set; }

		[Required, MinLength(4, ErrorMessage = "Yêu cầu nhập Mô tả Thương Hiệu")]
		public string Description { get; set; }
		[Required]
		public string Slug { get; set; }
		public int Status { get; set; }
	}
}
