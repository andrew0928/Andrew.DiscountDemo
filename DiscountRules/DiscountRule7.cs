using System.Collections.Generic;
using System.Linq;
using Andrew.DiscountDemo.Core;

namespace Andrew.DiscountDemo.DiscountRules
{
    public class DiscountRule7 : DiscountRuleBase
    {
        private (string drink, string food, decimal price)[] _discount_table = new (string, string, decimal)[]
        {
            ("超值配飲料39", "超值配鮮食39", 39m),
            ("超值配飲料49", "超值配鮮食59", 49m),
            ("超值配飲料49", "超值配鮮食49", 49m),
            ("超值配飲料59", "超值配鮮食59", 59m),
            ("超值配飲料59", "超值配鮮食49", 59m),
        };

        public DiscountRule7(string exclusiveTag = null)
        {
            Name = "配對折扣";
            Note = $"餐餐超值選 39/49/59 優惠";

            ExclusiveTag = exclusiveTag;
        }

        public override IEnumerable<Discount> Process(CartContext cart)
        {
            List<LineItem> purchased_items = new List<LineItem>(cart.GetVisibleLineItems(ExclusiveTag));

            foreach (var d in _discount_table)
            {
                var drinks = purchased_items.Where(p => p.Tags.Contains(d.drink)).OrderByDescending(p => p.CheckoutPrice).ToArray();
                var foods = purchased_items.Where(p => p.Tags.Contains(d.food)).OrderByDescending(p => p.CheckoutPrice).ToArray();

                if (drinks.Count() == 0) continue;
                if (foods.Count() == 0) continue;

                for (int i = 0; true; i++)
                {
                    if (drinks.Length <= i) break;
                    if (foods.Length <= i) break;

                    if (purchased_items.Contains(drinks[i]) == false) break;
                    if (purchased_items.Contains(foods[i]) == false) break;


                    purchased_items.Remove(drinks[i]);
                    purchased_items.Remove(foods[i]);
                    yield return new Discount()
                    {
                        Rule = this,
                        LineItems = new LineItem[] { drinks[i], foods[i] },
                        Amount = drinks[i].CheckoutPrice + foods[i].CheckoutPrice - d.price
                    };
                }
            }
        }
    }
}
