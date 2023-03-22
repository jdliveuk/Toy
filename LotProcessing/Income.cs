using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotsProcessing
{
    abstract class Income
    {

        public DateTime TradeDate;
        public string StockCode;
        public string CCY;

        public decimal FXRate;
        public string ObjType;
        public string Source;
        public string IncomeType;
        


        public int TaxYear;

        public decimal Amount;
        public decimal Amount_AUD;


        public abstract decimal CalcAUDAmount(decimal amount, decimal fXRate);




        //public abstract List<Trade> CalculateTaxTrades();


    }
}
