using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace LotsProcessing
{



    class IncomeByTaxYear : Report
    {


        public IncomeByTaxYear(string fileName)
        {
            outputStream = new StreamWriter(OutPutDirectory + fileName);
        }
        
        public void OutputReport(List<Income> incomeData)
        {
            outputStream.Write($"Income Report for:");
            outputStream.Write("\n");
            outputStream.Write("Trade Date, Tax Year, Source, Income Type, Stock Code, Ccy, Amount, Amount $A");
            outputStream.Write("\n");
            foreach (Income income in incomeData)
            {
                outputStream.Write($"{income.TradeDate.ToShortDateString()}, {income.TaxYear}, {income.Source}, {income.IncomeType}, {income.StockCode}, {income.CCY}, {income.Amount}, {income.Amount_AUD} ");
                outputStream.Write("\n");
            }
            outputStream.Close();
        }
    }
}
