// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;

namespace CameraModule
{
    public interface IImageSource
    {
        /// <summary>
        /// Return an ImageBody with the jpeg image filled in
        /// </summary>
        /// <returns></returns>
        byte[] RequestImage();

        void Disconnect();
    }
}
