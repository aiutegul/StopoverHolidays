using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace StopoverAdminPanel.UserModels
{
	public class RegisterUserModel
	{
		[Required]
		[Display(Name = "User name")]
		public string UserName { get; set; }

		[Required(ErrorMessage = "Please select a role")]
		[Display(Name = "Role")]
		public string role { get; set; }

		[Required(ErrorMessage = "Please select a Partner")]
		[Display(Name = "Partner")]
		public int PartnerId { get; set; }

		public List<SelectListItem> roles { get; set; }
		public List<SelectListItem> partners { get; set; }
	}
}