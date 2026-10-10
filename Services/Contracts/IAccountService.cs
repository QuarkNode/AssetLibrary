
namespace PartManagementSystem.Services.Contracts
{
    using Microsoft.AspNetCore.Identity;

    using PartManagementSystem.ViewModels.Account;
    public interface IAccountService
    {
        public Task<SignInResult> LoginAsync(LoginViewModel model);
        public Task LogoutAsync();
        public Task<IdentityResult> RegisterAsync(RegisterViewModel model);
    }
}
