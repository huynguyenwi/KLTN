﻿using Shopping.Repository.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shopping.Models
{
	public class SliderModel
	{
		[Key]
		public int Id { get; set; }
		[Required(ErrorMessage = "Yêu cầu nhập tên slider")]
		public string Name { get; set; }

		[Required(ErrorMessage = "Yêu cầu nhập Mô tả ")]
		public string Description { get; set; }

		public int? Status { get; set; }

		public string Image {  get; set; }
		[NotMapped]
		[FileExtension]
		public IFormFile? ImageUpload { get; set; }
	}
}
