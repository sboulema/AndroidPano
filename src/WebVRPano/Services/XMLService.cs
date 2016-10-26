using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using WebVRPano.Models;

namespace WebVRPano.Services
{
    public class XmlService : IXmlService
    {
        XElement _tourXml;

        public void Init()
        {
            _tourXml = new XElement("krpano");

            var include = new XElement("include");
            var includeUrl = new XAttribute("url", "../../xml/krpano_vr.xml");
            include.Add(includeUrl);
            _tourXml.Add(include);
        }

        public void WriteToFile(string dir)
        {
            using (var fs = new FileStream(Path.Combine(dir, "tour.xml"), FileMode.Create))
            {
                _tourXml.Save(fs);
            }
            
            _tourXml = null;
        }

        public void AddScene(Pano pano, IEnumerable<XElement> hotspots, string imageUrl)
        {
            var scene = new XElement("scene");
            var sceneName = new XAttribute("name", pano.Omschrijving);
            scene.Add(sceneName);

            // Images
            var image = new XElement("image");
            var cube = new XElement("cube");
            var cubeUrl = new XAttribute("url", imageUrl.Replace("_l", "_%s"));
            cube.Add(cubeUrl);
            image.Add(cube);
            scene.Add(image);

            // Hotspots
            foreach (var spot in hotspots)
            {
                scene.Add(spot);
            }

            _tourXml.Add(scene);
        }
    }
}
