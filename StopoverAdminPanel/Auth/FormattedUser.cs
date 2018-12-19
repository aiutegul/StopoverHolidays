using System.Collections.Generic;
using System.Web.Mvc;

namespace StopoverAdminPanel.Auth
{
	public class FormattedUser
	{
		public string Id { get; set; }
		public string UserName { get; set; }
		public string Role { get; set; }
		public int PartnerId { get; set; }
		public List<SelectListItem> Roles { get; set; }
		public List<SelectListItem> Partners { get; set; }
	}
}