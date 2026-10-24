using AssetLibrary.Services.Models.Home;

namespace AssetLibrary.Services.Contracts
{
    public interface IHomeService
    {
        public Task<HomeViewModel> GetHomeViewModelAsync(int? userId);
    }
}
