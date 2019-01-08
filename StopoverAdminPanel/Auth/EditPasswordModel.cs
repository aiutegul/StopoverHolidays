using System.ComponentModel.DataAnnotations;

namespace StopoverAdminPanel.Auth
{
	public class EditPasswordModel
	{
		public string Username { get; set; }

		[Required]
		[Display(Name = "Previous password")]
		[DataType(DataType.Password)]
		public string PrevPassword { get; set; }

		[Required]
		[Display(Name = "New password")]
		[DataType(DataType.Password)]
		public string NewPassword { get; set; }
	}
}