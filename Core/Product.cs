using System.Collections.Generic;
using System.Linq;

namespace Andrew.DiscountDemo.Core
{
    public class Product
    {
        public int Id;
        public string SKU;
        public string Name;
        public decimal Price;
        public HashSet<string> Tags;

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
