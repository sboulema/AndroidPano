using System.Collections.Generic;

namespace WebVRPano.Models;

public class RootObject
{
    public string Id { get; set; } = string.Empty;

    public int IndexNumber { get; set; }

    public List<MediaItem> MediaItems { get; set; } = [];

    public string Omschrijving { get; set; } = string.Empty;
}
