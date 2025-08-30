using System.Text;

namespace Server
{
    public class HTTPparser
    {
        public static void getMethod()
        {
            
            Console.WriteLine("getMethod");
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
                Console.WriteLine(e.Message);
                Console.WriteLine("Error parsing Domain");
            }
            return url;
        }
    
        
        static void getVersion()
        {
            Console.WriteLine("getVersion");
        }

        static void getHeaders()
        {
            Console.WriteLine("getHeaders");
        }
        
        static void getBody()
        {
            Console.WriteLine("getBody");
        }
    }
}