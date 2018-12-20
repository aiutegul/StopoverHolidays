using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

namespace StopoverAdminPanel.Audit
{
	public class DbContextAuditable : DbContext
	{
		private static string _user;
		private static DbContext _auditContext;

		public DbContextAuditable(string nameOrConnectionString) : base(nameOrConnectionString)
		{
			if (System.Configuration.ConfigurationManager.AppSettings["auditEnable"].Equals("true"))
			{
				AuditManager.DefaultConfiguration.AutoSavePreAction = (context, audit) =>
				{
					if (_auditContext != null)
					{
						var auditDbSet = _auditContext.Set(typeof(AuditEntry));
						auditDbSet.AddRange(audit.Entries);
						_auditContext.SaveChanges();
					}
				};
			}
		}

		public static void SetUser(string user)
		{
			_user = user;
		}

		public static string GetUser()
		{
			if (_user != null)
			{
				return _user;
			}
			return "SYSTEM";
		}

		public static void SetAuditContext(DbContext context)
		{
			_auditContext = context;
		}

		public static DbContext GetAuditContext()
		{
			return _auditContext;
		}

		public override int SaveChanges()
		{
			var audit = new Z.EntityFramework.Plus.Audit();
			audit.CreatedBy = GetUser();
			audit.PreSaveChanges(this);
			var rowAffecteds = base.SaveChanges();
			audit.PostSaveChanges();

			if (audit.Configuration.AutoSavePreAction != null)
			{
				audit.Configuration.AutoSavePreAction(this, audit);
				base.SaveChanges();
			}

			return rowAffecteds;
		}

		public override Task<int> SaveChangesAsync()
		{
			return SaveChangesAsync(CancellationToken.None);
		}

		public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
		{
			var audit = new Z.EntityFramework.Plus.Audit();
			audit.PreSaveChanges(this);
			var rowAffecteds = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
			audit.PostSaveChanges();

			if (audit.Configuration.AutoSavePreAction != null)
			{
				audit.Configuration.AutoSavePreAction(this, audit);
				await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
			}

			return rowAffecteds;
		}
	}
}