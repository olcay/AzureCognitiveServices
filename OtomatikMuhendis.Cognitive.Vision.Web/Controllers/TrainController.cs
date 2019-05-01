using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using OtomatikMuhendis.Cognitive.Vision.Web.Models;

namespace OtomatikMuhendis.Cognitive.Vision.Web.Controllers
{
    public class TrainController : Controller
    {
        public static IHostingEnvironment _environment;

        private const string subscriptionKey = "1e500aa5ba4144538e9764a18788dff7";

        private FaceClient _faceClient;

        public TrainController(IHostingEnvironment environment)
        {
            _environment = environment;

            _faceClient = new FaceClient(
                new ApiKeyServiceClientCredentials(subscriptionKey),
                new System.Net.Http.DelegatingHandler[] { });

            _faceClient.Endpoint = "https://westeurope.api.cognitive.microsoft.com";
        }

        [HttpGet("train/status/{personGroupId}")]
        public async Task<IActionResult> Status([FromRoute] string personGroupId)
        {
            try
            {
                var trainingStatus = await _faceClient.PersonGroup.GetTrainingStatusAsync(personGroupId);
                return Ok(trainingStatus.Status);
            }
            catch (Exception)
            {
                return NotFound();
            }
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(List<IFormFile> files, string name)
        {
            string personGroupId = "myfriends";

            //await _faceClient.PersonGroup.CreateAsync(personGroupId, "My friends");

            var friend1 = await _faceClient.PersonGroupPerson.CreateAsync(
                personGroupId,
                name
            );

            List<string> filePathList = new List<string>();
            
            foreach (var formFile in files)
            {
                var fileName = "\\uploads\\" + Convert.ToString(Guid.NewGuid()) + ".jpg";

                var filePath = _environment.WebRootPath + fileName;

                if (formFile.Length > 0)
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await formFile.CopyToAsync(stream);
                        filePathList.Add(fileName);
                    }

                    using (Stream imageStream = new FileStream(filePath, FileMode.Open))
                    {
                        await _faceClient.PersonGroupPerson.AddFaceFromStreamAsync(personGroupId, friend1.PersonId, imageStream);
                    }
                }
            }

            await _faceClient.PersonGroup.TrainAsync(personGroupId);

            return View(new ResultViewModel { FilePathList = filePathList, AnalysisResult = "Training Started" });
        }
    }
}
