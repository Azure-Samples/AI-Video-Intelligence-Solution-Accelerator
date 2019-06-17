using System;
using System.Collections.Generic;
using System.Text;
using VideoProcessorGrpc;

namespace VideoProcessorModule
{
    /// <summary>
    /// This first implementation delivers the most recently added ImageBody
    /// in round-robin fashion, doing one camera after another.
    /// TODO: Choose by oldest age instead of round-robin
    /// </summary>
    public class InputBuffer
    {
        private class BufferEntry
        {
            public string CameraId { get; set; }
            public ImageBody Body { get; set; }
        }
        private readonly static Object s_bufferLock = new Object();
        private readonly static List<BufferEntry> s_entryList = new List<BufferEntry>();
        private readonly static Dictionary<string, BufferEntry> s_entryDictionary = new Dictionary<string, BufferEntry>();
        private static int s_entryListIndex = 0;

        /// <summary>
        /// Add a received image to the processing buffer
        /// </summary>
        /// <param name="body"></param>
        public static void Add(ImageBody body)
        {
            lock (s_bufferLock)
            {
                s_entryDictionary.TryGetValue(body.CameraId, out BufferEntry found);
                if (found == null)
                {
                    BufferEntry entry = new BufferEntry() { CameraId = body.CameraId, Body = body };
                    s_entryDictionary.Add(body.CameraId, entry);
                    s_entryList.Add(entry);
                    found = entry;
                }
                // Keep only the latest Body for processing, discarding any unprocessed older ones
                found.Body = body;
            }
        }

        /// <summary>
        /// Return the next image to process
        /// </summary>
        /// <returns>May be null</returns>
        public static ImageBody GetNext()
        {
            lock(s_bufferLock)
            {
                // Walk the list looking for a non-null ImageBody to process
                for (int i = 0; i < s_entryList.Count; i++)
                {
                    // s_entryListIndex is left pointing at the last camera whose image was processed
                    s_entryListIndex++;
                    s_entryListIndex = (int)(s_entryListIndex % s_entryList.Count);
                    if (s_entryList[s_entryListIndex].Body != null)
                    {
                        ImageBody result = s_entryList[s_entryListIndex].Body;
                        s_entryList[s_entryListIndex].Body = null;
                        return result;
                    }
                }

                return null;
            }
        }
    }
}
