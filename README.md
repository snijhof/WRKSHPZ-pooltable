# WRKSHPZ Making the pooltable smart

In this workshop we are going to make the pooltable smarter via sensors and Azure Computer Vision.
There is a Livestream of the pool table which can be used to retrieve images of the table and the sensors can give you data about hits and balls going into pockets.

The Livestream is a RTSP stream, running on `rtsp://192.168.1.124:8554/gopro`, you can test this via Vlc Media Player or via `ffplay rtsp://192.168.1.124:8554/gopro` after installing ffmpeg.

## Setup

To run this you don't have to install anything, but for some things you may need ffmpeg.

### Windows

``` powershell
choco install ffmpeg
```

### MacOS

```zsh
brew install ffmpeg
```

## Lets get started

### 1. Retrieve image from Livestream

We will start of by getting an image from the livestream. You can just take a print screen or we can do this by running one of the following scripts which will save an image to your disk.

#### .NET

Install the following nuget package:

``` xml
<ItemGroup>
    <PackageReference Include="Emgu.CV" Version="4.9.0.5494" />
    <PackageReference Include="Emgu.CV.runtime.windows" Version="4.9.0.5494" />
</ItemGroup>
```

Create an application with the following [code](./Scripts/dotnet.cs)

#### .NET (MacOS)

Install the following nuget package:

``` xml
<ItemGroup>
    <PackageReference Include="FFMpegCore" Version="5.1.0" />
    <PackageReference Include="SkiaSharp" Version="3.0.0-preview.5.4" />
</ItemGroup>
```

Create an application with the following [code](./Scripts/dotnet-macos.cs)

#### Python

To start of we can create a virtual environment and install opencv for python.

``` bash
python3 -m venv venv
source venv/bin/activate
pip install opencv-python
```

You can now copy [this](./Scripts/python.py) application and run it to see if the stream is working

### 2. Analyze an image via Azure Computer Vision

Now you can send the saved image to the Azure Computer Vision API and see if you can retrieve the objects in the image. See the examples on how to call the API.

> The example given in the [documentation](https://learn.microsoft.com/en-us/azure/ai-services/computer-vision/quickstarts-sdk/image-analysis-client-library-40?tabs=visual-studio%2Clinux&pivots=programming-language-csharp) does not work with a custom model. The examples are based on the calls of the `Try It Out` functionality in Azure Vision Studio.

After retrieving the data, be sure to filter the data so you will set a minimum level of confidence, for example 70 or 80%.

#### Example HttpClient

``` csharp
builder.Services.AddHttpClient<IImageAnalyzer, ImageAnalyzer>(cfg =>
{
    var key = builder.Configuration["AzureComputerVision:Key"];
    var endpoint = builder.Configuration["AzureComputerVision:Endpoint"];
    cfg.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", key);
    cfg.BaseAddress = new Uri(endpoint);
});
```

#### Example request

``` csharp
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
```

### 3. Retrieve sensor data

On the pool table there are sensors installed in the pockets and there are sensors attached to the Cue's. The data of the sensors are accessible via a socket.
Try to connect and see if you can retrieve some sensor data.

``` csharp
...
private WebsocketClient _client;

public PoolTafelSensor(string poolTableIp = "192.168.1.225", int poolTablePort = 81)
{
    var url = new Uri($"ws://{poolTableIp}:{poolTablePort}");

    _client = new WebsocketClient(url);
    _client.ReconnectTimeout = TimeSpan.FromSeconds(30);
    _client.ReconnectionHappened.Subscribe(info =>
        Console.WriteLine($"Reconnection happened, type: {info.Type}"));

    _client.MessageReceived.Subscribe(msg =>
    {
        if (msg.Text == null)
            return;

        var message = msg.Text;

        if (!message.Contains(','))
            return;

        if (message.StartsWith("hit"))
        {
            Console.WriteLine("Received a hit!");
        }

        if (message.StartsWith("pocket"))
        {
            Console.WriteLine($"Received ball in pocket");
        }
    });
    _client.Start();
}

public void Dispose()
{
    _client.Stop(WebSocketCloseStatus.NormalClosure, "Stop");
}
...
```

### 4. Make something cool!

Now you have the basics working, you can make something cool to elevate the Pool game to the next level.
As an example you can make a dashboard to show which balls are on the table or other player statistics.

#### Possible packages

- FFmpeg
- OpenCV
- EmguCV
