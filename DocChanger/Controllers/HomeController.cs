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
using System.Text;

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


                            //conversion of date format to ddmmyyyy
                            char[] charArray = { '-', '.' };
                            var dateFromFile = rows[4].Split(charArray);
                            var a = dateFromFile[0];
                            var b = dateFromFile[0].Length;

                            if (dateFromFile[0].Length != 2 || dateFromFile[1].Length != 2 || dateFromFile[2].Length != 4)
                            {
                                ViewBag.Data = "Zły format daty - prawidłowy format to DD-MM-YYYY";
                            }

                            var day = Int32.Parse(dateFromFile[0]);
                            var month = Int32.Parse(dateFromFile[1]);
                            var year = Int32.Parse(dateFromFile[2]);
                            var dateConverted = new DateTime(year, month, day);

                            //removes white space from GrecosBank1
                            var GrecosBank1ToTable = rows[13].Trim().Split(' ');
                            var GrecosBank1ToTableLenght = GrecosBank1ToTable.Length;
                            StringBuilder GrecosBank1String = new StringBuilder();

                            for (int i = 0; i < GrecosBank1ToTableLenght; i++)
                            {
                                var s = GrecosBank1ToTable[i];
                                GrecosBank1String.Append(s);
                            }

                            //removes white space from GrecosBank2
                            var GrecosBank2ToTable = rows[14].Trim().Split(' ');
                            var GrecosBank2ToTableLenght = GrecosBank2ToTable.Length;
                            StringBuilder GrecosBank2String = new StringBuilder();

                            for (int i = 0; i < GrecosBank2ToTableLenght; i++)
                            {
                                var s = GrecosBank2ToTable[i];
                                GrecosBank2String.Append(s);
                            }

                            mt.Add(new ImportModel
                            {
                                Dest = rows[0],
                                HotelCode = rows[1],
                                Hotel = rows[2],
                                Amount = float.Parse(rows[3]),
                                Date = dateConverted,
                                Name = rows[5],
                                Address = rows[6],
                                Country = rows[7],
                                IBAN = rows[8],
                                SWIFT = rows[9],
                                Currency = (Currency)Enum.Parse(typeof(Currency), rows[10], true),
                                Title = rows[11],
                                Commission = rows[12],
                                GrecosBank1 = GrecosBank1String.ToString(),
                                GrecosBank2 = GrecosBank2String.ToString(),
                                Category = rows[15],
                                Realisation = rows[16]
                            }) ;
                        }
                    }

                    TempData["MyData"] = Newtonsoft.Json.JsonConvert.SerializeObject(mt);

                    return RedirectToAction("List");
                }
                catch (Exception e)
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

        //Raport MT103 dla banku Millenium
        public IActionResult RaportML()
        {
            try
            {
                List<ImportModel> model = JsonConvert.DeserializeObject<List<ImportModel>>((string)TempData["MyNewData"]);

                List<string> mt103 = new List<string>();

                foreach (var item in model)
                {
                    mt103.Add(":32A:" + item.Date.ToString("yyMMdd") + item.Currency + item.Amount.ToString("F", CultureInfo.CurrentCulture));
                    mt103.Add(":50:" + "GRECOS HOLIDAY");
                    mt103.Add("UL. GRUNWALDZKA 76 A");
                    mt103.Add("60-311 POZNAŃ");
                    mt103.Add(":52D:" + item.GrecosBank1.Substring(2));
                    mt103.Add(item.GrecosBank2.Substring(2));
                    mt103.Add("");
                    mt103.Add("               " + item.Country + " " + item.IBAN.Substring(0, 2));
                    mt103.Add(":57A:" + item.SWIFT);
                    mt103.Add(":59:/" + item.IBAN);

                    if (item.Name.Length > 70)
                    {
                        mt103.Add(item.Name.Substring(0, 35));
                        mt103.Add(item.Name.Substring(35, 35));
                    }
                    else if (item.Name.Length > 35 && item.Name.Length <= 70)
                    {
                        mt103.Add(item.Name.Substring(0, 35));
                        mt103.Add(item.Name.Substring(35));
                    }
                    else
                    {
                        mt103.Add(item.Name);
                    }

                    if (item.Address.Length > 70)
                    {
                        mt103.Add(item.Address.Substring(0, 35));
                        mt103.Add(item.Address.Substring(35, 35));
                    }
                    if (item.Address.Length > 35 && item.Address.Length <= 70)
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
                        mt103.Add(":70:" + item.Title.Substring(0, 31));
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
                    mt103.Add("/" + item.Realisation + "/");
                }

                var filename = "mt103_ML.pla";
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
            catch (Exception e)
            {

            }

            return RedirectToAction("Import");
        }

        //Raport MT103 dla banku ING
        public IActionResult RaportING()
        {
            try
            {
                List<ImportModel> model = JsonConvert.DeserializeObject<List<ImportModel>>((string)TempData["MyNewData"]);

                List<string> mt103 = new List<string>();

                mt103.Add(":04:" + model.FirstOrDefault().GrecosBank1.Substring(4, 8));
                mt103.Add(":05:" + "GRECOS HOLIDAY");
                mt103.Add("UL. GRUNWALDZKA 76 A");
                mt103.Add("60-311 POZNAŃ");

                foreach (var item in model)
                {

                    mt103.Add(":20:2");
                    mt103.Add(":32A:" + item.Date.ToString("yyMMdd") + item.Currency + item.Amount.ToString("F", CultureInfo.CurrentCulture));
                    mt103.Add(":50:" + "GRECOS HOLIDAY");
                    mt103.Add("UL. GRUNWALDZKA 76 A");
                    mt103.Add("60-311 POZNAŃ");
                    mt103.Add(":52D:" + item.GrecosBank1.Substring(2));
                    mt103.Add(item.GrecosBank2.Substring(2));
                    mt103.Add("");
                    mt103.Add("               " + item.Country + " " + item.IBAN.Substring(0, 2));
                    mt103.Add(":57A:" + item.SWIFT);
                    mt103.Add(":57D:");
                    mt103.Add(":59:/" + item.IBAN);

                    if (item.Name.Length > 70)
                    {
                        mt103.Add(item.Name.Substring(0, 35));
                        mt103.Add(item.Name.Substring(35, 35));
                    }
                    else if (item.Name.Length > 35 && item.Name.Length <= 70)
                    {
                        mt103.Add(item.Name.Substring(0, 35));
                        mt103.Add(item.Name.Substring(35));
                    }
                    else
                    {
                        mt103.Add(item.Name);
                    }

                    if (item.Address.Length > 70)
                    {
                        mt103.Add(item.Address.Substring(0, 35));
                        mt103.Add(item.Address.Substring(35, 35));
                    }
                    if (item.Address.Length > 35 && item.Address.Length <= 70)
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
                        mt103.Add(":70:" + item.Title.Substring(0, 31));
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
                    mt103.Add("");
                    mt103.Add("/" + item.Realisation + "/");
                }

                var filename = "mt103_ING.pla";
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

            catch (Exception e)
            {

            }

            return RedirectToAction("Import");
        }

        //Raport MT103 dla banku Santander
        public IActionResult RaportSR()
        {
            try
            {
                List<ImportModel> model = JsonConvert.DeserializeObject<List<ImportModel>>((string)TempData["MyNewData"]);

                List<string> mt103 = new List<string>();

                int x = 0;

                mt103.Add(":01:" + DateTime.Today.ToString("yyyyMMddhhmmss"));
                mt103.Add(":02:" + model.Sum(x => x.Amount).ToString());
                mt103.Add(":03:" + model.Count().ToString());
                mt103.Add(":05:" + "GRECOS HOLIDAY");
                mt103.Add("UL. GRUNWALDZKA 76 A");
                mt103.Add("60-311 POZNAŃ");

                foreach (var item in model)
                {
                    if (x > 0)
                        mt103.Add("$");

                    var swiftLenght = item.SWIFT.Length;
                    string swiftString = null;

                    if (swiftLenght == 11)
                        swiftString = item.SWIFT;
                    else if (swiftLenght == 10)
                        swiftString = item.SWIFT + "X";
                    else if (swiftLenght == 9)
                        swiftString = item.SWIFT + "XX";
                    else if (swiftLenght == 8)
                        swiftString = item.SWIFT + "XXX";
                    else
                        swiftString = null;


                    mt103.Add("{1:F01" + item.GrecosBank1.Substring(4, 8) + "XXXX0001000001}" + "{2:I100" + swiftString + "XN1}" + "{4:");
                    mt103.Add(":20:2");
                    mt103.Add(":32A:" + item.Date.ToString("yyMMdd") + item.Currency + item.Amount.ToString("F", CultureInfo.CurrentCulture));
                    mt103.Add(":50:" + "GRECOS HOLIDAY");
                    mt103.Add("UL. GRUNWALDZKA 76 A");
                    mt103.Add("60-311 POZNAŃ");
                    mt103.Add("POLAND");
                    mt103.Add(":52D:" + item.GrecosBank1.Substring(2));
                    mt103.Add(item.GrecosBank2.Substring(2));
                    mt103.Add("");
                    mt103.Add("               " + item.Country + " " + item.IBAN.Substring(0, 2));
                    mt103.Add(":57A:" + swiftString);
                    mt103.Add(":57D:");
                    mt103.Add(":59:/" + item.IBAN);

                    if (item.Name.Length > 70)
                    {
                        mt103.Add(item.Name.Substring(0, 35));
                        mt103.Add(item.Name.Substring(35, 35));
                    }
                    else if (item.Name.Length > 35 && item.Name.Length <= 70)
                    {
                        mt103.Add(item.Name.Substring(0, 35));
                        mt103.Add(item.Name.Substring(35));
                    }
                    else
                    {
                        mt103.Add(item.Name);
                    }

                    if (item.Address.Length > 70)
                    {
                        mt103.Add(item.Address.Substring(0, 35));
                        mt103.Add(item.Address.Substring(35, 35));
                    }
                    if (item.Address.Length > 35 && item.Address.Length <= 70)
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
                        mt103.Add(":70:" + item.Title.Substring(0, 31));
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
                    mt103.Add("");
                    mt103.Add("/" + item.Realisation + "/");
                    mt103.Add("-}");

                    x++;
                }

                var filename = "mt103_SR.pla";
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
            catch (Exception e)
            {

            }

            return RedirectToAction("Import");
        }

    }
}
