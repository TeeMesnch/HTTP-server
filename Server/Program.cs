using System.Net;
using System.Net.Sockets;
using System.IO.Compression;
using System.Text;

namespace Server
{
    class Server
    {
        static void Main(string[] args)
        {
            // TESTING DIRECTORY
            string newDirectory = "/Users/jonathan/desktop/Server";
            Directory.SetCurrentDirectory(newDirectory);
            Console.WriteLine($"changed directory (directory: {newDirectory})\n");
            
            RunServer();
        }

        static private List<TcpClient> TCPclient = new List<TcpClient>();
        
        static void RunServer()
        {
            const int port = 4200;
            var ip = IPAddress.Parse("127.0.0.1");
            
            //Random random = new Random();
            //int id = random.Next();

            int id = 1234;
            
            TcpListener server = new TcpListener(ip, port);
            server.Start();
            
            Console.WriteLine($"starting server (port: {port}) (ip: {ip})\n");
            Console.WriteLine($"(id: {id})\n");

            while (true)
            {
                using TcpClient client = server.AcceptTcpClient();
                var stream = client.GetStream();
                
                lock (TCPclient)
                {
                    TCPclient.Add(client);
                }

                string url = "";
                string request = "";

                try
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    var lines =  request.Split("\r\n");
                    var requestLine = lines[0].Split(" ");
                    url = requestLine[1];
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine("Error parsing request");
                }

                if (url == "/")
                {
                    ServeIndex();
                }
                else if (url.StartsWith("/echo/"))
                {
                    Echo(url);
                }
                else if (url.StartsWith("/file/"))
                {
                    FileRequest(url, request, id);
                }
            }
        }

        static void ServeIndex()
        {
            var indexHtml = "index.html";
            var indexCss = "index.css";
            long htmlSize = 0;
            long cssSize = 0;
            
            try
            {
                FileInfo fileInfo = new FileInfo(indexHtml);
                htmlSize = fileInfo.Length;
                indexHtml = File.ReadAllText(indexHtml);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Error reading index.html");
            }

            try
            {
                FileInfo fileInfo = new FileInfo(indexCss);
                cssSize = fileInfo.Length;
                indexCss = File.ReadAllText(indexCss);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Error reading index.css");
            }
            
            var htmlHeader =Encoding.UTF8.GetBytes($"HTTP/1.1 200 OK \r\nContent-Type: text/html; charset=utf-8\r\nContent-Length: {htmlSize}\r\n\r\n");
            var htmlBody = Encoding.UTF8.GetBytes(indexHtml);
            
            var cssHeader = Encoding.UTF8.GetBytes($"HTTP/1.1 200 OK \r\nContent-Type: text/css; charset=utf-8\r\nContent-Length: {cssSize}\r\n\r\n");
            var cssBody = Encoding.UTF8.GetBytes(indexCss);
            
            SendData(htmlHeader, htmlBody);
            SendData(cssHeader, cssBody);
        }

        static void Echo(string url)
        {
            var prefix = "/echo/".Length; 
            var bodyStr = url.Substring(prefix);
            
            var echoBody = Encoding.UTF8.GetBytes($"{bodyStr}");
            var echoHeader = Encoding.UTF8.GetBytes($"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {bodyStr.Length}\r\n\r\n");

            try
            {
                SendData(echoHeader, echoBody);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Error replaying echo");
            }
            
            Console.WriteLine($"Echo Command (message: {bodyStr})");
        }

        static void FileRequest(string url, string request, int id)
        {
            var prefix = "/file/".Length;
            var fileName = url.Substring(prefix);
            var file = "";
            
            if (request.Contains(id.ToString()))
            {
                if (File.Exists(fileName))
                {
                    if (request.Contains("gzip"))
                    {
                        Compress(url);
                    }
                    else
                    {
                        Console.WriteLine($"requested File (name: {fileName})");
                        FileInfo fileInfo = new FileInfo(fileName);
                        long fileSize = fileInfo.Length;
                        file = File.ReadAllText(fileName);
                    
                        var header = Encoding.UTF8.GetBytes($"HTTP/1.1 200 OK\r\nContent-Type: application/octet-stream\r\nContent-Length: {fileSize}\r\n\r\n");
                        var body = Encoding.UTF8.GetBytes($"{file}");
                    
                        SendData(header, body);
                    }
                }
                else if (!File.Exists(fileName))
                {
                    if (request.Contains("POST"))
                    {
                        Post(fileName, request);
                    }
                    else
                    {
                        var notfoundHeader = Encoding.UTF8.GetBytes($"HTTP/1.1 404 Not Found\r\n\r\n");;
                        var emptyBody = Encoding.UTF8.GetBytes("");
                        
                        SendData(notfoundHeader, emptyBody);
                    }
                }
            }
            else if (!request.Contains(id.ToString()))
            {
                Console.WriteLine("Access forbidden (code: 403)");
                
                var forbiddenHeader  = Encoding.UTF8.GetBytes("HTTP/1.1 403 Forbidden\r\n\r\n");
                var emptyBody = Encoding.UTF8.GetBytes("");
                
                SendData(forbiddenHeader, emptyBody);
            }
        }

        static void Post(string fileName, string request)
        {
            var requestBody = request.Split("\r\n\r\n");
            var fileContent = requestBody[1];
            var fileEnding = fileName.Split(".");
            var allowedType = "txt";

            if (fileEnding[1] == allowedType)
            {
                try
                {
                    File.WriteAllText(fileName, fileContent);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                Console.WriteLine("POST request fulfilled");
            }
            else if (fileEnding[1] != allowedType)
            {
                Console.WriteLine("Creating forbidden File Type (code: 403)");
                
                var forbiddenHeader  = Encoding.UTF8.GetBytes("HTTP/1.1 403 Forbidden\r\n\r\n");
                var emptyBody = Encoding.UTF8.GetBytes("");
                
                SendData(forbiddenHeader, emptyBody);
            }
        }

        static void Compress(string url)
        {
            var fileName = url.Substring("/file/".Length);
            var inputFile = fileName;
            var outputFile = "compressedFile.gz";

            try
            {
                using FileStream fs = File.Open(inputFile, FileMode.Open);
                using FileStream fsc = File.Create(outputFile);
                using GZipStream gs = new GZipStream(fsc, CompressionMode.Compress);
                fs.CopyTo(gs);
                Console.WriteLine($"compressing file (name: {inputFile})");
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        static void SendData(byte[] header, byte[] body)
        {
            try
            {
                lock (TCPclient)
                {
                    foreach (var client in TCPclient.ToList())
                    {
                        try
                        {
                            client.GetStream().Write(header, 0, header.Length);
                            client.GetStream().Write(body, 0, body.Length);
                            Console.WriteLine("successfully sent Data");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("TCP client Exception");
                            client.Close();
                            TCPclient.Remove(client);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Error sending data");
            }
        }
    }
}