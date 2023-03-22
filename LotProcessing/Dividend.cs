using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotsProcessing
{
    class Dividend : Income
    {


        public Dividend()
        {
        }

        public Dividend(DateTime tradeDate, string stockCode, string cCY, decimal fXRate, decimal amount, string source)
        {
            this.TradeDate = tradeDate;
            this.StockCode = stockCode;
            this.CCY = cCY;
            this.FXRate = fXRate;
            this.Amount = amount;
            this.Amount_AUD = CalcAUDAmount(amount, fXRate);
            this.TaxYear = Trade.SetTaxYear(tradeDate);
            this.IncomeType = "Dividend";
            this.Source = source;

        }


        public override decimal CalcAUDAmount(decimal amount, decimal fXRate)
        {

            return amount / fXRate;

        }




    }
}