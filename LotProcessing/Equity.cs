using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LotsProcessing;

namespace LotsProcessing
{
    class Equity : Trade
    {

        public Equity()
        {
            ObjType = this.GetType().ToString();
        }
        //public Equity(DateTime tradeDate, string stockCode, int quantity, string action, decimal price, decimal fees, decimal cost, decimal pnl, decimal tradeLength, string CCY, decimal fxrate) : base( tradeDate,  stockCode,  quantity,  action,  price,  fees,  cost,  pnl,  tradeLength,  CCY,  fxrate)
        public Equity(DateTime tradeDate, string stockCode, decimal quantity, string action, decimal price, decimal fees, decimal cost, decimal pnl, decimal tradeLength, string CCY, decimal fxrate, decimal taxpnl, string source)
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
            this.Source = source +"_" + tradeDate.Date.ToString("yyyyMMdd") + "_" + stockCode + "_" + action + "_" + quantity;
        }
    }
}