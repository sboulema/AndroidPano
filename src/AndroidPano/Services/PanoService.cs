using AndroidPano.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AndroidPano.Services
{
    public class PanoService : IPanoService
    {
        private readonly IConfigurationService configurationService;
        private readonly IXMLService xmlService;

        public PanoService(IConfigurationService configurationService, IXMLService xmlService)
        {
            this.configurationService = configurationService;
            this.xmlService = xmlService;
        }

        public void LoadPano(string tinyId)
        {
            var panos = GetPanos(tinyId);
            var objectDir = $@"androidpano\{tinyId}\";
            Directory.CreateDirectory(objectDir);

            xmlService.Init();

            ProcessPanos(panos, objectDir);
        }

        private PanoModel GetPanos(string tinyId)
        {
            string globalId = Descramble(tinyId);
            var soortAanbod = GetSoortAanbod(globalId);
            var panoItems = Get360Photos(globalId, soortAanbod);
            return new PanoModel(panoItems, globalId, soortAanbod);
        }

        public List<RootObject> Get360Photos(string globalId, string soortAanbod)
        {
            var apiKey = configurationService.Get("ApiKey");
            var requestUrl = $"http://partnerapi.funda.nl/feeds/Aanbod.svc/get360photos/{apiKey}/?globalId={globalId}&soortAanbod={soortAanbod}";

            try
            {
                string response;
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    response = httpClient.GetStringAsync(requestUrl).Result;
                    return JsonConvert.DeserializeObject<List<RootObject>>(response);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return null;
        }

        private string GetSoortAanbod(string globalId)
        {
            var apiKey = configurationService.Get("ApiKey");
            var detailAlgemeenUrl = $"http://partnerapi.funda.nl/feeds/Aanbod.svc/detail/{apiKey}/algemeen/{globalId}";
            HttpResponseMessage result;

            using (HttpClient client = new HttpClient())
            {
                result = client.GetAsync(detailAlgemeenUrl).Result;
            }

            if (result.IsSuccessStatusCode)
            {
                var xml = result.Content.ReadAsStringAsync().Result;
                var xdoc = XElement.Parse(xml);
                return xdoc.Elements().FirstOrDefault(e => e.Name.LocalName.Equals("SoortAanbod")).Value;
            }
            else
            {
                Console.WriteLine(result.ReasonPhrase);
            }
            
            return string.Empty;
        }

        private void ProcessPano(Pano pano, PanoModel panoModel, string objectDir)
        {
            try
            {
                XDocument xdoc = XDocument.Load($"http://m.{panoModel.GetWebsite()}.nl/partialaction/krpanoxml/{panoModel.GlobalId}/{panoModel.SoortAanbod}/{pano.Id}/");
                var images = xdoc.Root.Descendants("tablet").Descendants();

                xmlService.AddScene(pano, GetHotspots(xdoc, panoModel), images.First().FirstAttribute.Value);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void ProcessPanos(PanoModel panos, string objectDir)
        {
            foreach (var pano in panos.Panos)
            {
                ProcessPano(pano, panos, objectDir);
            }
            xmlService.WriteToFile(objectDir);
        }

        private IEnumerable<XElement> GetHotspots(XDocument xdoc, PanoModel panos)
        {
            var hotspots = xdoc.Root.Descendants("hotspot");
            foreach (var hotspot in hotspots)
            {
                if (!hotspot.Attribute("style").Value.Equals("infospot"))
                {
                    hotspot.SetAttributeValue("style", "hotspotstyle");
                    var linkedScene = new XAttribute("linkedscene", panos.Panos.FirstOrDefault(p => p.Id.Equals(hotspot.Attribute("href").Value)).Omschrijving);
                    hotspot.Attribute("href").Remove();
                    hotspot.Add(linkedScene);
                }
            }

            return hotspots;
        }

        private async Task DownloadFileAsync(string url, string filename)
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    var contentStream = httpClient.GetStreamAsync(url).Result;
                    var stream = new FileStream(filename, FileMode.Create);
                    await contentStream.CopyToAsync(stream);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static string Descramble(string input)
        {
            try
            {
                int scrambleKey = int.Parse(input[0].ToString());
                //scramble key is eerste cijfer

                //de rest van het getal gaan we descramble-en
                string strGetal = input.Substring(1);
                int[] cijferArray = new int[strGetal.Length];
                for (int i = 0; i < strGetal.Length; i++)
                    cijferArray[i] = int.Parse(strGetal[i].ToString());

                //cijfer-shift terug
                for (int i = 0; i < strGetal.Length; i++)
                {
                    int increment = (((i % 2) * 2 - 1) * (scrambleKey + i / 2)) % 10;
                    cijferArray[i] = (cijferArray[i] - increment + 10) % 10;
                }

                return ConcatenateToString(cijferArray);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string ConcatenateToString(ICollection<int> getalArray)
        {
            StringBuilder sb = new StringBuilder(getalArray.Count);
            foreach (int getal in getalArray)
                sb.Append(getal);
            return sb.ToString();
        }
    }
}
