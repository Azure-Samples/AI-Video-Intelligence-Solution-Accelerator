// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Tensorflow.Serving;

namespace FpgaClient
{
    public interface IScoringRequest
    {
        PredictRequest MakePredictRequest();
    }
}