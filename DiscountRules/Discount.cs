using Andrew.DiscountDemo.Core;

namespace Andrew.DiscountDemo.DiscountRules
{
    public class Discount
    {
        public int Id;
        public DiscountRuleBase Rule;
        public Product[] Products;
        public decimal Amount;
    }
}
