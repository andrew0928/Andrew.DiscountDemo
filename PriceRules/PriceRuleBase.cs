using Andrew.DiscountDemo.Core;
using Andrew.DiscountDemo.DiscountRules;
using System;
using System.Collections.Generic;
using System.Text;

namespace Andrew.DiscountDemo.PriceRules
{
    public abstract class PriceRuleBase
    {
        public int Id;
        public string Name;
        public string Note;

        public string ExclusiveTag = null;

        public abstract IEnumerable<SetPrice> Process(CartContext cart);
    }

    public class SetPrice
    {
        public Product Product;     // affected product
        public decimal NewPrice;    // new price
        public string[] Tags;       // tags to be added
    }
}
