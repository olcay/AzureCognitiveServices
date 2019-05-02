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
using OtomatikMuhendis.Cognitive.Vision.Web.Core;
using OtomatikMuhendis.Cognitive.Vision.Web.Models;

namespace OtomatikMuhendis.Cognitive.Vision.Web.Controllers
{
    public class IdentifyController : Controller
    {
        private static IHostingEnvironment _environment;

        private readonly IFaceClient _faceClient;

        public IdentifyController(IHostingEnvironment environment, IFaceClient faceClient)
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
                            await _faceClient.Face.DetectWithStreamAsync(imageStream);

                        var faceIds = faceList.Where(face => face.FaceId.HasValue).Select(face => face.FaceId.Value).ToList();

                        var results = await _faceClient.Face.IdentifyAsync(faceIds, AzureCognitiveServiceParameters.PersonGroupId);
                        foreach (var identifyResult in results)
                        {
                            if (identifyResult.Candidates.Any())
                            {
                                // Get top 1 among all candidates returned
                                var candidateId = identifyResult.Candidates[0].PersonId;
                                var person = await _faceClient.PersonGroupPerson.GetAsync(AzureCognitiveServiceParameters.PersonGroupId, candidateId);
                                analysisResult += $"Identified as {person.Name}" + Environment.NewLine;
                            }
                            else
                            {
                                analysisResult += "Unknown" + Environment.NewLine;
                            }
                        }
                    }
                }
            }

            return View(new ResultViewModel { FilePathList = new List<string>() { fileName }, AnalysisResult = analysisResult });
        }
    }
}
