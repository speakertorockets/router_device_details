using AngleSharp.Dom;
using System;
using System.Text.Json;

namespace RouterDeviceDetails
{
    internal class Program
    {
        static readonly string routerUrl = @"http://192.168.1.254/xslt?PAGE=C_2_0a";
        static readonly string destinationFile = "router_devices.txt";

        static void Main(string[] args)
        {
            Console.WriteLine("Getting device details from router...");

            string json = GetDeviceListFromRouter();
            File.WriteAllText(destinationFile, json);

            Console.WriteLine("done.");
        }

        private static string GetDeviceListFromRouter()
        {
            // accumulate the device details into a list
            List<RouterDeviceInfo> routerDevices = new();

            // grab the router device list from the url
            HttpClient httpClient = new();
            string htmlstring = httpClient.GetStringAsync(routerUrl).Result;

            // parse the html into an object so we can scrape it for details
            var htmlParser = new AngleSharp.Html.Parser.HtmlParser();
            var doc = htmlParser.ParseDocument(htmlstring);

            // we find the device attributes using the "colortable" class on all the tables
            var deviceTableNodes = doc.All.Where(m => m.ClassName is not null && m.ClassName.Equals("colortable"));
            foreach (var deviceTableNode in deviceTableNodes)
            {
                // we find the device name from the element previous to the table
                var deviceNameNode = deviceTableNode.PreviousElementSibling;

                // cleanup and save the device name removing the "Device : " string (9 chars)prepended to it

                string hostname  = deviceNameNode.TextContent.Substring(9).Trim();
                var routerDeviceInfo = new RouterDeviceInfo { Hostname = hostname };

                // we need to scrape the 'colortable' for attribute keys and values
                // get a list of all the cells in the table
                var allCells = deviceTableNode.QuerySelectorAll("td");
                string attributeKey = default!;

                // walk the cells
                foreach (var cell in allCells)
                {
                    // cell index 1 is the attribute key
                    if (cell.Index() == 1)
                    {
                        attributeKey = cell.TextContent.Trim();
                    }

                    // cell index 3 is the attribute value
                    // ignore attribute values if we don't have a current key
                    if ((cell.Index() == 3) && (!String.IsNullOrEmpty(attributeKey)))
                    {
                        string attributeValue = cell.TextContent.Trim();
                        routerDeviceInfo.Attributes.Add(new RouterDeviceAttribute { Key = attributeKey, Value = attributeValue });
                    }

                    // reset the key if we didn't just set it
                    if (cell.Index() != 1) 
                    { 
                        attributeKey = String.Empty; 
                    }
                }
                // we finished scraping the table, add this device to the devices list
                routerDevices.Add(routerDeviceInfo);
            }

            // serialize the devices list into json and return
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(routerDevices, options);
        }
    }

    public class RouterDeviceInfo
    {
        public required string Hostname { get; set; }
        public List<RouterDeviceAttribute> Attributes { get; set; }
        public RouterDeviceInfo()
        {
            Attributes = new();
        }
    }

    public class RouterDeviceAttribute
    {
        public required string Key { get; set; }
        public required string Value { get; set; }
    }
}