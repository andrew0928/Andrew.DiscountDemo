using System;
using System.Collections.Generic;
using System.Linq;
using Andrew.DiscountDemo.DiscountRules;

namespace Andrew.DiscountDemo.Core
{
    public class CartContext
    {
        [Obsolete("replace with lineitems")]
        public readonly List<Product> PurchasedItems = new List<Product>();

        public readonly List<LineItem> LineItems = new List<LineItem>();
        public readonly List<Discount> AppliedDiscounts = new List<Discount>();
        public decimal TotalPrice = 0m;

        [Obsolete("replace with lineitems", true)]
        public IEnumerable<Product> GetVisiblePurchasedItems(string exclusiveTag)
        {
            if (string.IsNullOrEmpty(exclusiveTag)) return PurchasedItems;
            return PurchasedItems.Where(p => !p.Tags.Contains(exclusiveTag));
        }

        public IEnumerable<LineItem> GetVisibleLineItems(string exclusiveTag)
        {
            if (string.IsNullOrEmpty(exclusiveTag)) return LineItems;
            return LineItems.Where(p => !p.Tags.Contains(exclusiveTag));
        }

        public void AddLineItem(Product product)
        {
            LineItems.Add(new LineItem(product));
        }
    }

    public class LineItem
    {
        private static int _seed = 0;

        public LineItem(Product product, string[] tags = null)
        {
            this.Id = _seed++;
            this.Product = product;

            // clone from product
            this.ProductId = product.Id;
            this.CheckoutPrice = product.Price;
            this.Tags = new HashSet<string>(product.Tags);
            if (tags != null) this.Tags.UnionWith(tags);
        }

        public int Id = 0;
        public int ProductId = 0;
        public readonly Product Product = null;
        public decimal CheckoutPrice = 0m;
        public HashSet<string> Tags = new HashSet<string>();

        public string TagsValue
        {
            get
            {
                if (Tags == null || Tags.Count == 0) return "";
                return ", Tags: " + string.Join(",", Tags.Select(t => '#' + t));
            }
        }
    }
}
