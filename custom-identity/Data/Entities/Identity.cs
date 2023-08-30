using Microsoft.AspNetCore.Identity;

namespace custom_identity.Data.Entities
{
    public class User : IdentityUser<long>
    {
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
    }
    public class UserLogin : IdentityUserLogin<long> { }
    public class UserRole : IdentityUserRole<long> { }
    public class UserClaim : IdentityUserClaim<long> { }
    public class Role : IdentityRole<long> { }
    public class RoleClaim : IdentityRoleClaim<long> { }
    public class UserToken : IdentityUserToken<long> { }
}
