using System.Collections.Generic;
using System.Linq;

namespace AndroidPano.Models
{
    public class PanoModel
    {
        private List<RootObject> Items { get; set; }

        public List<Pano> Panos { get; set; }
        public string GlobalId { get; set; }
        public string SoortAanbod { get; set; }

        public PanoModel(List<RootObject> items, string globalId, string soortAanbod)
        {
            Items = items;
            GlobalId = globalId;
            SoortAanbod = soortAanbod;

            Panos = new List<Pano>();
            foreach (var item in items)
            {
                var pano = new Pano();

                pano.Omschrijving = item.Omschrijving;
                if (Panos.Any(p => p.Omschrijving.Equals(item.Omschrijving)))
                {
                    pano.Omschrijving += item.IndexNumber;
                }

                pano.Id = item.Id;
                pano.Url = item.MediaItems.Where(mi => mi.Category == 23).Select(mi => mi.Url).FirstOrDefault();
                Panos.Add(pano);
            }
        }

        public PanoModel(List<RootObject> items)
        {
            Items = items;

            Panos = new List<Pano>();
            foreach (var item in items)
            {
                var pano = new Pano();

                pano.Omschrijving = item.Omschrijving;
                pano.Url = "http://www.funda.nl";
                Panos.Add(pano);
            }
        }

        public int GetIndex(Pano pano)
        {
            for (int i = 0; i < Panos.Count; i++)
            {
                if (Panos[i] == pano) return i;
            }
            return -1;
        }

        public string GetWebsite()
        {
            switch (SoortAanbod.ToLower())
            {
                case "kantoor":
                case "bedrijfshal":
                case "winkel":
                case "horeca":
                case "bouwgrond":
                case "overig":
                    return "fundainbusiness";
                default:
                    return "funda";
            }
        }
    }

    public class Pano
    {
        public string Omschrijving { get; set; }
        public string Url { get; set; }
        public string Id { get; set; }
    }
}
