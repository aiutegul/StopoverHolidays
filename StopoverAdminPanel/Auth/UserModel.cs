using System.ComponentModel.DataAnnotations;

namespace StopoverAdminPanel.Auth
{
	public class UserModel
	{
		[Required]
		[Display(Name = "User name")]
		public string UserName { get; set; }

		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "Password")]
		public string Password { get; set; }

		[Required(ErrorMessage = "Role is required")]
		public string Role { get; set; }
	}
}