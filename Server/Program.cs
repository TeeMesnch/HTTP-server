using System.Net;
using System.IO;
using System.IO.Compression;
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
            string newDirectory = "/Users/jonathan/desktop";
            Directory.SetCurrentDirectory(newDirectory);
            Console.WriteLine($"Current Directory: {Directory.GetCurrentDirectory()}\n");
            
            TcpListener server = new TcpListener(IPAddress.Any, 4221);
            server.Start();

            bool running = true;

            while (running)
            {
                var response = Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\r\n\r\n");
                var created = Encoding.UTF8.GetBytes("HTTP/1.1 201 Created\r\n\r\n");
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
                    var header = Encoding.UTF8.GetBytes($"HTTP/1.1 200 OK\r\n\r\nContent-Type: text/plain\r\n\r\nContent-Length: {bodyStr.Length}\r\n\r\n");
                    
                    if (request.Contains("gzip"))
                    {
                        var fileName = url.Substring(prefix);
                        var inputFile = fileName;
                        var outputFile = "compressedFile.gz";
                        
                        try
                        {
                            using FileStream fs = File.Open(inputFile, FileMode.Open);
                            using FileStream cfs = File.Create(outputFile);
                            using GZipStream gs = new GZipStream(cfs, CompressionMode.Compress);
                            {
                                fs.CopyTo(gs);
                                Console.WriteLine($"compressing File (name: {outputFile})");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }

                        long fileSize = new FileInfo("compressedFile.gz").Length;
                        
                        var gzip = Encoding.UTF8.GetBytes($"HTTP/1.1 200 OK\r\n\r\nContent-Type: text/plain\r\n\r\nContent-Length: {fileSize}\r\n\r\nContent-Encoding: gzip\r\n\r\n");
                        
                        stream.Write(gzip);
                    }
                    else if (!request.Contains("gzip"))
                    {
                        stream.Write(header);
                        stream.Write(body);
                    }
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
                    var prefix = "/file/".Length;
                    var file = url.Substring(prefix);
                    
                    if (File.Exists(file))
                    {
                        Console.WriteLine($"file requested: {file}");
                        FileInfo fileInfo = new FileInfo(file);
                        long lenght = fileInfo.Length;
                        file = File.ReadAllText(file);
                        
                        var header = Encoding.UTF8.GetBytes($"HTTP/1.1 200 OK\r\n\r\nContent-Type: application/octet-stream\r\n\r\nContent-Length: {lenght}\r\n\r\n");
                        var body = Encoding.UTF8.GetBytes($"{file}\r\n\r\n");
                        
                        stream.Write(header);
                        stream.Write(body);
                    }
                    else if (!File.Exists(file))
                    {
                        Console.WriteLine($"file not found: {file}");
                        if (request.Contains("POST"))
                        {
                            var split = request.Split("\r\n\r\n");
                            Console.WriteLine($"creating new file (name: {file})");
                            File.WriteAllText(file, split[1]);
                            
                            stream.Write(created);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"file not found: {file}");
                        stream.Write(notFound);
                    }
                }
                else
                {
                    Console.WriteLine("not found 404");
                    stream.Write(notFound);
                }

            }
        }
    }
}