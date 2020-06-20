using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DocChanger.Models
{
    public class ImportCsv
    {
        private List<ImportModel> lines = new List<ImportModel>();

        public void Import(string filename)
        {
            try
            {
                using (var fs = new StreamReader(filename)) 
                {
                    lines = new CsvHelper.CsvReader(fs, CultureInfo.InvariantCulture).GetRecords<ImportModel>().ToList();
                }
            }
            
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
