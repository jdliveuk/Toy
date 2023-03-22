using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LotsProcessing
{
    class TaxReport
    {
        public string Name;
        public List<TaxLot> taxLots = new List<TaxLot>();

        /*Logic of tax trades is as follows:
            1. The tax Pnl of equity trades is in AUD, therefore it effectively captures any exchange rate movements
            2. Therefore when I buy a USD stock I must already have USD, therefore for every buy of a USD stock there is a sell of USD
            3. And vice versa, when I sell a USD stock, I buy USD. 
            4. Therefore I need to record these sells and buys every time there is an equity trade, and save it in FX Tax Trades
            5. The FX tax trades is then combined with normal FX trades, and then we run a tax report on ALL FX tax trades
        */
        public List<Trade> FX_taxTrades = new List<Trade>();

        //public TaxReport(List<Trade> trades, string name, ref List<Trade> FX_taxTrades)
        public TaxReport(List<Trade> trades, string name, bool createFX_TaxTrades = false)
        {
            /*filter into buys and sells
             * for each buy create a new lot and a respective FX tax trade
             * for each sell add the trade to the relevant lot. Split the trade if necessary. Create respespective tax trades
             * The lot class with have methods the calculate the total pnl so far, and also the remaining balance.
             * All lots will be stored in a list.
             * */
            Name = name;

            //filter out the BUY trades
            List<Trade> buyTrades = trades.Where(p => p.Action.ToUpper() == "BUY").ToList();
            //Create a tax lot for each and add to a list of TaxLots
            foreach (Trade trade in buyTrades)
            {
                taxLots.Add(new TaxLot(trade.TradeDate, trade));

                //add a new FXTaxTrade if it is a USD equity
                if (createFX_TaxTrades)
                {
                    //Check for only USD and JPY trades and where the the cost was not equal to zero as there is no fX trade if there is no cost
                    if (trade.CCY == "USD" && trade.Cost!=0)
                    {
                        this.FX_taxTrades.Add(new FX(trade.TradeDate, "AUD.USD", trade.Cost, "SELL", trade.FXRate, 0, -1 * trade.CostAUD, 0, 0, "AUD", 1, 0, trade.Source + " FXTaxTrade "));
                    }
                    else if (trade.CCY == "JPY" && trade.Cost != 0)
                    {
                        this.FX_taxTrades.Add(new FX(trade.TradeDate, "AUD.JPY", trade.Cost, "SELL", trade.FXRate, 0, -1 * trade.CostAUD, 0, 0, "AUD", 1, 0, trade.Source + " FXTaxTrade "));
                    }
                }
            }

            //Filter for Sell trades and then loop through trades and apply to tax lots. Currently assumes a tax lot can only be opened with a buy.
            List<Trade> sellTrades = trades.Where(p => p.Action.ToUpper() == "SELL").ToList();
            bool partialTrade = false;
            foreach (Trade trade in sellTrades)
            {
                //loop through tax lots
                
                foreach (TaxLot taxlot in this.taxLots)
                {
                    decimal osBalance = taxlot.getOutstandingBalance();
                    if (osBalance > 0)
                    {
                        //add the trade to the tax lot, check the size of the trade to the amount of outstanding in the tax lot
                        if (trade.Quantity * -1 < osBalance)
                        {
                            //add the trade Pnl and Pnl Date
                            trade.TradeLocalPnl = trade.Cost - trade.Quantity * taxlot.UnitCost;
                            trade.Trade_AUD_PnL = trade.TradeLocalPnl / trade.FXRate;
                            trade.Trade_AUD_TaxPnl = trade.CostAUD - trade.Quantity * taxlot.UnitCostAUD;
                            trade.TradeLength = Convert.ToDecimal((trade.TradeDate - taxlot.LotStartDate).TotalDays / 365);
                            if (trade.TradeLength > 1 && trade.Trade_AUD_TaxPnl > 0)// if holding length is greater than 1, reduce taxable profits by half
                            {
                                trade.Trade_AUD_TaxPnl = trade.Trade_AUD_TaxPnl / 2;
                            }
                            

                            //trade.Trade_AUD_TaxPnl = trade.CostAUD - trade.Quantity * taxlot.UnitCostAUD;
                            trade.TaxYear = Trade.SetTaxYear(trade.TradeDate);
                            //trade.TradeLength = Convert.ToDecimal((trade.TradeDate - taxlot.LotStartDate).TotalDays / 365);
                            if (partialTrade)
                            {
                                trade.Source = trade.Source + " Partial LotTrade_" + trade.TradeDate.Date.ToString("yyyyMMdd") + "_" + trade.StockCode + "_" + trade.Action + "_" + trade.Quantity; 
                            }
                            taxlot.AddWholeTrade(trade);

                            //create a new tax trade if USD or JPY equal to opposite sign of cost and cost AUD
                            if (createFX_TaxTrades)
                            {
                                if (trade.CCY == "USD" && trade.Cost != 0)
                                {
                                    this.FX_taxTrades.Add(new FX(trade.TradeDate, "AUD.USD", trade.Cost, "BUY", trade.FXRate, 0, -1 * trade.CostAUD, 0, 0, "AUD", 1, 0, trade.Source + " FXTaxTrade Whole "));
                                }
                                else if (trade.CCY == "JPY" && trade.Cost != 0)
                                {
                                    this.FX_taxTrades.Add(new FX(trade.TradeDate, "AUD.JPY", trade.Cost, "BUY", trade.FXRate, 0, -1 * trade.CostAUD, 0, 0, "AUD", 1, 0, trade.Source + " FXTaxTrade Whole "));
                                }
                            }
                            break;
                        }
                        else
                        {
                            //for the Balance of the remaining tax lot we create a NEW trade and add it to the tax lot to take it to zero, then
                            //we amend the existing trade and keep looping.
                            //create a new trade
                            double ratio = -1 * Convert.ToDouble(Convert.ToDouble(osBalance) / Convert.ToDouble(trade.Quantity));
                            decimal newCost = Convert.ToDecimal(Convert.ToDouble(trade.Cost) * ratio);
                            decimal newAUDCost = Convert.ToDecimal(Convert.ToDouble(trade.CostAUD) * ratio);
                            //* (osBalance / trade.Quantity);
                            decimal remainingCost = trade.Cost - newCost;
                            decimal remainingAUDcost = trade.CostAUD - newAUDCost;
                            decimal pnl = newCost - (-1 * osBalance * taxlot.UnitCost);
                            decimal taxpnl = newAUDCost - (-1 * osBalance * taxlot.UnitCostAUD);
                            decimal tradeLength = Convert.ToDecimal((trade.TradeDate - taxlot.LotStartDate).TotalDays / 365);
                            if (tradeLength > 1 && taxpnl > 0)// if holding length is greater than 1, reduce taxable profits by half
                            {
                                taxpnl = taxpnl / 2;
                            }


                            Trade newtrade;
                            switch (trade.ObjType)
                            {
                                case "LotsProcessing.Equity":
                                    newtrade = new Equity(trade.TradeDate, trade.StockCode, -1 * osBalance, trade.Action, trade.Price, trade.Fees, newCost, pnl, tradeLength, trade.CCY, trade.FXRate, taxpnl, trade.Source + " Partial LotTrade ");
                                    break;

                                case "LotsProcessing.FX":
                                    newtrade = new FX(trade.TradeDate, trade.StockCode, -1 * osBalance, trade.Action, trade.Price, trade.Fees, newCost, pnl, tradeLength, trade.CCY, trade.FXRate, taxpnl, trade.Source + " Partial LotTrade ");
                                    break;
                                case "LotsProcessing.Option":
                                    newtrade = new Option(trade.TradeDate, trade.StockCode, -1 * osBalance, trade.Action, trade.Price, trade.Fees, newCost, pnl, tradeLength, trade.CCY, trade.FXRate, taxpnl, trade.Source + " Partial LotTrade ");
                                    break;

                                default:
                                    newtrade = new Trade(trade.TradeDate, trade.StockCode, -1 * osBalance, trade.Action, trade.Price, trade.Fees, newCost, pnl, tradeLength, trade.CCY, trade.FXRate, taxpnl, trade.Source + " Partial LotTrade ");
                                    break;

                            }
                            taxlot.AddWholeTrade(newtrade);
                            //create a new tax trade if USD or JPY, equal to opposite sign of cost and cost AUD
                            if (createFX_TaxTrades)
                            {
                                if (trade.CCY == "USD" && trade.Cost != 0)
                                {
                                    FX newFXTaxTrade = new FX(newtrade.TradeDate, "AUD.USD",newtrade.Cost, "BUY", newtrade.FXRate, 0, -1 * newtrade.CostAUD, 0, 0, "AUD", 1, 0, trade.Source + " FXTaxTrade Partial ");
                                    this.FX_taxTrades.Add(newFXTaxTrade);
                                }
                                else if (trade.CCY == "JPY" && trade.Cost != 0)
                                {
                                    FX newFXTaxTrade = new FX(newtrade.TradeDate, "AUD.JPY", newtrade.Cost, "BUY", newtrade.FXRate, 0, -1 * newtrade.CostAUD, 0, 0, "AUD", 1, 0, trade.Source + " FXTaxTrade Partial ");
                                    this.FX_taxTrades.Add(newFXTaxTrade);
                                }
                            }

                            //Amend the existing trade in the loop and keep looping unless trade quantity = 0
                            trade.Quantity = trade.Quantity + osBalance;
                            trade.Cost = remainingCost;
                            trade.CostAUD = remainingAUDcost;
                            partialTrade = true;
                            if (trade.Quantity == 0) break;
                        }
                    }
                }
            }
        }
    }
}