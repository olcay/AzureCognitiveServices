using System;
using System.Collections.Generic;
using System.IO;
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
        private static IHostingEnvironment _environment;

        private readonly IFaceClient _faceClient;

        // Specify the features to return
        private static readonly FaceAttributeType[] FaceAttributes =
            {
                FaceAttributeType.Age,
                FaceAttributeType.Gender,
                FaceAttributeType.Smile,
                FaceAttributeType.HeadPose,
                FaceAttributeType.FacialHair,
                FaceAttributeType.Glasses,
                FaceAttributeType.Makeup,
                FaceAttributeType.Accessories,
                FaceAttributeType.Hair
            };

        public FaceController(IHostingEnvironment environment, IFaceClient faceClient)
        {
            _environment = environment;
            _faceClient = faceClient;
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
                        IList<DetectedFace> faceList =
                            await _faceClient.Face.DetectWithStreamAsync(
                                imageStream, true, false, FaceAttributes);
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
