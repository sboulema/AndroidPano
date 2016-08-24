using System.Collections.Generic;
using System.Xml.Linq;
using AndroidPano.Models;

namespace WebVRPano.Services
{
    public interface IXMLService
    {
        void Init();
        void WriteToFile(string dir);
        void AddScene(Pano pano, IEnumerable<XElement> hotspots, string imageUrl);
    }
}
