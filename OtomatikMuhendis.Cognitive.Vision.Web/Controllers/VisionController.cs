using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Newtonsoft.Json;
using OtomatikMuhendis.Cognitive.Vision.Web.Core;
using OtomatikMuhendis.Cognitive.Vision.Web.Models;

namespace OtomatikMuhendis.Cognitive.Vision.Web.Controllers
{
    public class VisionController : Controller
    {
        private static IHostingEnvironment _environment;

        private readonly IComputerVisionClient _computerVisionClient;

        // Specify the features to return
        private static readonly List<VisualFeatureTypes> Features =
            new List<VisualFeatureTypes>()
        {
            VisualFeatureTypes.Categories, VisualFeatureTypes.Description,
            VisualFeatureTypes.Faces, VisualFeatureTypes.ImageType,
            VisualFeatureTypes.Tags, VisualFeatureTypes.Color,
            VisualFeatureTypes.Brands, VisualFeatureTypes.Objects
        };

        public VisionController(IHostingEnvironment environment, IComputerVisionClient computerVisionClient)
        {
            _environment = environment;
            _computerVisionClient = computerVisionClient;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(List<IFormFile> files)
        {
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
                        var analysis = await _computerVisionClient.AnalyzeImageInStreamAsync(imageStream, Features);
                        analysisResult = ReadResults(analysis);
                    }
                }
            }

            return View(new ResultViewModel { FilePathList = new List<string>() { fileName }, AnalysisResult = analysisResult });
        }

        private static string ReadResults(ImageAnalysis analysis)
        {
            return JsonPrint.Prettify(JsonConvert.SerializeObject(analysis));
        }
    }
}
