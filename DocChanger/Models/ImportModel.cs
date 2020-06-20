using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DocChanger.Models
{
    public class ImportModel
    {
        public string Dest { get; set; }
        public string HotelCode { get; set; }
        public string Hotel { get; set; }
        public float Amount { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Country { get; set; }
        public string IBAN { get; set; }
        public string SWIFT { get; set; }
        public Currency Currency { get; set; }
        public string Title { get; set; }
        public string Commission { get; set; }
        public string GrecosBank1 { get; set; }
        public string GrecosBank2 { get; set; }
        public string Category { get; set; }
        public string Realisation { get; set; }

        //public override string ToString()
        //{
        //    return $"Dest {Dest}: HotelCode: {HotelCode}: Hotel: {Hotel}, Amount: {Amount}, Date: {Date}, Name: {Name}, Address: {Address}, IBAN: {IBAN}, SWIFT: {SWIFT}, Currency: {Currency}";
        //}

    }
}
