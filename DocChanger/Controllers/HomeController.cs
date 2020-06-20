using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DocChanger.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System.Globalization;

namespace DocChanger.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _env;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Import()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Import(IFormFile postedFile)
        {
            if (postedFile != null)
            {
                try
                {
                    var mt = new List<ImportModel>();
                    string fileExtension = Path.GetExtension(postedFile.FileName);
                    var path = Path.Combine(_env.WebRootPath, postedFile.FileName);
                    

                    //Validate uploaded file and return error.
                    if (fileExtension != ".csv")
                    {
                        ViewBag.Message = "Please select the csv file with .csv extension";
                        return View();
                    }

                    using (var sreader = new StreamReader(postedFile.OpenReadStream()))
                    {
                        string[] headers = sreader.ReadLine().Split(';');

                        while (!sreader.EndOfStream)
                        {
                            string[] rows = sreader.ReadLine().Split(';');

                            mt.Add(new ImportModel
                            {
                                Dest = rows[0],
                                HotelCode = rows[1],
                                Hotel = rows[2],
                                Amount = float.Parse(rows[3]),
                                Date = DateTime.Parse(rows[4]),
                                Name = rows[5],
                                Address = rows[6],
                                Country = rows[7],
                                IBAN = rows[8],
                                SWIFT = rows[9],
                                Currency = (Currency)Enum.Parse(typeof(Currency), rows[10], true),
                                Title = rows[11],
                                Commission = rows[12],
                                GrecosBank1 = rows[13],
                                GrecosBank2 = rows[14],
                                Category = rows[15],
                                Realisation = rows[16]
                            }) ;
                        }
                    }

                    TempData["MyData"] = Newtonsoft.Json.JsonConvert.SerializeObject(mt);

                    return RedirectToAction("List");
                }
                catch (Exception)
                {

                }
            }

            return View(new ImportModel());
        }

        public IActionResult List()
        {
            List<ImportModel> model = JsonConvert.DeserializeObject<List<ImportModel>>((string)TempData["MyData"]);
            TempData["MyNewData"] = Newtonsoft.Json.JsonConvert.SerializeObject(model);

            return View(model);
        }

        public IActionResult Raport()
        {
            List<ImportModel> model = JsonConvert.DeserializeObject<List<ImportModel>>((string)TempData["MyNewData"]);
            
            List<string> mt103 = new List<string>();

            foreach (var item in model)
            {
                mt103.Add(":32A:" + DateTime.Now.ToString("ddMMyy") + item.Currency + item.Amount.ToString("F", CultureInfo.InvariantCulture));
                mt103.Add(":50:" + "GRECOS HOLIDAY");
                mt103.Add("UL. GRUNWALDZKA 76 A");
                mt103.Add("60-311 POZNAŃ");
                mt103.Add(":52D:" + item.GrecosBank1);
                mt103.Add(item.GrecosBank2);
                mt103.Add("");
                mt103.Add("               " + item.Country + " " + item.Country);
                mt103.Add(":57:" + item.SWIFT);
                mt103.Add(":59:/" + item.IBAN);

                if (item.Name.Length > 35)
                {
                    mt103.Add(item.Name.Substring(0, 35));
                    mt103.Add(item.Name.Substring(35));
                }
                else
                {
                    mt103.Add(item.Name);
                }

                if (item.Address.Length > 35)
                {
                    mt103.Add(item.Address.Substring(0, 35));
                    mt103.Add(item.Address.Substring(35));
                }
                else
                {
                    mt103.Add(item.Address);
                }

                if (item.Title.Length <= 31)
                {
                    mt103.Add(":70:" + item.Title);
                }
                else if (item.Title.Length > 31 && item.Title.Length <= 66) 
                {
                    mt103.Add(":70:" + item.Title.Substring(0,31));
                    mt103.Add(item.Title.Substring(31));
                }

                else if (item.Title.Length > 66 && item.Title.Length <= 101)
                {
                    mt103.Add(":70:" + item.Title.Substring(0, 31));
                    mt103.Add(item.Title.Substring(31, 35));
                    mt103.Add(item.Title.Substring(66));
                }

                else if (item.Title.Length > 101 && item.Title.Length <= 136)
                {
                    mt103.Add(":70:" + item.Title.Substring(0, 31));
                    mt103.Add(item.Title.Substring(31, 35));
                    mt103.Add(item.Title.Substring(66, 35));
                    mt103.Add(item.Title.Substring(101));
                }

                mt103.Add(":71A:" + item.Commission);
                mt103.Add(":72:");
                mt103.Add("\\" + item.Category + "\\");
                mt103.Add("\\" + item.Realisation + "\\");
            }

            var filename = "mt103.pla";
            var path = Path.Combine(_env.WebRootPath, filename);

            TextWriter tw = new StreamWriter(path);

            foreach (var item in mt103)
            {
                tw.WriteLine(item);
            }

            tw.Close();

            var memory = new MemoryStream();
            using (var stream = new FileStream(path, FileMode.Open))
            {
                stream.CopyTo(memory);
            }
            memory.Position = 0;

            return File(memory, "text/csv", Path.GetFileName(path));
        }
    }
}
