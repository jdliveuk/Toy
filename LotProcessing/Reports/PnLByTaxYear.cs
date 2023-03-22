using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace LotsProcessing
{
    class PnLByTaxYear : Report
    {
        public PnLByTaxYear(string fileName)
        {
            outputStream = new StreamWriter(OutPutDirectory + fileName);
        }

        public void OutputReport(List<TaxReport> taxReports)
        {
            outputStream.Write("Tax Lot Name, Tax Lot Number, Trade Date, Stock Code, CCY, Quantity, Action, Price, Cost, PnL, FXRate, AUD PnL, Cost AUD, Tax PnL AUD, Tax Year, Holding Length, Trade Type, Source");
            outputStream.Write("\n");
            foreach (TaxReport taxReport in taxReports)
            {
                foreach (TaxLot taxlot in taxReport.taxLots)
                {
                    IEnumerable<string> tradesTexts = taxlot.Trades.Select(p => String.Join(",", taxlot.Name, taxReport.taxLots.IndexOf(taxlot), p.TradeDate.Date.ToShortDateString(), p.StockCode, p.CCY, p.Quantity, p.Action, p.Price, p.Cost, p.TradeLocalPnl, p.FXRate, p.Trade_AUD_PnL, p.CostAUD, p.Trade_AUD_TaxPnl, p.TaxYear, p.TradeLength, p.ObjType, p.Source));
                    string joined = String.Join(Environment.NewLine, tradesTexts);
                    outputStream.Write(joined);
                    outputStream.Write("\n");
                }
            }
            outputStream.Close();

        }
    }
}
