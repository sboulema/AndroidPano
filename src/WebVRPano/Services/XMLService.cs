using System.IO;
using System.Xml.Linq;

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

            var view = new XElement("view");
            var hlookat = new XAttribute("hlookat", "180");
            view.Add(hlookat);
            _tourXml.Add(view);

            //< view hlookat = "180" vlookat = "0" maxpixelzoom = "1.0" limitview = "auto" fovmax = "90" ></ view >
        }

        public void WriteToFile(string dir)
        {
            using (var fs = new FileStream(Path.Combine(dir, "tour.xml"), FileMode.Create))
            {
                _tourXml.Save(fs);
            }
            
            _tourXml = null;
        }

        public void AddScene(string cubeUrl)
        {
            var scene = new XElement("scene");

            // Images
            var image = new XElement("image");
            var type = new XAttribute("type", "cube");
            var multires = new XAttribute("multires", "true");
            var tilesize = new XAttribute("tilesize", "512");
            image.Add(type);
            image.Add(multires);
            image.Add(tilesize);

            var level1 = new XElement("level");
            var tiledImageWidth1 = new XAttribute("tiledimagewidth", "1664");
            var tiledImageHeight1 = new XAttribute("tiledimageheight", "1664");
            level1.Add(tiledImageWidth1);
            level1.Add(tiledImageHeight1);

            var cube = new XElement("cube");
            var cubeUrlAttribute = new XAttribute("url", cubeUrl);
            cube.Add(cubeUrlAttribute);

            level1.Add(cube);
            image.Add(level1);

            scene.Add(image);

            _tourXml.Add(scene);
        }
    }
}
