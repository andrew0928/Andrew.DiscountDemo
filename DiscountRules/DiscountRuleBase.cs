using System.Collections.Generic;
using Andrew.DiscountDemo.Core;

namespace Andrew.DiscountDemo.DiscountRules
{
    public abstract class DiscountRuleBase
    {
        public int Id;
        public string Name;
        public string Note;

        public string ExclusiveTag = null;

        public abstract IEnumerable<Discount> Process(CartContext cart);
    }
}
