using System;
using System.Net;
using System.IO;

namespace SDW
{

    class Connectionbuilder {
        private string url;

        public Connectionbuilder(string url)
        {
            this.url = url;
        }

        public  WebRequest BuildConnection() 
        {
           return  WebRequest.Create(url);
        }

        public string Retrieve() => url;

    }
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("========== SDW Interface =========");

            try
                {
                    // Define the Connection API
                    Connectionbuilder conn =  new Connectionbuilder("https://sdw-wsrest.ecb.europa.eu/service/data/EXR/M.USD.EUR.SP00.A");
                    // Display the Connection
                    Console.WriteLine("Connecting to {0}", conn.Retrieve());
                    //Define the mime content

                    // Get the response from the Server
                    HttpWebResponse response = (HttpWebResponse)conn.BuildConnection().GetResponse ();
                    // Display the status of the response
                    Console.WriteLine("Server Answer: {0}\nServer Code {1}", response.StatusDescription, response.StatusCode);
                    // Get the response as a byte stream
                    Stream dataStream = response.GetResponseStream ();
                    // Open the stream using a StreamReader for easy access.
                    StreamReader reader = new StreamReader (dataStream);
                    // Read the content and store as string
                    string responseFromServer = reader.ReadToEnd ();
                    // Save the string into a file
                    string fileName = "C:\\Users\\D60016\\Software\\SDW\\output\\serverContent.xml";
                    Console.WriteLine("Saving Content to file {0}", fileName);
                    System.IO.File.WriteAllText (@fileName, responseFromServer);
                    // Console.WriteLine (responseFromServer);
                    // Cleanup the streams and the response.
                    reader.Close ();
                    dataStream.Close ();
                    response.Close ();

                }
            catch (System.Exception)
                {
                    Console.WriteLine("Issue with SDW Interface");
                    throw;
                };

                
            





        }
    }
}
