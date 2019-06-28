using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace CameraModule
{
    public class ImageCrop
    {
        public float HorizontalOffsetPercent { get; }
        public float VerticalOffsetPercent { get; }
        public float WidthPercent { get; }
        public float HeightPercent { get; }

        public ImageCrop(JObject imageCrop)
        {
            if (imageCrop == null)
            {
                HorizontalOffsetPercent = 0;
                VerticalOffsetPercent = 0;
                WidthPercent = 100;
                HeightPercent = 100;
            }
            else
            {
                HorizontalOffsetPercent = (float)imageCrop["hOffset"];
                VerticalOffsetPercent = (float)imageCrop["vOffset"];
                WidthPercent = (float)imageCrop["width"];
                HeightPercent = (float)imageCrop["height"];
            }
        }
    }
}
