using System.Collections.Generic;
using System.Linq;
using Andrew.DiscountDemo.Core;

namespace Andrew.DiscountDemo.DiscountRules
{
    public class DiscountRule3 : DiscountRuleBase
    {
        private string TargetTag;
        private int PercentOff;
        public DiscountRule3(string targetTag, int percentOff)
        {
            Name = "滿件折扣3";
            Note = $"{targetTag}第二件{10 - percentOff / 10}折";

            TargetTag = targetTag;
            PercentOff = percentOff;
        }
        public override IEnumerable<Discount> Process(CartContext cart)
        {
            List<LineItem> matched = new List<LineItem>();
            foreach (var p in cart.GetVisibleLineItems(ExclusiveTag).Where(p => p.Tags.Contains(TargetTag)))
            {
                matched.Add(p);
                if (matched.Count == 2)
                {
                    yield return new Discount()
                    {
                        Amount = p.CheckoutPrice * PercentOff / 100,
                        LineItems = matched.ToArray(),
                        Rule = this
                    };
                    matched.Clear();
                }
            }
        }
    }
}
