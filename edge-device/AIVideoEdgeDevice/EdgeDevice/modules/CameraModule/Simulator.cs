using BlobStorage;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VideoProcessorGrpc;

namespace CameraModule
{
    public class Simulator : IImageSource
    {
        private readonly static string[] simulatedImageNames;
        private readonly static Dictionary<int, List<int>> cycles = new Dictionary<int, List<int>>();

        /// <summary>
        /// For single images, the index into simulatedImageNames. For cycles, the position in the cycle.
        /// </summary>
        private int imageIndex = 0;
        private readonly int? cycleId = null;

        static Simulator()
        {
            simulatedImageNames = Directory.EnumerateFiles("simulated-images", "*.jpg").OrderBy(s => s).ToArray();
            Console.WriteLine($"Enumerating simulated image files:");
            int thisImageIndex = 0;
            foreach (string str in simulatedImageNames)
            {
                Console.WriteLine($"Found simulated image file: {str}");
                string baseImageName = Path.GetFileName(str);

                // If this image is part of an image cycle, add it to that image cycle record
                if (baseImageName.StartsWith("cycle-"))
                {
                    // e.g. cycle-0-0.png
                    string[] parts = baseImageName.Split('-');
                    int cycleIdx = int.Parse(parts[1]);
                    // Ensure a dictionary entry for this cycle of images
                    if (!cycles.TryGetValue(cycleIdx, out List<int> foundCycle))
                    {
                        foundCycle = new List<int>();
                        cycles.Add(cycleIdx, foundCycle);
                    }
                    foundCycle.Add(thisImageIndex);
                }
                thisImageIndex++;
            }
            foreach(int cycleId in cycles.Keys)
            {
                Console.WriteLine($"Found simulated image cycle: {cycleId},   cycle length: {cycles[cycleId].Count}");
                List<int> cycle = cycles[cycleId];
                foreach(int imageIdx in cycle)
                {
                    Console.WriteLine($"     {simulatedImageNames[imageIdx]}");
                }

            }
        }

        /// <summary>
        /// Construct a simulated image source
        /// </summary>
        /// <param name="hardwarePort"></param>
        public Simulator(string hardwarePort)
        {
            // e.g. cycle-0-0.jpg or cycle-0
            string[] parts = hardwarePort.Split('-');
            if (hardwarePort.StartsWith("cycle-") && parts.Length == 2)
            {
                // This is the name of a cycle
                try
                {
                    this.cycleId = int.Parse(parts[1]);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Could not parse simulated camera port {hardwarePort}: {ex.Message}");
                }
            }
            else
            {
                // This is a specific filename
                for (int i = 0; i < simulatedImageNames.Length; i++)
                {
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(simulatedImageNames[i]);
                    if (fileNameWithoutExtension == hardwarePort)
                    {
                        imageIndex = i;
                        return;
                    }
                }
                throw new ApplicationException("Camera simulated hardware port name not found in simulated images");
            }
        }

        /// <summary>
        /// Retrieve images from a camera.
        /// </summary>
        /// <returns>The large and small images</returns>
        byte[] IImageSource.RequestImage()
        {
            // If we're not cycling then this.imageIndex is what we want. Otherwise
            // we need to cycle.
            int simulatedImageIndex = this.imageIndex;
            if (cycleId != null)
            {
                List<int> selectedCycle = cycles[cycleId.Value];
                simulatedImageIndex = selectedCycle[this.imageIndex];
                this.imageIndex++;
                this.imageIndex = this.imageIndex % selectedCycle.Count;
            }
            string filename = simulatedImageNames[simulatedImageIndex];
            byte[] content = File.ReadAllBytes(filename);

            return content;
        }

        void IImageSource.Disconnect()
        {
        }
    }
}
