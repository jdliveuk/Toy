using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace LotsProcessing
{
    class Report
    {
        public string OutPutDirectory = "C:\\Temp\\";
        public StreamWriter outputStream;


        public Report()
        {
            //outputStream = new StreamWriter("C:\\Temp\\IncomeReport.csv");
        }


    }
}
