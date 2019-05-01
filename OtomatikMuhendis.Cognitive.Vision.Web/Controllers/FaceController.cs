using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using Newtonsoft.Json;
using OtomatikMuhendis.Cognitive.Vision.Web.Core;
using OtomatikMuhendis.Cognitive.Vision.Web.Models;

namespace OtomatikMuhendis.Cognitive.Vision.Web.Controllers
{
    public class FaceController : Controller
    {
        public static IHostingEnvironment _environment;

        private const string subscriptionKey = "1e500aa5ba4144538e9764a18788dff7";

        // Specify the features to return
        private static readonly FaceAttributeType[] faceAttributes =
            { FaceAttributeType.Age, FaceAttributeType.Gender, FaceAttributeType.Smile, FaceAttributeType.Emotion };

        public FaceController(IHostingEnvironment environment)
        {
            _environment = environment;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(List<IFormFile> files)
        {
            FaceClient faceClient = new FaceClient(
                new ApiKeyServiceClientCredentials(subscriptionKey),
                new System.Net.Http.DelegatingHandler[] { });

            faceClient.Endpoint = "https://westeurope.api.cognitive.microsoft.com";

            var fileName = "\\uploads\\" + Convert.ToString(Guid.NewGuid()) + ".jpg";

            var filePath = _environment.WebRootPath + fileName;

            string analysisResult = null;
            
            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                    }

                    using (Stream imageStream = new FileStream(filePath, FileMode.Open))
                    {
                        IList<DetectedFace> faceList =
                            await faceClient.Face.DetectWithStreamAsync(
                                imageStream, true, false, faceAttributes);
                        analysisResult = ReadResults(faceList);
                    }
                }
            }

            return View(new ResultViewModel { FilePathList = new List<string>() { fileName }, AnalysisResult = analysisResult });
        }

        private static string ReadResults(IList<DetectedFace> faceList)
        {
            return JsonPrint.Prettify(JsonConvert.SerializeObject(faceList));
        }
    }
}
