using Newtonsoft.Json;
namespace rag_met_office.MetOffice
{
    public class Feature
    {
        [JsonProperty("properties")]
        public Properties? Properties {get; set;}
    }
}