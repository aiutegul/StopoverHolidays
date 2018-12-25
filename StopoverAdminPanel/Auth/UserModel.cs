using System.ComponentModel.DataAnnotations;

namespace StopoverAdminPanel.Auth
{
	public class UserModel
	{
		[Required]
		[DataType(DataType.EmailAddress)]
		[Display(Name = "User name")]
		public string UserName { get; set; }

		[Required(ErrorMessage = "Role is required")]
		public string Role { get; set; }
		public int? PartnerId { get; set; }
	}
}