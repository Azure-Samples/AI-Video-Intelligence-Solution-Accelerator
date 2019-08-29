using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CameraServer
{
    public class Server
    {
        static HttpListener listener;
        static Task runner;
        static bool isDone = false;

        public static int Port { get { return 8080; } }
        public static void Start()
        {
            listener = new HttpListener();
            listener.Prefixes.Add($"http://*:{Port}/image/");
            listener.Start();
            runner = new Task(Run);
            runner.Start();
        }

        private static void ReturnNotFoundError(HttpListenerResponse response, string e)
        {
            response.StatusDescription = e;
            response.StatusCode = (int)HttpStatusCode.NotFound;
            response.OutputStream.Close();
        }

        private static void Run()
        {
            while(!isDone)
            {
                try
                {
                    HttpListenerContext context = listener.GetContext();
                    HttpListenerRequest request = context.Request;
                    // Obtain a response object.
                    string[] parts = request.RawUrl.Split('/');
                    if (parts.Length != 3 || parts[1] != "image")
                    {
                        ReturnNotFoundError(context.Response, "Bad url. Good urls look like 'http:servername:8080/image/700'");
                    }
                    else
                    {
                        var (image, errOut) = Program.OnImageRequestReceived(parts[2]);
                        if (errOut == null)
                        {
                            HttpListenerResponse response = context.Response;
                            response.ContentType = "image/jpeg";
                            response.ContentLength64 = image.Length;
                            response.OutputStream.Write(image, 0, image.Length);
                            response.StatusCode = (int)HttpStatusCode.OK;
                            response.OutputStream.Close();
                        }
                        else
                        {
                            ReturnNotFoundError(context.Response, errOut);
                        }
                    }

                }
                catch(Exception)
                {
                    // GetContext throws when stopping, ignore
                }
            }
        }

        public static void Stop()
        {
            isDone = true;
            listener.Stop();
        }
    }
}
