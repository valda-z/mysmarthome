using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SmartHomeCore
{
    public class MyHttpServerControler
    {
        private Thread _serverThread;
        private HttpListener _listener;
        private int _port;

        public int Port
        {
            get { return _port; }
            private set { }
        }

        /// <summary>
        /// Construct server with given port.
        /// </summary>
        /// <param name="port">Port of the server.</param>
        public MyHttpServerControler(int port)
        {
            this.Initialize(port);
        }

        /// <summary>
        /// Stop server and dispose all functions.
        /// </summary>
        public void Stop()
        {
            _serverThread.Abort();
            _listener.Stop();
        }

        private void Listen()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://+:" + _port.ToString() + "/");
            _listener.Start();
            Console.WriteLine("HTTP server starting on port: " + _port.ToString());
            while (true)
            {
                try
                {
                    HttpListenerContext context = _listener.GetContext();
                    Process(context);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.StackTrace);
                }
            }
        }

        private void Process(HttpListenerContext context)
        {
            string filename = context.Request.Url.AbsolutePath;
            Console.WriteLine("HTTP server received: " + filename);

            context.Response.ContentType = "text/html";
            context.Response.ContentEncoding = Encoding.UTF8;

            switch (filename)
            {
                case "/":
                case "/heating.api":
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    using (var sw = new StreamWriter(context.Response.OutputStream))
                    {
                        int resp = 0;
                        if (StaticJablotronState.IsMovementThere)
                        {
                            resp += 1;
                        }
                        sw.WriteLine(resp.ToString());
                        Console.WriteLine(" >>http-req: {0}  => {1}", filename, resp);
                    }
                    break;
                case "/outunit.api":
                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    using (var sw = new StreamWriter(context.Response.OutputStream))
                    {
                        int resp = 0;
                        if (StaticJablotronState.IsMovementThere)
                        {
                            resp += 1;
                        }
                        sw.WriteLine(resp.ToString());
                        Console.WriteLine(" >>http-req: {0}  => {1}", filename, resp);
                    }
                    break;
                default:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
            }

            try
            {
                context.Response.OutputStream.Close();
            }
            catch { }
        }

        private void Initialize(int port)
        {
            this._port = port;
            _serverThread = new Thread(this.Listen);
            _serverThread.Start();
        }

    }
}
