using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;

namespace CameraSim
{
    public class CameraSimulator
    {
        const int width = 640;
        const int height = 480;
        readonly Rectangle background = new Rectangle(0, 0, width, height);
        readonly Rectangle topLine = new Rectangle(20, 40, 600, 40);
        readonly Rectangle secondLine = new Rectangle(20, 80, 600, 40);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cameraId">An AI Video camera semantic ID, e.g. bldg52room2117/grid01x04look27</param>
        public CameraSimulator(string cameraId)
        {
            CameraId = cameraId;
        }

        public string CameraId { get; private set; }

        public byte[] GetSimulatedImage(string line1)
        {
            byte[] result = new byte[0];
            using (Bitmap image = new Bitmap(width, height))
            using (MemoryStream stream = new MemoryStream())
            {
                try
                {
                    Font font = new Font("Tahoma", 16);
                    Graphics g = Graphics.FromImage(image);
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    g.FillRectangle(Brushes.Wheat, background);
                    g.DrawString(CameraId, font, Brushes.Black, topLine);
                    g.DrawString(line1, font, Brushes.Black, secondLine);

                    image.Save(stream, ImageFormat.Jpeg);
                    stream.Seek(0, SeekOrigin.Begin);
                    result = new byte[stream.Length];
                    int size = stream.Read(result, 0, (int)stream.Length);
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Failed to generate image: {ex.Message}");
                }
            }
            return result;
        }
    }
}
