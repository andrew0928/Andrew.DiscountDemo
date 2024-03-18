using System.Collections.Generic;
using System.Linq;
using Andrew.DiscountDemo.Core;

namespace Andrew.DiscountDemo.DiscountRules
{
    public class DiscountRule1 : DiscountRuleBase
    {
        private string TargetTag;
        private int MinCount;
        private decimal DiscountAmount;

        public DiscountRule1(string targetTag, int minBuyCount, decimal discountAmount, string exclusiveTag = null)
        {
            Name = "滿件折扣1";
            Note = $"{targetTag}滿{minBuyCount}件折{discountAmount}";
            TargetTag = targetTag;
            MinCount = minBuyCount;
            DiscountAmount = discountAmount;
            ExclusiveTag = exclusiveTag;
        }

        public override IEnumerable<Discount> Process(CartContext cart)
        {
            List<Product> matched = new List<Product>();
            foreach (var p in cart.GetVisiblePurchasedItems(ExclusiveTag).Where(p => p.Tags.Contains(TargetTag)))
            {
                matched.Add(p);
                if (matched.Count == MinCount)
                {
                    yield return new Discount()
                    {
                        Amount = DiscountAmount,
                        Products = matched.ToArray(),
                        Rule = this
                    };
                    matched.Clear();
                }
            }
        }
    }
}
