using System.Net;
using System.IO.Compression;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    class MainClass
    {
        static void Main(string[] args)
        {
            string newDirectory = "/Users/jonathan/desktop";
            Directory.SetCurrentDirectory(newDirectory);
            Console.WriteLine($"\nCurrent Directory: {Directory.GetCurrentDirectory()}\n");

            const int port = 4221;
            var ip = IPAddress.Loopback;
            
            TcpListener server = new TcpListener(ip, port);
            server.Start();
            
            Console.WriteLine($"starting server (port: {port}) (ip: {ip})\n");

            bool running = true;

            while (running)
            {
                var response = Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\r\n\r\n");
                var created = Encoding.UTF8.GetBytes("HTTP/1.1 201 Created\r\n\r\n");
                var notFound = Encoding.UTF8.GetBytes("HTTP/1.1 404 Not Found\r\n\r\n");

                using TcpClient client = server.AcceptTcpClient();
                var stream = client.GetStream();
                var reader = new StreamReader(stream, Encoding.UTF8);

                var url = "";
                string request = "";
                
                try
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    var lines = request.Split("\r\n");
                    var requestLine = lines[0].Split(' ');
                    url = requestLine[1];
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                
                if (url == "/")
                {
                    stream.Write(response);
                    Console.WriteLine("connection made");
                }
                else if (url.StartsWith("/echo/"))
                {
                    var prefix = "/echo/".Length;
                    var bodyStr = url.Substring(prefix);
                    
                    var body = Encoding.UTF8.GetBytes($"{bodyStr}");
                    var header = Encoding.UTF8.GetBytes($"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {bodyStr.Length}\r\n\r\n");
                    
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

                        try
                        {
                            long fileSize = new FileInfo("compressedFile.gz").Length;

                            var gzip = Encoding.UTF8.GetBytes($"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {fileSize}\r\nContent-Encoding: gzip\r\n\r\n");

                            stream.Write(gzip);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                    else if (!request.Contains("gzip"))
                    {
                        Console.WriteLine("request wont be compressed"); 
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

                    var header = Encoding.UTF8.GetBytes($"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\n\r\n");
                    var body = Encoding.UTF8.GetBytes($"{userAgent}");

                    Console.WriteLine("user-agent requested");
                    
                    stream.Write(header);
                    stream.Write(body);
                }
                else if (url.StartsWith($"/file/"))
                {
                    var prefix = "/file/".Length;
                    var file = url.Substring(prefix);
                    
                    if (File.Exists(file))
                    {
                        try
                        {
                            Console.WriteLine($"file requested: {file}");
                            FileInfo fileInfo = new FileInfo(file);
                            long lenght = fileInfo.Length;
                            file = File.ReadAllText(file);


                            var header = Encoding.UTF8.GetBytes(
                                $"HTTP/1.1 200 OK\r\nContent-Type: application/octet-stream\r\nContent-Length: {lenght}\r\n\r\n");
                            var body = Encoding.UTF8.GetBytes($"{file}");

                            stream.Write(header);
                            stream.Write(body);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                    else if (!File.Exists(file))
                    {
                        Console.WriteLine($"file not found: {file}");
                        try
                        {
                            if (request.Contains("POST"))
                            {
                                var split = request.Split("\r\n\r\n");
                                Console.WriteLine($"creating new file (name: {file})");
                                File.WriteAllText(file, split[1]);

                                stream.Write(created);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
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