namespace AssetLibrary.Services.Contracts
{
    using AssetLibrary.Services.Models.Account;
    using Microsoft.AspNetCore.Identity;
    public interface IAccountService
    {
        public Task<SignInResult> LoginAsync(LoginViewModel model);
        public Task LogoutAsync();
        public Task<IdentityResult> RegisterAsync(RegisterViewModel model);
    }
}
