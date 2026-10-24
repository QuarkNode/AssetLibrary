namespace AssetLibrary.Services.Services
{
    using AssetLibrary.Data.Models;
    using AssetLibrary.Services.Contracts;
    using AssetLibrary.Services.Models.Account;
    using Microsoft.AspNetCore.Identity;

    public class AccountService : IAccountService
    {
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;

        public AccountService(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        public async Task<SignInResult> LoginAsync(LoginViewModel model)
        {
            return await signInManager.PasswordSignInAsync(
                model.Username,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);
        }

        public async Task<IdentityResult> RegisterAsync(RegisterViewModel model)
        {
            User user = CreateUser(model);

            var identityResult = await this.userManager.CreateAsync(user, model.Password);

            if (!identityResult.Succeeded)
            {
                return identityResult;
            }

            await this.signInManager.SignInAsync(user, isPersistent: false);

            return IdentityResult.Success;
        }

        public async Task LogoutAsync()
        {
            await signInManager.SignOutAsync();
        }

        // Method
        public static User CreateUser(RegisterViewModel model)
        {
            User user = new User
            {
                UserName = model.Username,
                Email = model.Email,
                CompanyName = model.CompanyName,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Gender = model.Gender,
                DateOfBirth = model.DateOfBirth,
                City = model.City,
                Country = model.Country,
                CreatedOn = DateTime.UtcNow
            };

            return user;
        }
    }
}
