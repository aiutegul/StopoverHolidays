using System.Collections.Generic;

namespace StopoverAdminPanel.Auth
{
	public class EmailDestination
	{
		public string Description { get; set; } = "About account";
		public List<Addresses> Addresses { get; set; }
	}
}