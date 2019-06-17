// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;

namespace CameraModule
{
    public interface IImageSource
    {
        /// <summary>
        /// Return an ImageBody with the two images filled in
        /// </summary>
        /// <returns></returns>
        VideoProcessorGrpc.ImageBody RequestImages();

        void Disconnect();
    }
}
