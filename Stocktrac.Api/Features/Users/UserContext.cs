using System.Security.Claims;

namespace Stocktrac.Api.Features.Users
{
    public class UserContext(IHttpContextAccessor accessor)
    {
        private readonly IHttpContextAccessor _accessor = accessor;

        public object? Identity
        {
            get
            {
                return _accessor.HttpContext?.User.Identity;
            }
        }

        public IEnumerable<Claim>? Claims
        {
            get
            {
                return _accessor.HttpContext?.User.Claims;
            }
        }
    }
}
