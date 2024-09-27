using Newtonsoft.Json;
public class Feature
{
    [JsonProperty("properties")]
    public Properties? Properties {get; set;}
}