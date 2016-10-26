using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using WebVRPano.Models;

namespace WebVRPano.Services
{
    public class PanoService : IPanoService
    {
        private readonly IConfigurationService _configurationService;
        private readonly IXmlService _xmlService;
        private readonly IHostingEnvironment _env;

        public PanoService(IConfigurationService configurationService, IXmlService xmlService, IHostingEnvironment env)
        {
            _configurationService = configurationService;
            _xmlService = xmlService;
            _env = env;
        }

        public void LoadPano(string tinyId)
        {
            var panos = GetPanos(tinyId);
            var objectDir = $@"{_env.WebRootPath}/webvrpano/{tinyId}/";
            Directory.CreateDirectory(objectDir);

            _xmlService.Init();

            ProcessPanos(panos, objectDir);
        }

        private PanoModel GetPanos(string tinyId)
        {
            var globalId = Descramble(tinyId);
            if (string.IsNullOrEmpty(globalId) || globalId.Length < 7)
            {
                throw new Exception("Ongeldig tinyId");
            }

            var soortAanbod = GetSoortAanbod(globalId);
            var panoItems = Get360Photos(globalId, soortAanbod);
            return new PanoModel(panoItems, globalId, soortAanbod);
        }

        public List<RootObject> Get360Photos(string globalId, string soortAanbod)
        {
            var apiKey = _configurationService.Get("ApiKey");

            if (soortAanbod.Equals("nieuwbouw"))
            {
                soortAanbod = "nieuwbouwproject";
            }

            var requestUrl = $"http://partnerapi.funda.nl/feeds/Aanbod.svc/get360photos/{apiKey}/?globalId={globalId}&soortAanbod={soortAanbod}";

            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var response = httpClient.GetStringAsync(requestUrl).Result;
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
            var apiKey = _configurationService.Get("ApiKey");
            var detailAlgemeenUrl = $"http://partnerapi.funda.nl/feeds/Aanbod.svc/detail/{apiKey}/algemeen/{globalId}";
            HttpResponseMessage result;

            using (var client = new HttpClient())
            {
                result = client.GetAsync(detailAlgemeenUrl).Result;
            }

            if (result.IsSuccessStatusCode)
            {
                var xml = result.Content.ReadAsStringAsync().Result;
                var xdoc = XElement.Parse(xml);
                return xdoc.Elements().FirstOrDefault(e => e.Name.LocalName.Equals("SoortAanbod")).Value;
            }

            Console.WriteLine(result.ReasonPhrase);
            return string.Empty;
        }

        private void ProcessPano(Pano pano, PanoModel panoModel)
        {
            HttpResponseMessage result;
            var soortAanbod = panoModel.SoortAanbod;

            using (var client = new HttpClient())
            {
                result = client.GetAsync($"http://partnerapi.funda.nl/feeds/MijnFunda.svc/GetKrpanoXmlContent/?type={soortAanbod}&globalId={panoModel.GlobalId}&mediaGuid={pano.Id}").Result;
            }

            if (result.IsSuccessStatusCode)
            {
                var xml = result.Content.ReadAsStringAsync().Result;
                var xdoc = XDocument.Parse(Sanitize(xml));
                var images = xdoc.Root.Descendants("tablet").Descendants();
                _xmlService.AddScene(pano, GetHotspots(xdoc, panoModel), images.First().FirstAttribute.Value);
            }
            else
            {
                Console.WriteLine(result.ReasonPhrase);
            }
        }

        private static string Sanitize(string xml)
        {
            var result = xml;

            result = result.Replace("<string xmlns=\"http://schemas.microsoft.com/2003/10/Serialization/\">", string.Empty);
            result = result.Replace("&lt;", "<");
            result = result.Replace("&gt;", ">");
            result = result.Replace("</string>", string.Empty);

            return result;
        }

        private void ProcessPanos(PanoModel panos, string objectDir)
        {
            foreach (var pano in panos.Panos)
            {
                ProcessPano(pano, panos);
            }
            _xmlService.WriteToFile(objectDir);
        }

        private static IEnumerable<XElement> GetHotspots(XDocument xdoc, PanoModel panos)
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
