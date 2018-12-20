using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using StopoverAdminPanel.Auth;
using StopoverAdminPanel.Models;

namespace StopoverAdminPanel.Controllers
{
	[RoutePrefix("api/Account")]
	public class AccountController : ApiController
	{
		private readonly AuthRepository _repo;
		private readonly StopoverDbContext _context;

		public AccountController()
		{
			_repo = new AuthRepository();
			_context = new StopoverDbContext();
		}

		// POST api/Account/Register
		// MUST DELETE IN PROD OR ONLY ADMIN CAN REGISTER USERS
		[Authorize(Roles = "Admin")]
		[Route("Register")]
		public async Task<IHttpActionResult> Register(UserModel userModel)
		{
			if (!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			IdentityResult result = await _repo.RegisterUser(userModel);

			IHttpActionResult errorResult = GetErrorResult(result);

			if (errorResult != null)
			{
				return errorResult;
			}

			return Ok();
		}

		[Authorize(Roles = "Admin")]
		[Route("GetPartnersAndRoles")]
		[HttpGet]
		public IHttpActionResult GetPartnersAndRoles()
		{
			var partnersList = _context.Partner.Select(p => new
			{
				p.Id,
				p.Code
			}).ToList();
			var rolelist = _repo.GetListRoles();
			return Ok(new { partners = partnersList, roles = rolelist });
		}

		[Authorize(Roles = "Admin")]
		[Route("Users")]
		[HttpGet]
		public IHttpActionResult GetUsers()
		{
			return Ok(_repo.GetUsers());
		}

		[Authorize(Roles = "Admin")]
		[HttpGet]
		[Route("{id}/roles")]
		public async Task<IHttpActionResult> GetRoles(string id)
		{
			return Ok(await _repo.UserRoles(id));
		}

		[Authorize(Roles = "Admin")]
		[HttpGet]
		[Route("{id}")]
		public async Task<IHttpActionResult> Get(string id)
		{
			return Ok(await _repo.FindUserById(id));
		}

		[Authorize(Roles = "Admin")]
		[HttpDelete]
		[Route("{id}")]
		[HttpPut]
		public async Task<IHttpActionResult> Edit([FromBody] FormattedUser user)
		{
			var result = await _repo.EditUser(user);
			if (result.Succeeded)
			{
				return Ok();
			}
			return BadRequest("Can't edit");
		}

		[Authorize(Roles = "Admin")]
		[HttpDelete]
		[Route("{id}")]
		public async Task<IHttpActionResult> Delete(string id)
		{
			var result = await _repo.DeleteUser(id);
			var errorResult = GetErrorResult(result);

			if (errorResult != null)
			{
				return errorResult;
			}
			return Ok();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_repo.Dispose();
				_context.Dispose();
			}

			base.Dispose(disposing);
		}

		private IHttpActionResult GetErrorResult(IdentityResult result)
		{
			if (result == null)
			{
				return InternalServerError();
			}

			if (!result.Succeeded)
			{
				if (result.Errors != null)
				{
					foreach (string error in result.Errors)
					{
						ModelState.AddModelError("", error);
					}
				}

				if (ModelState.IsValid)
				{
					// No ModelState errors are available to send, so just return an empty BadRequest.
					return BadRequest();
				}

				return BadRequest(ModelState);
			}

			return null;
		}
	}
}