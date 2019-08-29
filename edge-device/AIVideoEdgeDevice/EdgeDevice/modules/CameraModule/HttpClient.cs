using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CameraModule
{
    class HttpClient : IImageSource
    {
        System.Net.Http.HttpClient httpClient;
        string url;
        public HttpClient(string url)
        {
            httpClient = new System.Net.Http.HttpClient();
            this.url = url;
        }

        public void Disconnect()
        {
        }

        public byte[] RequestImage()
        {
            try
            {
                var task = httpClient.GetAsync(url);
                task.Wait();
                task.Result.EnsureSuccessStatusCode();
                var responseBody = task.Result.Content.ReadAsByteArrayAsync();
                responseBody.Wait();
                return responseBody.Result;
            }
            catch (System.Net.Http.HttpRequestException e)
            {
                Console.WriteLine("Error in HttpClient.RequestImage: {0} ", e.Message);
                return null;
            }
        }
    }
}
