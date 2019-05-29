using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using OtomatikMuhendis.Cognitive.Vision.Web.Core;
using OtomatikMuhendis.Cognitive.Vision.Web.Models;

namespace OtomatikMuhendis.Cognitive.Vision.Web.Controllers
{
    public class TrainController : Controller
    {
        private static IHostingEnvironment _environment;

        private readonly IFaceClient _faceClient;


        public TrainController(IHostingEnvironment environment, IFaceClient faceClient)
        {
            _environment = environment;
            _faceClient = faceClient;
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

        public async Task<IActionResult> CreateGroup()
        {
            await _faceClient.PersonGroup.CreateAsync(AzureCognitiveServiceParameters.PersonGroupId, "My person group");

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Index(List<IFormFile> files, string name)
        {
            var friend1 = await _faceClient.PersonGroupPerson.CreateAsync(
                AzureCognitiveServiceParameters.PersonGroupId,
                name
            );

            var filePathList = new List<string>();
            
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
                        await _faceClient.PersonGroupPerson.AddFaceFromStreamAsync(AzureCognitiveServiceParameters.PersonGroupId, friend1.PersonId, imageStream);
                    }
                }
            }

            await _faceClient.PersonGroup.TrainAsync(AzureCognitiveServiceParameters.PersonGroupId);

            return View(new ResultViewModel { FilePathList = filePathList, AnalysisResult = "Training started" });
        }
    }
}
