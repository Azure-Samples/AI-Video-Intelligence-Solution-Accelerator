using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf;
using Google.Protobuf.Collections;

namespace VideoProcessorModule
{
    class GpuModel : IProcessImage
    {
        public const string GpuModelProcessorType = "GPU";

        public string ProcessorType { get { return GpuModelProcessorType; } }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public List<ImageFeature> Process(ByteString image)
        {
            throw new NotImplementedException();
        }
    }
}
