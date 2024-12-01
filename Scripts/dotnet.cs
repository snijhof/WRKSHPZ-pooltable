// See https://aka.ms/new-console-template for more information
using Emgu.CV;

var rtspUrl = "rtsp://192.168.1.124:8554/gopro";

// Create a VideoCapture object
using (VideoCapture capture = new VideoCapture(rtspUrl, VideoCapture.API.Ffmpeg))
{
    if (!capture.IsOpened)
    {
        Console.WriteLine("Error: Could not open RTSP stream.");
        return;
    }

    // Create a directory to save the frames
    string outputDir = $"saved_frames_{Guid.NewGuid()}";
    if (!Directory.Exists(outputDir))
    {
        Directory.CreateDirectory(outputDir);
    }

    int frameCount = 0;
    int savedFrameCount = 0;

    using (Mat frame = new Mat())
    {
        while (true)
        {
            capture.Read(frame);
            if (frame.IsEmpty)
            {
                Console.WriteLine("Failed to grab frame");
                break;
            }

            // Display the frame
            CvInvoke.Imshow("RTSP Stream", frame);

            // Save every 30th frame to disk
            if (frameCount % 30 == 0)
            {
                string frameFilename = Path.Combine(outputDir, $"frame_{savedFrameCount}.jpg");
                frame.Save(frameFilename);
                Console.WriteLine($"Saved {frameFilename}");
                savedFrameCount += 1;
            }

            frameCount++;

            // Press 'q' to exit the loop
            if (CvInvoke.WaitKey(1) == 'q')
            {
                break;
            }
        }
    }

    // Release the VideoCapture object
    capture.Release();
    CvInvoke.DestroyAllWindows();
}