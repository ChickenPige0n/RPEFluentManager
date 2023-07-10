using Newtonsoft.Json;
using RPEFluentManager.ViewModels;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using static RPEFluentManager.ViewModels.DashboardViewModel;

namespace RPEFluentManager.Models
{
    public class ChartFileServer
    {
        private readonly DashboardViewModel.ChartData _chartData;
        private bool _isRunning;
        private HttpListener _listener;
        private string _resourcesPath;
    
        public ChartFileServer(DashboardViewModel.ChartData chartData, int port)
        {
            _chartData = chartData;
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://+:{port}/");
            _resourcesPath = SettingsHandler.GetSettings().ResourcePath;
        }

        public void Start()
        {
            if (_isRunning)
            {
                return;
            }
            _listener.Start();
            _isRunning = true;
            ThreadPool.QueueUserWorkItem(Listen);
        }
    
        public void Stop()
        {
            if (!_isRunning)
            {
                return;
            }
            _listener?.Stop();
            _listener?.Close();
            _isRunning = false;
        }
    
        private void Listen(object state)
        {
            while (_isRunning)
            {
                try
                {
                    var context = _listener.GetContext();
                    ThreadPool.QueueUserWorkItem(ProcessRequest, context);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error occurred while processing request: {ex.Message}");
                }
            }
        }
    
        private void ProcessRequest(object state)
        {
            var context = (HttpListenerContext)state;
            var request = context.Request;

            ChartEditViewModel.PackPEZ(false, _chartData);



            var filePath = Path.Combine(_resourcesPath, _chartData.ChartPath+".pez"); // 指定要发送的文件路径

            StreamWriter sw = new StreamWriter("F:\\Program Files (x86)\\RPEv1.2\\Resources\\111.txt", false, new UTF8Encoding(false));
            sw.Write(filePath);
            sw.Close();


            try
            {
                if (request.HttpMethod.ToUpper() == "GET")
                {
                    if (File.Exists(filePath))
                    {
                        var response = context.Response;
                        response.ContentType = "text/plain";
                        response.Headers.Add("Access-Control-Allow-Origin", "*");


                        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        {
                            fileStream.CopyTo(response.OutputStream);
                        }
    
                        response.Close();
                    }
                    else
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        context.Response.Close();
                    }
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                    context.Response.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while processing request: {ex.Message}");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.Close();
            }
        }
    }
}