using System.Collections.Generic;
using System.Xml.Linq;
using WebVRPano.Models;

namespace WebVRPano.Services
{
    public interface IXmlService
    {
        void Init();
        void WriteToFile(string dir);
        void AddScene(Pano pano, IEnumerable<XElement> hotspots, string imageUrl);
    }
}
