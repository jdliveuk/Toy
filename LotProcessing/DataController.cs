using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LotsProcessing
{
    class FilePaths
    {
        //work
        //public const string sourceDirectory = "C:\\Users\\jchristof\\LotsProcessingSourceFiles\\NAB\\";
        //home
        public const string sourceDirectory = "C:\\Users\\John\\LotProcessingSourceFiles\\NAB\\";
        public const string NAB = sourceDirectory + "Confirmation Notes 20200107112232.csv";
        public const string IB = sourceDirectory + "IB_Trade_Activity.csv";
        public const string ANZ = sourceDirectory + "ANZ_Trade_Activity.csv";
        public const string FxRates = sourceDirectory + "FxRates.csv";
        public const string JPYFxRates = sourceDirectory + "JPY_FxRates.csv";
        public const string IB_Dividends_Interest = sourceDirectory + "IB_Divs_Int_WHTax.csv";
        public const string NAB_Dividends_Interest = sourceDirectory + "NAB_Cash Transactions_DIvsInt.csv";
    }
    class DataController
    {
        public Dictionary<DateTime, decimal> FXRatesByDate;
        public Dictionary<DateTime, decimal> JPY_FXRatesByDate;
        //constructor
        public DataController()
        {
            //Load in FX rates to dictionary field
            FXRatesByDate = File.ReadAllLines(FilePaths.FxRates).Skip(1).Select(line => line.Split(',')).ToDictionary(line => Convert.ToDateTime(line[0]), line => Convert.ToDecimal(line[1]));
            JPY_FXRatesByDate = File.ReadAllLines(FilePaths.JPYFxRates).Skip(1).Select(line => line.Split(',')).ToDictionary(line => Convert.ToDateTime(line[0]), line => Convert.ToDecimal(line[1]));

        }
        public List<Trade> CSVtoListOfTrades(string filepath)
        {
            List<Trade> data = new List<Trade>();
            switch (filepath)
            {
                case FilePaths.NAB:
                    data = File.ReadAllLines(filepath)
                                                   .Skip(1)
                                                   .Select(v => FromNABCsv(v, FXRatesByDate))
                                                   .ToList();
                    break;
                case FilePaths.IB:
                    data = File.ReadAllLines(filepath)
                                                   .Skip(1)
                                                   .Select(v => FromIBCsv(v, FXRatesByDate, JPY_FXRatesByDate))
                                                   .ToList();
                    break;

                case FilePaths.ANZ:
                    data = File.ReadAllLines(filepath)
                                                   .Skip(1)
                                                   .Select(v => FromANZCsv(v, FXRatesByDate))
                                                   .ToList();
                    break;
            }
            return data;

        }

        public List<Income> CSVtoListOfIncome(string filepath)
        {
            List<Income> data = new List<Income>();
            switch (filepath)
            {

                case FilePaths.IB_Dividends_Interest:


                    foreach (var line in File.ReadLines(filepath))
                    {
                        string[] values = line.Split(',');
                        if (values[1] == "Data" && values[4] != "")
                        {
                            data.Add(csvLinetoIncome(values[2], Convert.ToDateTime(values[3]).Date, Convert.ToDecimal(values[5]), values[4], values[0], FXRatesByDate, "IB"));

                        }

                    }

                   
                    break;

                case FilePaths.NAB_Dividends_Interest:
                    foreach (var line in File.ReadLines(filepath))
                    {
                        string[] values = line.Split(',');
                        if (values[2].Contains("DIV") || values[2].Contains(" GR "))
                        {
                            data.Add(csvLinetoIncome("AUD", Convert.ToDateTime(values[0]).Date, Convert.ToDecimal(values[4]), values[2], "Dividends", FXRatesByDate, "NAB"));

                        }
                        else if (values[2] == "INTEREST")
                        {
                            data.Add(csvLinetoIncome("AUD", Convert.ToDateTime(values[0]).Date, Convert.ToDecimal(values[4]), values[2], "Interest", FXRatesByDate, "NAB"));
                        }
                    }
                    break;

            }
            return data;

        }



        public static Trade FromNABCsv(string csvLine, Dictionary<DateTime, decimal> fxrates)
        {
            string[] values = csvLine.Split(',');
            string action = values[4];
            int quantity = Convert.ToInt32(values[3]);
            decimal cost = Convert.ToDecimal(values[7]);
            if (action == "Sell") //for a sell trade make the quantity negative
            {
                quantity = quantity * -1;
            }
            else //for a buy trade make the cost negative
            {
                cost = cost * -1;
            }

            //all trades in NAB are Equities
            Equity trade = new Equity(Convert.ToDateTime(values[0]), values[2], quantity, action, Convert.ToDecimal(values[5]), Convert.ToDecimal(values[6]), cost, 0, 0, "AUD", 1, 0, "NAB");
            return trade;
        }

        public static Trade FromANZCsv(string csvLine, Dictionary<DateTime, decimal> fxrates)
        {
            string[] values = csvLine.Split(',');
            string action = values[3];
            int quantity = Convert.ToInt32(values[7]);
            decimal cost = Convert.ToDecimal(values[15]);
            if (action == "Sell") //for a sell trade make the quantity negative
            {
                quantity = quantity * -1;
            }
            else //for a buy trade make the cost negative
            {
                cost = cost * -1;
            }

            //all trades in NAB are Equities
            Equity trade = new Equity(Convert.ToDateTime(values[4]), values[1] + ".ANZ", quantity, action, Convert.ToDecimal(values[6]), Convert.ToDecimal(values[10]), cost, 0, 0, "AUD", 1, 0,"ANZ");
            return trade;
        }
        public static Trade FromIBCsv(string csvLine, Dictionary<DateTime, decimal> fxrates, Dictionary<DateTime, decimal> jpyfxrates)
        {
            string[] values = csvLine.Split(',');
            //set fx rate
            decimal TradeFXRate;
            if (values[8] == "USD")
            {
                TradeFXRate = fxrates[Convert.ToDateTime(values[1]).Date];
            }
            else if (values[8] == "JPY")
            {
                TradeFXRate = jpyfxrates[Convert.ToDateTime(values[1]).Date];
            }
            else
            {
                TradeFXRate = 1;
            }
            string action = values[2];
            decimal cost = Convert.ToDecimal(values[6]);
            //Create a new trade type with respective type
            Trade trade;
            switch (values[9])
            {
                case "STK":
                    trade = new Equity(Convert.ToDateTime(values[1]), values[0], Convert.ToInt32(values[3]), action, Convert.ToDecimal(values[4]), Convert.ToDecimal(values[7]), cost, 0, 0, values[8], TradeFXRate, 0, "IB");
                    break;
                case "CASH":
                    //swap buy and sell
                    int quantity = Convert.ToInt32(Math.Round(Convert.ToDecimal(values[6]), 0));
                    cost = Convert.ToDecimal(values[3]);
                    //all FX trades are in AUD and the PnL is calculated on AUD amount so set FXrate to 1
                    string tradeCCY = "AUD";
                    TradeFXRate = 1;
                    if (action == "BUY")
                    {
                        action = "SELL";
                    }
                    else
                    {
                        action = "BUY";
                    }
                    trade = new FX(Convert.ToDateTime(values[1]), values[0], quantity, action, Convert.ToDecimal(values[4]), Convert.ToDecimal(values[7]), cost, 0, 0, tradeCCY, TradeFXRate, 0, "IB");
                    break;
                case "OPT":
                    trade = new Option(Convert.ToDateTime(values[1]), values[0], Convert.ToInt32(values[3]), action, Convert.ToDecimal(values[4]), Convert.ToDecimal(values[7]), cost, 0, 0, values[8], TradeFXRate, 0, "IB");
                    break;

                default:
                    //return blank trade;
                    trade = new Trade();
                    break;
            }
            return trade;
        }


        public static Income csvLinetoIncome(string ccy, DateTime tradeDate, decimal amount, string stockCodeString, string incomeType, Dictionary<DateTime, decimal> fxrates, string source)
        {
            Income income;
                decimal tradeFXRate;
                if (ccy == "USD")
                {
                    tradeFXRate = fxrates[tradeDate];
                }
                else
                {
                    tradeFXRate = 1;
                }
                //string action = values[2];
                //decimal amount = Convert.ToDecimal(values[5]);
                //Create a new trade type with respective type


                string[] strArray = stockCodeString.Split('(');

                //Dividend dividend;
                switch (incomeType)
                {
                    case "Dividends":


                        string stockcode = strArray[0];
                        income = new Dividend(tradeDate, stockcode, ccy, tradeFXRate, amount, source);
                        break;

                    case "Interest":


                        //string stockcode = strArray[0];
                        income = new Interest(tradeDate, "INT", ccy, tradeFXRate, amount, source);
                        break;

                    default:
                        //return blank dividend;
                        income = new Dividend();
                        break;
                }
            
            
            return income;
        }


        public static Income FromIB_DivInt_csv(string csvLine, Dictionary<DateTime, decimal> fxrates)
        {
            string[] values = csvLine.Split(',');
            //set fx rate
            //Dividend dividend = new Dividend();
            Income income;

            if (values[1] == "Data" && values[4] != "")
            {

                decimal tradeFXRate;
                if (values[2] == "USD")
                {
                    tradeFXRate = fxrates[Convert.ToDateTime(values[3]).Date];
                }
                else
                {
                    tradeFXRate = 1;
                }
                //string action = values[2];
                decimal amount = Convert.ToDecimal(values[5]);
                //Create a new trade type with respective type


                string[] strArray = values[4].Split('(');

                //Dividend dividend;
                switch (values[0])
                {
                    case "Dividends":


                        string stockcode = strArray[0];
                        income = new Dividend(Convert.ToDateTime(values[3]), stockcode, values[2], tradeFXRate, Convert.ToDecimal(values[5]), "IB");
                        break;

                    case "Interest":


                        //string stockcode = strArray[0];
                        income = new Dividend(Convert.ToDateTime(values[3]), "INT", values[2], tradeFXRate, Convert.ToDecimal(values[5]), "IB");
                        break;

                    default:
                        //return blank dividend;
                        income = new Dividend();
                        break;
                }
            }
            else
            {
                //I need this otherwise the compiler does not like it, but it is not good coding.
                income = new Dividend();
            }
            return income;
        }
    }
}