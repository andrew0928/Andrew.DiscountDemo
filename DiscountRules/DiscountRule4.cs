using System.Collections.Generic;
using System.Linq;
using Andrew.DiscountDemo.Core;

namespace Andrew.DiscountDemo.DiscountRules
{
    public class DiscountRule4 : DiscountRuleBase
    {
        private string TargetTag;
        private decimal DiscountAmount;

        public DiscountRule4(string tag, decimal amount, string exclusiveTag = null)
        {
            Name = "同商品加購優惠";
            Note = $"加{amount}元多一件";
            TargetTag = tag;
            DiscountAmount = amount;
            ExclusiveTag = exclusiveTag;
        }
        public override IEnumerable<Discount> Process(CartContext cart)
        {
            List<LineItem> matched = new List<LineItem>();
            foreach (var sku in cart.GetVisibleLineItems(ExclusiveTag).Where(p => p.Tags.Contains(TargetTag)).Select(p => p.Product.SKU).Distinct())
            {
                matched.Clear();
                foreach (var p in cart.GetVisibleLineItems(ExclusiveTag).Where(p => p.Product.SKU == sku))
                {
                    matched.Add(p);
                    if (matched.Count == 2)
                    {
                        yield return new Discount()
                        {
                            LineItems = matched.ToArray(),
                            Amount = DiscountAmount,
                            Rule = this
                        };
                        matched.Clear();
                    }
                }
            }
        }
    }
}
