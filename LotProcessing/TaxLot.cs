using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotsProcessing
{
    class TaxLot
    {
        public string Name;
        public List<Trade> Trades = new List<Trade>();
        public decimal OpeningBalance;
        public decimal UnitCost;
        public decimal UnitCostAUD;
        public DateTime LotCloseDate;
        public DateTime LotStartDate;
        public TaxLot(DateTime tradeDate, Trade openTrade)
        {
            LotStartDate = tradeDate;
            Name = openTrade.StockCode + "_" + tradeDate.Date.ToString("yyyyMMdd");
            OpeningBalance = openTrade.Quantity;
            UnitCost = openTrade.Cost / openTrade.Quantity;
            UnitCostAUD = openTrade.CostAUD / openTrade.Quantity;
            Trades.Add(openTrade);
        }
        public decimal getOutstandingBalance()
        {
            decimal total = this.Trades.Sum(item => item.Quantity);
            return total;
        }
        public decimal getPnL()
        {
            if (getOutstandingBalance() == 0)
            {
                decimal total = this.Trades.Sum(item => item.Cost);
                return Math.Round(total, 2);
            }
            else
            {
                return 0;
            }

        }

        public decimal getTaxPnL()
        {
            if (getOutstandingBalance() == 0)
            {
                decimal total = this.Trades.Sum(item => item.CostAUD);
                return Math.Round(total, 2);
            }
            else
            {
                return 0;
            }

        }
        public DateTime getLotCloseDate()
        {
            string result = this.Trades.OrderByDescending(t => t.TradeDate).First().TradeDate.ToString();
            DateTime resultDate = DateTime.Parse(result);
            if (getOutstandingBalance() == 0)
            {
                LotCloseDate = resultDate;
                return resultDate.Date;
            }
            else
            {
                return resultDate.Date;
            }

        }
        public double getlotPeriodLength()
        {
            LotCloseDate = getLotCloseDate();
            if (getOutstandingBalance() == 0)
            {
                double days = (LotCloseDate - LotStartDate).TotalDays;
                double years = days / 365;
                return years;
            }
            else
            {
                return 0;
            }
        }

        public void AddWholeTrade(Trade trade)
        {
            this.Trades.Add(trade);

        }


    }
}