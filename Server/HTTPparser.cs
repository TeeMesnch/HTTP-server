namespace Server
{
    public class HttpParser
    {
        public static string GetMethod(string request)
        {
            var method = "";

            try
            {
                method = request.Split("/")[0];
                method = method.Replace(" ", "");
            }
            catch (Exception e)
            {
                Console.WriteLine("Error parsing Method");
            }
            
            return method;
        }

        public static string GetDomain(string request)
        {
            string url = "";
            
            try
            {
                var lines = request.Split("\r\n");
                var requestLine = lines[0].Split(" ");
                url = requestLine[1];
            }
            catch (Exception e)
            {
                Console.WriteLine("Error parsing Domain");
            }
            
            return url;
        }
    
        
        public static string GetVersion(string request, string url, string method)
        {
            string version = "";

            try
            {
                var prefix = method.Length + url.Length + 2;
                var line = request.Substring(prefix);
                version = line.Split("\r\n")[0];
            }
            catch (Exception e)
            {
                Console.WriteLine("Error parsing HTTP Version");
            }
            
            return version;
        }

        public static string GetHeaders(string request)
        {
            var headers = "";

            try
            {
                var line = request.Split("\r\n");
                int toTrim = line[0].Length;
                
                headers = request.Substring(toTrim);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error parsing Headers");
            }
            
            return headers;
        }
        
        static void GetBody()
        {
            Console.WriteLine("getBody");
        }
    }
}