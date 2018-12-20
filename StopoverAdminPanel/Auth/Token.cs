namespace StopoverAdminPanel.Auth
{
	public class Token
	{
		public string access_token { get; set; }
		public string expires_in { get; set; }
		public string roles { get; set; }
		public string PartnerId { get; set; }
	}
}