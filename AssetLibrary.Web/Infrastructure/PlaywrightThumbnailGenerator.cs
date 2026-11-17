
namespace AssetLibrary.Web.Infrastructure
{
    using AssetLibrary.Services.Contracts;
    using Microsoft.Playwright;

    public sealed class PlaywrightThumbnailGenerator : IThumbnailRendererService
    {
        // Generates a thumbnail for one model.
        public async Task<byte[]> RenderAssetAsync(string rendererUrl, string modelUrl)
        {
            using var playwright = await Playwright.CreateAsync();

            await using var browser = await playwright.Chromium.LaunchAsync(
                new BrowserTypeLaunchOptions
                {
                    Headless = true
                });

            var page = await browser.NewPageAsync();

            await page.GotoAsync(rendererUrl);

            await page.WaitForFunctionAsync("() => typeof window.generateModelThumbnail === 'function'");

            string base64Thumbnail = await page.EvaluateAsync<string>("async (modelUrl) => await window.generateModelThumbnail(modelUrl)", modelUrl);

            return Convert.FromBase64String(base64Thumbnail);
        }

        // Generates a thumbnail for multiple assembled models.
        public async Task<byte[]> RenderProjectAsync(string rendererUrl, IReadOnlyCollection<string> modelUrls)
        {
            ArgumentNullException.ThrowIfNull(modelUrls);

            if (modelUrls.Count == 0)
            {
                throw new ArgumentException("At least one model URL is required.", nameof(modelUrls));
            }

            using var playwright = await Playwright.CreateAsync();

            await using var browser = await playwright.Chromium.LaunchAsync(
                new BrowserTypeLaunchOptions
                {
                    Headless = true
                });

            var page = await browser.NewPageAsync();

            await page.GotoAsync(rendererUrl);

            await page.WaitForFunctionAsync("() => typeof window.generateProjectThumbnail === 'function'");

            string base64Thumbnail = await page.EvaluateAsync<string>("async (modelUrls) => await window.generateProjectThumbnail(modelUrls)", modelUrls.ToArray());

            return Convert.FromBase64String(base64Thumbnail);
        }
    }
}
