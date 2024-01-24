using OpenCvSharp;
using System.Net.Http.Headers;
using System.Runtime.Intrinsics.X86;

string videoUrl = "https://192.168.0.72:8080/video";

VideoCapture get_camera(string videoUrl)
{
    var cam = VideoCapture.FromFile(videoUrl);

    if (!cam.IsOpened())
    {
        return null;
    }
    cam.Set(VideoCaptureProperties.FrameHeight, 1080);
    cam.Set(VideoCaptureProperties.FrameWidth, 1920);
    cam.Set(VideoCaptureProperties.Exposure, 3);
    cam.Set(VideoCaptureProperties.Fps, 100);

    return cam;
}
Mat blur(Mat src)
{
    var result = new Mat();
    Cv2.GaussianBlur(src, result, new Size(3, 3), 1);
    return result;
}

Mat find_by_mask(Mat src)
{
    Mat hsv = new Mat();
    Cv2.CvtColor(src, hsv, ColorConversionCodes.BGR2HSV);

    Mat mask_color = new Mat();

    Cv2.InRange(hsv, new Scalar(110, 20, 35), new Scalar(180, 255, 255), mask_color);

    Mat masked = new Mat();

    Cv2.BitwiseAnd(hsv, hsv, masked, mask_color);

    return masked;
}

Point[][] find_cont(Mat src)
{
    var result = new Mat(); Cv2.CvtColor(src, result, ColorConversionCodes.BGR2GRAY);
    Cv2.Canny(result, result, 40, 150);

    var cont = new Point [1000][];
    var hierarchy = new HierarchyIndex[1000];

    Cv2.FindContours(result, out cont, out hierarchy, RetrievalModes.List, ContourApproximationModes.ApproxSimple);

    return cont;
}

Mat find_rectangles(Mat src, Point[] [] contours)
{
    int j = 0;
    foreach(var i in contours)
    {
        var approx = Cv2.ApproxPolyDP(i, 0.01 * Cv2.ArcLength(i, true), true);

        if(approx.Length == 4)
        {
            var x = Cv2.BoundingRect(i).X; var y = Cv2.BoundingRect(i).Y; var w = Cv2.BoundingRect(i).Width; var h = Cv2.BoundingRect(i).Height;
            if (w > 10)
                Cv2.DrawContours(src, contours, j, new Scalar(255, 255, 255), 2);
        }
        j++;
    }
    return src;
}

var cum = get_camera(videoUrl);

if(cum == null)
{
    Console.WriteLine("Idi naxui cameri net");
}

else
{
    while (true)
    {
        Mat frame = new Mat(); bool ret = cum.Read(frame);
        if (ret)
        {
            Cv2.Resize(frame,frame, new Size(frame.Width / 2, frame.Height / 2));

            Mat frame1 = blur(frame);
            frame1 = find_by_mask(frame1);
            var cont = find_cont(frame1);
            frame = find_rectangles(frame, cont);
        }

        Cv2.ImShow("Stream", frame);

        if (Cv2.WaitKey(1) != -1)
            break;
    }

    cum.Release();
    Cv2.DestroyAllWindows();
}