using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LotsProcessing;

namespace LotsProcessing
{
    class Trade
    {
        public DateTime TradeDate;
        public string StockCode;
        public string CCY;
        public decimal Quantity;
        public string Action;
        public string TaxAction;
        public decimal Price;
        public decimal Fees;
        public decimal Cost;
        public decimal CostAUD;
        public decimal FXRate;
        public string ObjType;
        public int TaxYear;
        public decimal TradeLocalPnl;
        public decimal Trade_AUD_PnL;
        public decimal Trade_AUD_TaxPnl;
        public decimal TradeLength;
        public string Source;

        //constructor
        public Trade(DateTime tradeDate, string stockCode, decimal quantity, string action, decimal price, decimal fees, decimal cost, decimal pnl, decimal tradeLength, string CCY, decimal fxrate, decimal taxpnl, string source)
        {
            this.TradeDate = tradeDate;
            this.StockCode = stockCode;
            this.Quantity = quantity;
            this.Action = action;
            this.Price = price;
            this.Fees = fees;
            this.Cost = cost;
            this.CostAUD = cost / fxrate;
            this.TradeLocalPnl = pnl;
            this.Trade_AUD_PnL = pnl / fxrate;
            this.Trade_AUD_TaxPnl = taxpnl;
            this.TaxYear = SetTaxYear(tradeDate);
            this.TradeLength = tradeLength;
            this.CCY = CCY;
            this.FXRate = fxrate;
            this.ObjType = this.GetType().ToString();
            this.Source = source;

        }
        //Blank constructor
        public Trade()
        {
            ObjType = this.GetType().ToString();
        }

        public static int SetTaxYear(DateTime tradeDate)
        {
            int taxyear = 0;
            if (tradeDate <= new DateTime(2017, 6, 30))
            {
                taxyear = 2017;
            }
            else if (tradeDate > new DateTime(2017, 6, 30) && tradeDate <= new DateTime(2018, 6, 30))
            {
                taxyear = 2018;
            }
            else if (tradeDate > new DateTime(2018, 6, 30) && tradeDate <= new DateTime(2019, 6, 30))
            {
                taxyear = 2019;
            }
            else if (tradeDate > new DateTime(2019, 6, 30) && tradeDate <= new DateTime(2020, 6, 30))
            {
                taxyear = 2020;
            }
            else if (tradeDate > new DateTime(2020, 6, 30) && tradeDate <= new DateTime(2021, 6, 30))
            {
                taxyear = 2021;
            }
            else if (tradeDate > new DateTime(2021, 6, 30) && tradeDate <= new DateTime(2022, 6, 30))
            {
                taxyear = 2022;
            }
            else if (tradeDate > new DateTime(2022, 6, 30))
            {
                taxyear = 2023;
            }
            return taxyear;
        }
    }
}