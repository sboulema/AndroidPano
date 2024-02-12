using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using WebVRPano.Models;

namespace WebVRPano.Services;

public class PanoService(
    IXmlService xmlService,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    IWebHostEnvironment webHostEnvironment) : IPanoService
{
    private readonly string _apiKey = configuration.GetValue<string>("API_KEY") ?? string.Empty;

    public async Task LoadPano(string tinyId)
    {
        var panos = await GetPanos(tinyId);
        var objectDir = $@"{webHostEnvironment.WebRootPath}/webvrpano/{tinyId}/";
        Directory.CreateDirectory(objectDir);

        xmlService.Init();

        await ProcessPanos(panos, objectDir);
    }

    private async Task<PanoModel> GetPanos(string tinyId)
    {
        var globalId = Descramble(tinyId);
        if (string.IsNullOrEmpty(globalId) || globalId.Length < 7)
        {
            throw new Exception("Ongeldig tinyId");
        }

        var soortAanbod = await GetSoortAanbod(globalId);
        var panoItems = await Get360Photos(globalId, soortAanbod);
        return new PanoModel(panoItems, globalId, soortAanbod);
    }

    public async Task<List<RootObject>> Get360Photos(string globalId, string soortAanbod)
    {
        if (soortAanbod.Equals("nieuwbouw"))
        {
            soortAanbod = "nieuwbouwproject";
        }

        var client = httpClientFactory.CreateClient("funda");
        var result = await client.GetFromJsonAsync<List<RootObject>>($"/feeds/Aanbod.svc/get360photos/{_apiKey}/?globalId={globalId}&soortAanbod={soortAanbod}");
        return result ?? [];
    }

    private async Task<string> GetSoortAanbod(string globalId)
    {
        var client = httpClientFactory.CreateClient("funda");
        var result = await client.GetFromJsonAsync<AanbodAlgemeen>($"/feeds/Aanbod.svc/detail/{_apiKey}/algemeen/{globalId}");
        return result?.SoortAanbod ?? string.Empty;
    }

    private async Task ProcessPano(Pano pano, PanoModel panoModel)
    {
        var client = httpClientFactory.CreateClient();
        var xml = await client.GetStringAsync($"http://partnerapi.funda.nl/feeds/MijnFunda.svc/GetKrpanoXmlContent/?type={panoModel.SoortAanbod}&globalId={panoModel.GlobalId}&mediaGuid={pano.Id}");

        var xdoc = XDocument.Parse(Sanitize(xml));
        var images = xdoc.Root?.Descendants("tablet").Descendants();
        xmlService.AddScene(pano, GetHotspots(xdoc, panoModel), "https://corsproxy.io/?" + images?.First().FirstAttribute?.Value);
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

    private async Task ProcessPanos(PanoModel panos, string objectDir)
    {
        panos.Panos.ForEach(async pano => await ProcessPano(pano, panos));

        xmlService.WriteToFile(objectDir);
    }

    private static IEnumerable<XElement> GetHotspots(XDocument xdoc, PanoModel panos)
    {
        var hotspots = xdoc.Root?.Descendants("hotspot") ?? [];

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
            var scrambleKey = int.Parse(input[0].ToString());
            //scramble key is eerste cijfer

            //de rest van het getal gaan we descramble-en
            string strGetal = input[1..];
            int[] cijferArray = new int[strGetal.Length];

            for (int i = 0; i < strGetal.Length; i++)
            {
                cijferArray[i] = int.Parse(strGetal[i].ToString());
            }

            //cijfer-shift terug
            for (int i = 0; i < strGetal.Length; i++)
            {
                int increment = (i % 2 * 2 - 1) * (scrambleKey + i / 2) % 10;
                cijferArray[i] = (cijferArray[i] - increment + 10) % 10;
            }

            return ConcatenateToString(cijferArray);
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    private static string ConcatenateToString(ICollection<int> getalArray)
    {
        var sb = new StringBuilder(getalArray.Count);

        foreach (int getal in getalArray)
        {
            sb.Append(getal);
        }

        return sb.ToString();
    }
}
