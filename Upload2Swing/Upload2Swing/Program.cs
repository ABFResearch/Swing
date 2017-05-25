using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Upload2Swing
{
    class Program
    {
        // gebruikte URLS
        const string studioUrl = @"https://{0}/admin/studio/";
        const string testJiveUrl = @"https://{0}/admin/jive/";
        const string testJiveServicesUrl = @"https://{0}/admin/jiveservices/";
        const string jiveLiveUrl = @"https://{0}/jive/jive/";
        const string jiveServiceLiveUrl = @"https://{0}/jive/jiveservices/";

        static string GenerateTestImportCsvDataFile(string period, string geolevel, string geoitem, decimal value)
        {
            var csvStringWriter = new StringWriter();
            // add header
            csvStringWriter.WriteLine("\"period\";\"geolevel\";\"geoitem\";\"bulkapitest\"");
            // add data record
            csvStringWriter.WriteLine("\"{0}\";\"{1}\";\"{2}\";{3}", period, geolevel, geoitem, value.ToString());
            var tempFileDataCSV = Path.GetTempFileName().Replace(".tmp", ".csv");
            File.WriteAllText(tempFileDataCSV, csvStringWriter.ToString());
            return tempFileDataCSV;
        }

        static string GenerateTestImportCsvMetaDataFile()
        {
            var csvStringWriter = new StringWriter();
            // add header
            csvStringWriter.WriteLine("\"Onderwerpcode\";\"Naam\";\"Beschrijving\";\"Eenheid\";\"Bron\"");
            // add metadata record
            csvStringWriter.WriteLine("\"bulkapitest\";\"Test onderwerp voor BulkAPI\";\"Inlezen van data en meta data via Bulk API\";\"aantal\";\"abf\"");
            var tempFileMetaDataCSV = Path.GetTempFileName().Replace(".tmp", ".csv");
            File.WriteAllText(tempFileMetaDataCSV, csvStringWriter.ToString());
            return tempFileMetaDataCSV;
        }

        static void ExecuteApiTest(string domain, string apiKey, string period, string geolevel, string geoitem, decimal value)
        {
            using (var webClient = new WebClient())
            {
                // generate test data
                var testImportCsvDataFile = GenerateTestImportCsvDataFile(period, geolevel, geoitem, value);
                Debug.Write(testImportCsvDataFile);

                // generate test metadata
                var testImportCsvMetaDataFile = GenerateTestImportCsvMetaDataFile();
                Debug.Write(testImportCsvMetaDataFile);

                // Update admin/test version
                try
                {
                    // upload data
                    string address = string.Format("{0}/BulkAPI.ashx?apikey={1}", string.Format(testJiveServicesUrl, domain), apiKey);
                    Debug.Write(address);
                    byte[] responseBytes = webClient.UploadFile(address, null, testImportCsvDataFile);
                    string response = Encoding.UTF8.GetString(responseBytes);
                    Debug.Write(response);
                    if (response != "OK")
                        throw new Exception("Import data error for admin/test version");

                    // upload meta data
                    address = string.Format("{0}/BulkAPI.ashx?apikey={1}", string.Format(testJiveServicesUrl, domain), apiKey);
                    Debug.Write(address);
                    responseBytes = webClient.UploadFile(address, null, testImportCsvMetaDataFile);
                    response = Encoding.UTF8.GetString(responseBytes);
                    Debug.Write(response);
                    if (response != "OK")
                        throw new Exception("Import meta data error for admin/test version");

                    // clear data cache      
                    address = string.Format("{0}/Update.ashx?apikey={1}&command=cleanupdatacache", string.Format(testJiveUrl, domain), apiKey);
                    Debug.Write(address);
                    response = webClient.DownloadString(address);
                    Debug.Write(response);
                    if (response != "OK")
                        throw new Exception("Clear data error for admin/test version");
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }

                // Update live/production version
                try
                {
                    // upload data
                    var address = string.Format("{0}/BulkAPI.ashx?apikey={1}", string.Format(jiveServiceLiveUrl, domain), apiKey);
                    Debug.Write(address);
                    byte[] responseBytes = webClient.UploadFile(address, null, testImportCsvDataFile);
                    string response = Encoding.UTF8.GetString(responseBytes);
                    Debug.Write(response);
                    if (response != "OK")
                        throw new Exception("Import data error for live/production version");

                    // upload meta data
                    address = string.Format("{0}/BulkAPI.ashx?apikey={1}", string.Format(jiveServiceLiveUrl, domain), apiKey);
                    Debug.Write(address);
                    responseBytes = webClient.UploadFile(address, null, testImportCsvMetaDataFile);
                    response = Encoding.UTF8.GetString(responseBytes);
                    Debug.Write(response);
                    if (response != "OK")
                        throw new Exception("Import meta data error for live/production version");

                    // clear data cache      
                    address = string.Format("{0}/Update.ashx?apikey={1}&command=cleanupdatacache", string.Format(jiveLiveUrl, domain), apiKey);
                    Debug.Write(address);
                    response = webClient.DownloadString(address);
                    Debug.Write(response);
                    if (response != "OK")
                        throw new Exception("Clear data error for live/production version");
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }

                File.Delete(testImportCsvDataFile);
            }
        }

        static void Main(string[] args)
        {
            // The following command line arguments
            if (args.Length != 6)
            {
                Console.WriteLine("Invalid number of arguments!");
                Console.WriteLine();
                Console.WriteLine("The following arguments are required:");
                Console.WriteLine();
                Console.WriteLine("1- swing domain (for example demo5.swing.eu)");
                Console.WriteLine("2- apikey (a valid apikey for calling the service, for example 'e08652dc-f8b3-4ccb-9462-9cfc79fcf564')");
                Console.WriteLine("3- period for dummy data (a valid period code in the target database, example '2016')");
                Console.WriteLine("4- geolevel for dummy data (a valid geolevel code in the target database, example 'provincie')");
                Console.WriteLine("5- geoitem for dummy data (a valid geoitem code in the target database, example '1')");
                Console.WriteLine("6- a dummy value (example 1234)");
                Console.WriteLine();
                Console.WriteLine("Example UpdateJiveData.exe demo5.swing.eu e08652dc-f8b3-4ccb-9462-9cfc79fcf564 2016 provincie 1 1234");
                return;
            }

            ExecuteApiTest(args[0], args[1], args[2], args[3], args[4], decimal.Parse(args[5]));
            Console.WriteLine("Done");
        }
    }
}
