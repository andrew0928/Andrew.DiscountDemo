using System;
using System.Collections.Generic;
using System.Linq;
using Andrew.DiscountDemo.DiscountRules;
using Andrew.DiscountDemo.PriceRules;

namespace Andrew.DiscountDemo.Core
{
    public class POS
    {
        public readonly List<DiscountRuleBase> ActivedDiscountRules = new List<DiscountRuleBase>();
        public readonly List<PriceRuleBase> ActivedPriceRules = new List<PriceRuleBase>();

        public bool CheckoutProcess(CartContext cart)
        {
            // reset cart
            cart.AppliedDiscounts.Clear();

            foreach(var rule in ActivedPriceRules)
            {
                var prices = rule.Process(cart);
                cart.AppliedSetPrices.AddRange(prices);
                foreach (var p in prices)
                {
                    var item = cart.LineItems.FirstOrDefault(x => x.Id == p.LineItemId);
                    if (item != null)
                    {
                        item.CheckoutPrice = p.NewCheckoutPrice;
                        p.AppendTags.ToList().ForEach(x => item.Tags.Add(x));
                    }
                }
            }

            cart.TotalPrice = //cart.PurchasedItems.Select(p => p.Price).Sum();
                cart.LineItems.Select(p => p.CheckoutPrice).Sum();

            foreach (var rule in ActivedDiscountRules)
            {
                var discounts = rule.Process(cart);
                cart.AppliedDiscounts.AddRange(discounts);
                if (rule.ExclusiveTag != null)
                {
                    foreach (var d in discounts)
                    {
                        foreach (var p in d.LineItems) p.Tags.Add(rule.ExclusiveTag);
                    }
                }
                cart.TotalPrice -= discounts.Select(d => d.Amount).Sum();
            }
            return true;
        }
    }
}
