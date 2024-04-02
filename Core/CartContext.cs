using System.Collections.Generic;
using System.Linq;
using Andrew.DiscountDemo.DiscountRules;

namespace Andrew.DiscountDemo.Core
{
    public class CartContext
    {
        public readonly List<Product> PurchasedItems = new List<Product>();
        public readonly List<Discount> AppliedDiscounts = new List<Discount>();
        public decimal TotalPrice = 0m;

        public IEnumerable<Product> GetVisiblePurchasedItems(string exclusiveTag)
        {
            if (string.IsNullOrEmpty(exclusiveTag)) return PurchasedItems;
            return PurchasedItems.Where(p => !p.Tags.Contains(exclusiveTag));
        }
    }

    public class PurchasedItem
    {
        public int ProductId = 0;
        public decimal CheckoutPrice = 0m;
        public HashSet<string> Tags = new HashSet<string>();
    }
}
