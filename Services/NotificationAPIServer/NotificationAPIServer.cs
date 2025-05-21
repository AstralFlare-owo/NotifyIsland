
using System.IO;
using System.Net;
using System.Text.Json;

namespace cn.lixiaotuan.notifyisland.Services.NotificationAPIServer;

public class Notification
{
    public string title { get; set; }
    public int title_duration { get; set; }
    public string content { get; set; }
    public int content_duration { get; set; }
    public string title_voice { get; set; }
    public string content_voice  { get; set; }
}

public class NotificationReceivedEventArgs : EventArgs
{
    public Notification notification { get; set; }
}

public class NotificationAPIServer: IDisposable
{
    public static NotificationAPIServer? Current { get; set; }
    public event EventHandler<NotificationReceivedEventArgs>? NotificationReceived;
    private readonly HttpListener _listener;
    private readonly string _url;
    private readonly string _token;

    public NotificationAPIServer(string url = "http://0.0.0.0:1379/", string token = "")
    {
        _url = url.Replace("0.0.0.0","*").Replace("[::]","*");
        _token = token;
        _listener = new HttpListener();
        _listener.Prefixes.Add(_url);
        Start();
    }

    public void Start()
    {
        _listener.Start();
        ListenAsync();
    }

    public void Stop()
    {
        _listener.Stop();
    }

    private async void ListenAsync()
    { 
        while (_listener.IsListening)
        {
            HttpListenerContext Context;
            try
            {
                Context = await _listener.GetContextAsync();
            } catch { continue; }
            var Request = Context.Request;
            var Response = Context.Response;
            Response.ContentType = "application/json";
            Response.ContentEncoding = System.Text.Encoding.UTF8;
            if (!(Request.Url.LocalPath == "/api/notify"))
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;
                await Response.OutputStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes("{\"success\":false,\"status\":-404,\"message\":\"[-404]API端点不存在\"}"));
                Response.Close();
                continue;
            }
            if (!(Request.HttpMethod == "POST"))
            {
                Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                await Response.OutputStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes("{\"success\":false,\"status\":-405,\"message\":\"[-405]API端点仅允许POST请求\"}"));
                Response.Close();
                continue;
            }
            if (!(Request.ContentType == "application/json"||Request.ContentType == "text/json"||Request.ContentType =="raw"||Request.ContentType=="text/plain"))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await Response.OutputStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes("{\"success\":false,\"status\":-400,\"message\":\"[-400]传入内容应为JSON格式\"}"));
                Response.Close();
                continue;
            }
            if (!String.IsNullOrEmpty(_token))
            {
                if (String.IsNullOrEmpty(Request.Headers.Get("Authorization")))
                {
                    Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    await Response.OutputStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes("{\"success\":false,\"status\":-401,\"message\":\"[-401]API端点需要认证\"}"));
                    Response.Close();
                    continue;
                }
                if (!Request.Headers.Get("Authorization").StartsWith("Bearer ") || Request.Headers.Get("Authorization").Substring(7) != _token)
                {
                    Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    await Response.OutputStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes("{\"success\":false,\"status\":-403,\"message\":\"[-403]API端点认证失败\"}"));
                    Response.Close();
                    continue;
                }
            }

            using (var Reader = new StreamReader(Request.InputStream, Request.ContentEncoding))
            {
                Notification? notification;
                try
                {
                    notification = JsonSerializer.Deserialize<Notification>(await Reader.ReadToEndAsync());
                }
                catch
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await Response.OutputStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes("{\"success\":false,\"status\":-400,\"message\":\"[-400]传入内容应为JSON格式\"}"));
                    Response.Close();
                    continue;
                }
                NotificationReceived?.Invoke(this, new NotificationReceivedEventArgs() { notification = notification });
                Response.StatusCode = (int)HttpStatusCode.OK;
                await Response.OutputStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes("{\"success\":true,\"status\":200,\"message\":\"[200]已推送到ClassIsland\"}"));
                Response.Close();
            }
        }
    }

    public void Dispose()
    {
        if (_listener.IsListening)
        {
            _listener.Stop();
        }
        _listener.Close();
        Current = null;
    }
}