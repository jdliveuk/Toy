using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;


//C:\\Users\\John\\LotProcessingSourceFiles\\NAB\\

namespace LotsProcessing
{
    class Program
    {
        static void Main(string[] args)
        {

            DataController datacontroller = new DataController();
            //Use Datacontroller to import CSVs to LIst of Trades.

            List<Income> incomeData = datacontroller.CSVtoListOfIncome(FilePaths.IB_Dividends_Interest);
            List<Income> NABincomeData = datacontroller.CSVtoListOfIncome(FilePaths.NAB_Dividends_Interest);
            incomeData.AddRange(NABincomeData);
            //filter for USD and create USD Income Tax trades
            List<Income> incomeDatasubset = incomeData.Where(p => p.CCY == "USD").ToList();
            List<FX> incomeFXTrades = new List<FX>();
            foreach (Income income in incomeDatasubset)
            {
                incomeFXTrades.Add(new FX(income.TradeDate, "AUD.USD", income.Amount, "BUY", income.FXRate, 0, -1 * income.Amount_AUD, 0, 0, "AUD", 1, 0, "Income"));
                
            }

            List<Trade> tradeData = datacontroller.CSVtoListOfTrades(FilePaths.NAB);
            List<Trade> IBtradeData = datacontroller.CSVtoListOfTrades(FilePaths.IB);
            List<Trade> ANZtradeData = datacontroller.CSVtoListOfTrades(FilePaths.ANZ);
            //exlude options for now
            //IBtradeData = IBtradeData.Where(p => p.ObjType != "LotsProcessing.Option").ToList();


            //join the trade lists into one
            tradeData.AddRange(IBtradeData);
            tradeData.AddRange(ANZtradeData);
            //Sort the trade list by TradeDate
            tradeData.Sort((x, y) => x.TradeDate.CompareTo(y.TradeDate));
            //Obtain distinct list of StockCodes
            string[] stockCodeList = tradeData.Select(x => x.StockCode).Distinct().ToArray();
            List<TaxReport> taxReports = new List<TaxReport>();
            //create a taxreport for each stock code, do FX trades separately
            foreach (string s in stockCodeList)
            {
                if (s != "AUD.USD" && s != "AUD.JPY")
                //if (s != "AUD.USD")
                {
                    //filter tradeData by stockcode and create Tax report object
                    List<Trade> tradeSubset = tradeData.Where(p => p.StockCode == s).ToList();
                    //create tax report and add to tax report list
                    taxReports.Add(new TaxReport(tradeSubset, s, true));
                }
            }

            //filter for FX trades and add to FX_taxtrades and create tax report for the FX trades
            List<Trade> FXtradeSubset = tradeData.Where(p => p.StockCode == "AUD.USD" || p.StockCode == "AUD.JPY").ToList();
            //List<Trade> FXtradeSubset = tradeData.Where(p => p.StockCode == "AUD.USD").ToList();
            //Loop through all other taxReports and add FX_taxTrades to FXtradeSubset
            foreach (TaxReport taxReport in taxReports)
            {
                FXtradeSubset.AddRange(taxReport.FX_taxTrades);
            }
            //add IncomeFXTrades
            FXtradeSubset.AddRange(incomeFXTrades);
            //sort by trade date
            FXtradeSubset.Sort((x, y) => x.TradeDate.CompareTo(y.TradeDate));
            //create tax reports for FX trades.
            List<Trade> USD_FXTradeSubset = FXtradeSubset.Where(p => p.StockCode == "AUD.USD").ToList();
            taxReports.Add(new TaxReport(USD_FXTradeSubset, "AUD.USD", false));
            List<Trade> JPY_FXTradeSubset = FXtradeSubset.Where(p => p.StockCode == "AUD.JPY").ToList();
            taxReports.Add(new TaxReport(JPY_FXTradeSubset, "AUD.JPY", false));

            //output reports
            IncomeByTaxYear IncomeReport = new IncomeByTaxYear("IncomeReport.csv");
            IncomeReport.OutputReport(incomeData.Where(p => p.Amount != 0).ToList());

            PnLByTaxYear PnLReport = new PnLByTaxYear("DetailedTaxReport.csv");
            PnLReport.OutputReport(taxReports);
           
        }

        static void outputSummaryTaxReport(List<TaxReport> taxReports) //currently not used
        {
            StreamWriter file = new StreamWriter("C:\\Temp\\SummaryTaxReport.csv");
            foreach (TaxReport taxReport in taxReports)
            {
                file.Write($"Tax Report for:, {taxReport.Name}");
                file.Write("\n");
                file.Write("Tax Lot Number, Tax Lot Name, Opening Balance, Outstanding Balance, Lot Close Date, Profit/(Loss), Period Length, Object Type");
                file.Write("\n");
                foreach (TaxLot taxlot in taxReport.taxLots)
                {
                    file.Write($"{taxReport.taxLots.IndexOf(taxlot)}, {taxlot.Name}, {taxlot.OpeningBalance}, {taxlot.getOutstandingBalance()}, {taxlot.getLotCloseDate().Date.ToShortDateString()}, {taxlot.getPnL()}, {taxlot.getTaxPnL()}, {taxlot.getlotPeriodLength()} ");
                    file.Write("\n");
                }
            }
            file.Close();
        }

        static void OutputIncomeReport(List<Income> incomeData)
        {
            StreamWriter file = new StreamWriter("C:\\Temp\\IncomeReport.csv");
            file.Write($"Income Report for:");
            file.Write("\n");
            file.Write("Trade Date, Tax Year, Income Type, Stock Code, Ccy, Amount, Amount $A");
            file.Write("\n");
            foreach (Income income in incomeData)
            {
                file.Write($"{income.TradeDate.ToShortDateString()}, {income.TaxYear}, {income.IncomeType}, {income.StockCode}, {income.CCY}, {income.Amount}, {income.Amount_AUD} ");
                file.Write("\n");
            }
            file.Close();
        }

        static void outputDetailedTaxReport(List<TaxReport> taxReports)
        {
            StreamWriter file = new StreamWriter("C:\\Temp\\DetailedTaxReport.csv");
            file.Write("Tax Lot Name, Tax Lot Number, Trade Date, Stock Code, CCY, Quantity, Action, Price, Cost, PnL, FXRate, AUD PnL, Tax PnL AUD, Tax Year, Holding Length, Trade Type");
            file.Write("\n");
            foreach (TaxReport taxReport in taxReports)
            {
                foreach (TaxLot taxlot in taxReport.taxLots)
                {
                    IEnumerable<string> tradesTexts = taxlot.Trades.Select(p => String.Join(",", taxlot.Name, taxReport.taxLots.IndexOf(taxlot), p.TradeDate.Date.ToShortDateString(), p.StockCode, p.CCY, p.Quantity, p.Action, p.Price, p.Cost, p.TradeLocalPnl, p.FXRate, p.Trade_AUD_PnL, p.Trade_AUD_TaxPnl, p.TaxYear, p.TradeLength, p.ObjType));
                    string joined = String.Join(Environment.NewLine, tradesTexts);
                    file.Write(joined);
                    file.Write("\n");
                }
            }
            file.Close();
        }
    }
}