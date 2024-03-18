using System.Collections.Generic;
using System.Linq;
using Andrew.DiscountDemo.Core;

namespace Andrew.DiscountDemo.DiscountRules
{
    public class BuyMoreBoxesDiscountRule : DiscountRuleBase
    {
        public readonly int BoxCount = 0;
        public readonly int PercentOff = 0;

        public BuyMoreBoxesDiscountRule(int boxes, int percentOff)
        {
            BoxCount = boxes;
            PercentOff = percentOff;

            Name = $"任 {BoxCount} 箱結帳 {100 - PercentOff} 折!";
            Note = "熱銷飲品 限時優惠";
        }

        public override IEnumerable<Discount> Process(CartContext cart)
        {
            List<Product> matched_products = new List<Product>();

            foreach (var p in cart.GetVisiblePurchasedItems(ExclusiveTag))
            {
                matched_products.Add(p);

                if (matched_products.Count == BoxCount)
                {
                    // 符合折扣
                    yield return new Discount()
                    {
                        Amount = matched_products.Select(p => p.Price).Sum() * PercentOff / 100,
                        Products = matched_products.ToArray(),
                        Rule = this,
                    };
                    matched_products.Clear();
                }
            }
        }
    }
}
