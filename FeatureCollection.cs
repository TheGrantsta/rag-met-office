using Newtonsoft.Json;
public class FeatureCollection
{
    [JsonProperty("features")]
    public List<Feature> Features { get; set; } = new List<Feature>();
}