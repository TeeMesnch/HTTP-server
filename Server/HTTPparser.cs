namespace Server
{
    public class HttpParser
    {
        public static string GetMethod(string request)
        {
            string method;

            try
            {
                method = request.Split("/")[0];
                method = method.Replace(" ", "");
            }
            catch (Exception)
            {
                Console.WriteLine("Error parsing Method");
                throw;
            }
            
            return method;
        }

        public static string GetDomain(string request)
        {
            string url;
            
            try
            {
                var lines = request.Split("\r\n");
                var requestLine = lines[0].Split(" ");
                url = requestLine[1];
            }
            catch (Exception)
            {
                Console.WriteLine("Error parsing Domain");
                throw;
            }
            
            return url;
        }
    
        
        public static string GetVersion(string request, string url, string method)
        {
            string version;

            try
            {
                var prefix = method.Length + url.Length + 2;
                var line = request.Substring(prefix);
                version = line.Split("\r\n")[0];
            }
            catch (Exception)
            {
                Console.WriteLine("Error parsing HTTP Version");
                throw;
            }
            
            return version;
        }

        public static string GetHeaders(string request)
        {
            string headers;

            try
            {
                var line = request.Split("\r\n");
                int toTrim = line[0].Length;
                
                headers = request.Substring(toTrim);
            }
            catch (Exception)
            {
                Console.WriteLine("Error parsing Headers");
                throw;
            }
            
            return headers;
        }
        
        public static string GetBody(string request)
        {
            string body;

            try
            {
                body =  request.Split("\r\n\r\n")[1];

                if (body == "")
                {
                    Console.WriteLine("the body provided is empty");
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Error parsing Body");
                throw;
            }
            return body;
        }
    }
}