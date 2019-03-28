using System;
using System.IO;
using System.Xml;
using Microsoft.AspNetCore.Hosting;

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

        public void LoadPano(string vin)
        {
            var vinStripped = CleanupVin(vin);

            var cubeUrl = GetCubeUrl(vinStripped);

            var objectDir = $@"{_env.WebRootPath}/webvrpano/{vin}/";
            Directory.CreateDirectory(objectDir);

            _xmlService.Init();

            _xmlService.AddScene(cubeUrl);

            _xmlService.WriteToFile(objectDir);
        }

        public string GetCubeUrl(string vin)
        {
            var requestUrl = $"https://static-api.vivition.com/auto-nl/licenceplate_vin/{vin}_/krpanoscene.xml";

            try
            {
                var krpanoScene = new XmlDocument();
                krpanoScene.Load(requestUrl);

                XmlNode root = krpanoScene.DocumentElement;
                XmlNode node = root.SelectSingleNode("//level/cube");
                var cubeUrl = node.Attributes["url"].Value;

                return cubeUrl;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return null;
        }

        private string CleanupVin(string vin) => vin.Trim().Replace("-", string.Empty);
    }
}
