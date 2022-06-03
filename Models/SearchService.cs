using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AI_Hackathon.Models
{
    public class SearchService
    {


    }
    public class AzureSearch
    {
        
        [JsonProperty("@odata.context")]
        public string OdataContext { get; set; }
        public List<Value> value { get; set; }
    }

    public class Value
    {
        [JsonProperty("@search.score")]
        public double SearchScore { get; set; }
        public string id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ImageURL { get; set; }
    }

    

}