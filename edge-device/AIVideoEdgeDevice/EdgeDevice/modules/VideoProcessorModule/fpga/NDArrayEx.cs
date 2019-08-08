using NumSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace FpgaClient
{
    public static class NDArrayEx
    {
        public static NDArray FromTensorProto(Tensorflow.TensorProto tensor)
        {
            List<int> dims = new List<int>();
            foreach (var dim in tensor.TensorShape.Dim)
            {
                dims.Add((int)dim.Size);
            }
            float[] floats = new float[tensor.FloatVal.Count];
            tensor.FloatVal.CopyTo(floats, 0);
            NDArray result = new NDArray(floats, dims.ToArray());
            return result;
        }
    }
}
