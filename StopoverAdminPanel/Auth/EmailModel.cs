using System.Collections.Generic;
using System.Configuration;

namespace StopoverAdminPanel.Auth
{
	public class EmailModel
	{
		public string UserName { get; set; } = ConfigurationManager.AppSettings["emailSenderUsername"];
		public string Password { get; set; } = ConfigurationManager.AppSettings["emailSenderPassword"];
		public List<EmailDestination> Destinations { get; set; }
		public List<Messages> Messages { get; set; }
	}
}