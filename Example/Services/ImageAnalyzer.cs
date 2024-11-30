using System.Net.Http.Headers;

namespace Example.Services;

public interface IImageAnalyzer
{
    Task<string> Analyze(byte[] image);
}

internal sealed class ImageAnalyzer(HttpClient httpClient) : IImageAnalyzer
{
    private const string ComputerVisionUri = "/computervision/imageanalysis:analyze?overload=stream&model-name=pooltablemodelv3&api-version=2023-04-01-preview";

    public async Task<string> Analyze(byte[] image)
    {
        using var content = new ByteArrayContent(image);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        using var response = await httpClient.PostAsync(ComputerVisionUri, content);

        return await response.Content.ReadAsStringAsync();
    }
}