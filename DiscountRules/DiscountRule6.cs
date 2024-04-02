using System.Collections.Generic;
using System.Linq;
using Andrew.DiscountDemo.Core;

namespace Andrew.DiscountDemo.DiscountRules
{
    public class DiscountRule6 : DiscountRuleBase
    {
        private string TargetTag;
        private int PercentOff;
        public DiscountRule6(string targetTag, int percentOff)
        {
            Name = "滿件折扣6";
            Note = $"滿{targetTag}二件結帳{10 - percentOff / 10}折";

            TargetTag = targetTag;
            PercentOff = percentOff;
        }
        public override IEnumerable<Discount> Process(CartContext cart)
        {
            List<LineItem> matched = new List<LineItem>();
            foreach (var p in cart.GetVisibleLineItems(ExclusiveTag).Where(p => p.Tags.Contains(TargetTag)).OrderByDescending(p => p.CheckoutPrice))
            {
                matched.Add(p);
                if (matched.Count == 2)
                {
                    yield return new Discount()
                    {
                        Amount = matched.Sum(p => p.CheckoutPrice) * PercentOff / 100,
                        LineItems = matched.ToArray(),
                        Rule = this
                    };
                    matched.Clear();
                }
            }
        }
    }
}
