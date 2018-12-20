using System.Data.Entity;
using Z.EntityFramework.Plus;

namespace StopoverAdminPanel.Audit
{
	public class AuditContext : DbContext
	{
		public AuditContext()
			: base("name=auditContext")
		{
		}

		public virtual DbSet<AuditEntry> AuditEntries { get; set; }
		public virtual DbSet<AuditEntryProperty> AuditEntryProperties { get; set; }
	}
}