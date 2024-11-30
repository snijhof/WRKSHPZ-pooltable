using FFMpegCore;
using FFMpegCore.Pipes;
using SkiaSharp;

class Program
{
    static async Task Main(string[] args)
    {
        var ffmpegFrameCapturer = FFMpegArguments
            .FromUrlInput(new Uri("rtsp://localhost:8554/stream"))
            .OutputToPipe(new StreamPipeSink(async (stream, token) =>
                {
                    await ProcessStreamFrames(stream);
                }), options => options
                     .WithVideoCodec("mjpeg")
                     .WithFramerate(10)
                     .ForceFormat("mjpeg")
            );

        ffmpegFrameCapturer.ProcessSynchronously();
    }

    static async Task ProcessStreamFrames(Stream stream)
    {
        var ms = new MemoryStream();
        byte[] buffer = new byte[4096];
        int bytesRead;
        int frameCounter = 0;

        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            ms.Write(buffer, 0, bytesRead);

            // Check if a complete JPEG frame is in memory by looking for the JPEG end marker (0xFFD9)
            if (bytesRead > 1 && buffer[bytesRead - 2] == 0xFF && buffer[bytesRead - 1] == 0xD9)
            {
                // We found a complete image in ms
                ms.Seek(0, SeekOrigin.Begin);

                var frameData = ms.ToArray();
                frameCounter++;

                // Save every 30th frame to disk
                if (frameCounter % 30 == 0)
                {
                    SaveFrameToFile(frameData);
                }

                // Clear the MemoryStream for the next frame
                ms.SetLength(0);
            }
        }
    }

    static void SaveFrameToFile(byte[] frameData)
    {
        string filePath = $"frame_{DateTime.Now:yyyyMMdd_HHmmssfff}.jpg";

        using (var ms = new MemoryStream(frameData))
        using (var skStream = new SKManagedStream(ms))
        using (var codec = SKCodec.Create(skStream))
        {
            if (codec != null)
            {
                var info = codec.Info;
                using (var bitmap = new SKBitmap(info.Width, info.Height))
                {
                    if (codec.GetPixels(bitmap.Info, bitmap.GetPixels()) == SKCodecResult.Success)
                    {
                        // Encode the SKBitmap to JPEG format and save it to the file
                        using (var image = SKImage.FromBitmap(bitmap))
                        using (var data = image.Encode(SKEncodedImageFormat.Jpeg, 100))  // 100 for maximum quality
                        using (var fileStream = File.OpenWrite(filePath))
                        {
                            data.SaveTo(fileStream);
                        }
                        Console.WriteLine($"Saved frame to {filePath}");
                    }
                    else
                    {
                        Console.WriteLine("Failed to decode JPEG frame.");
                    }
                }
            }
            else
            {
                Console.WriteLine("SKCodec.Create returned null, indicating an invalid frame.");
            }
        }
    }
}