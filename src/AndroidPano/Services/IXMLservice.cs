using AndroidPano.Models;
using System.Collections.Generic;
using System.Xml.Linq;

namespace AndroidPano.Services
{
    public interface IXMLService
    {
        void Init();
        void WriteToFile(string dir);
        void AddScene(Pano pano, IEnumerable<XElement> hotspots, string imageUrl);
    }
}
