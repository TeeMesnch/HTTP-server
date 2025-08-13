using System.Net;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace Server
{
    class MainClass
    {
        static void Main(string[] args)
        {
            Console.WriteLine("\nstarting Server\n");
            
            TcpListener server = new TcpListener(IPAddress.Any, 4221);
            server.Start();

            bool running = true;

            while (running)
            {
                var response = Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\r\n\r\n");
                var notFound = Encoding.UTF8.GetBytes("HTTP/1.1 404 Not Found\r\n\r\n");

                using TcpClient client = server.AcceptTcpClient();
                var stream = client.GetStream();
                var reader = new StreamReader(stream, Encoding.UTF8);

                byte[] buffer = new byte[1024];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                var lines = request.Split("\r\n");
                var requestLine = lines[0].Split(' ');
                var url = requestLine[1];

                if (url == "/")
                {
                    stream.Write(response);
                    Console.WriteLine("connection made");
                }
                else if (url.StartsWith("/echo/"))
                {
                    var prefix = "/echo/".Length;
                    var bodyStr = url.Substring(prefix);

                    var body = Encoding.UTF8.GetBytes($"{bodyStr}\r\n\r\n");
                    var header = Encoding.UTF8.GetBytes(
                        $"HTTP/1.1 200 OK\r\n\r\nContent-Type: text/plain\r\n\r\nContent-Length: {body.Length}\r\n\r\n");

                    stream.Write(header);
                    stream.Write(body);
                    Console.WriteLine($"returning {body}");
                }
                else if (url.StartsWith("/user-agent"))
                {
                    var line = request;
                    var userAgent = line;

                    if (line.Contains("User-Agent:"))
                    {
                        userAgent = line.Replace("Accept: */*", "");
                        userAgent = userAgent.Substring(46);
                    }

                    var header = Encoding.UTF8.GetBytes($"HTTP/1.1 200 OK\r\n\r\nContent-Type: text/plain\r\n\r\n");
                    var body = Encoding.UTF8.GetBytes($"{userAgent}\r\n\r\n");

                    stream.Write(header);
                    stream.Write(body);
                }
                else if (url.StartsWith($"/file/"))
                {
                    //file.exists or something later
                    var prefix = "/file/".Length;
                    var file = url.Substring(prefix);
                    
                    Console.WriteLine(file);
                    
                    var header = Encoding.UTF8.GetBytes($"HTTP/1.1 200 OK\r\n\r\nContent-Type: application/octet-stream\r\n\r\nContent-Length: {file.Length}\r\n\r\n");
                    var body = Encoding.UTF8.GetBytes($"{file}\r\n\r\n");
                    
                    stream.Write(header);
                    stream.Write(body);
                }
                else
                {
                    Console.WriteLine("not found 404");
                    stream.Write(notFound);
                }

            }
            
            server.Stop();
        }
    }
}