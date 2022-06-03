using AI_Hackathon.Models;
using Microsoft.Azure;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static AI_Hackathon.Models.SearchService;

namespace AI_Hackathon.Controllers
{
    public class ImageController : Controller
    {
        ImageService imageService = new ImageService();

        // GET: Image
        public ActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Upload(HttpPostedFileBase photo)
        {
            SearchService Searchresponse = new SearchService();
            if (photo != null && photo.ContentLength > 0)
            {
                int MaxContentLength = 1024 * 1024 * 1; //Size = 1 MB  
                IList<string> AllowedFileExtensions = new List<string> { ".jpg", ".gif", ".png", ".jpeg" };
                var ext = photo.FileName.Substring(photo.FileName.LastIndexOf('.'));
                var extension = ext.ToLower();
                if (!AllowedFileExtensions.Contains(extension))
                {
                    ViewBag.Message = string.Format("Please Upload image of type .jpg,.jpeg,.png.");
                }
                else if (photo.ContentLength > MaxContentLength)
                {
                    ViewBag.Message = string.Format("Please Upload a file upto 1 mb.");
                }
                else
                {
                    var imageUrl = await imageService.UploadImageAsync(photo);
                    TempData["LatestImage"] = imageUrl.ToString();
                    //Searchresponse = JsonConvert.DeserializeObject<SearchService>(AzureSearch());

                    string AZURE_SEARCH_URL = CloudConfigurationManager.GetSetting("FunctionAppSearchURL"); ;
                    var client = new RestClient(AZURE_SEARCH_URL);
                    Console.WriteLine(AZURE_SEARCH_URL);
                    client.Timeout = -1;
                    var request = new RestRequest(Method.POST);
                    request.AddHeader("Content-Type", "application/json");
                    request.RequestFormat = DataFormat.Json;
                    BlobURLReq account = new BlobURLReq
                    {
                        url = imageUrl.ToString()
                    };
                    var requestModel = JsonConvert.SerializeObject(account, new JsonSerializerSettings());
                    request.AddJsonBody(requestModel);
                    client.FollowRedirects = false;
                    IRestResponse response = client.Execute(request);
                    return View("SearchResult", JsonConvert.DeserializeObject<AzureSearch>(response.Content));
                    // return await Task.FromResult(Searchresponse);
                    //return RedirectToAction("LatestImage");
                }
            }
            else
            {
                ViewBag.Message = string.Format("Please Upload an image.");
            }
            return View("SearchResult",new AzureSearch());
        }

        public ActionResult LatestImage()
        {

            var latestImage = string.Empty;
            if (TempData["LatestImage"] != null)
            {
                ViewBag.LatestImage = Convert.ToString(TempData["LatestImage"]);
            }

            return View();
        }



        public ActionResult AzureSearch(string url)
        {
            string AZURE_SEARCH_URL = CloudConfigurationManager.GetSetting("FunctionAppSearchURL"); ;
            var client = new RestClient(AZURE_SEARCH_URL);
            Console.WriteLine(AZURE_SEARCH_URL);
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json");
            request.RequestFormat = DataFormat.Json;
            BlobURLReq account = new BlobURLReq
            {
                url = url
            };
            var requestModel = JsonConvert.SerializeObject(account, new JsonSerializerSettings());
            request.AddJsonBody(requestModel);
            client.FollowRedirects = false;
            IRestResponse response = client.Execute(request);
            return PartialView("SearchResult", JsonConvert.DeserializeObject<List<AzureSearch>>(response.Content));
        }
        public class BlobURLReq
        {
            public string url { get; set; }
        }
    }
}