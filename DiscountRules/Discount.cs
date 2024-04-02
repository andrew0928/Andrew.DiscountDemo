using Andrew.DiscountDemo.Core;

namespace Andrew.DiscountDemo.DiscountRules
{
    public class Discount
    {
        public int Id;
        public DiscountRuleBase Rule;
        public LineItem[] LineItems;
        public decimal Amount;
    }
}
