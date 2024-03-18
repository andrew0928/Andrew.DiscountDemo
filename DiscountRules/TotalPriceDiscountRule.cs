using System.Collections.Generic;
using Andrew.DiscountDemo.Core;

namespace Andrew.DiscountDemo.DiscountRules
{
    public class TotalPriceDiscountRule : DiscountRuleBase
    {
        public readonly decimal MinDiscountPrice = 0;
        public readonly decimal DiscountAmount = 0;

        public TotalPriceDiscountRule(decimal minPrice, decimal discount)
        {
            Name = $"折價券滿 {minPrice} 抵用 {discount}";
            Note = $"每次交易限用一次";
            MinDiscountPrice = minPrice;
            DiscountAmount = discount;
        }

        public override IEnumerable<Discount> Process(CartContext cart)
        {
            if (cart.TotalPrice > MinDiscountPrice) yield return new Discount()
            {
                Amount = DiscountAmount,
                Rule = this,
                Products = cart.PurchasedItems.ToArray()
            };
        }
    }
}
