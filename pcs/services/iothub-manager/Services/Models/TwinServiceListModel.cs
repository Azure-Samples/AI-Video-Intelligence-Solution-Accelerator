// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;

namespace Microsoft.Azure.IoTSolutions.IotHubManager.Services.Models
{
    public class TwinServiceListModel
    {
        public string ContinuationToken { get; set; }

        public List<TwinServiceModel> Items { get; set; }

        public TwinServiceListModel(IEnumerable<TwinServiceModel> twins, string continuationToken = null)
        {
            this.ContinuationToken = continuationToken;
            this.Items = new List<TwinServiceModel>(twins);
        }
    }
}