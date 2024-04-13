using System;
using System.Collections.Generic;
using System.Linq;

namespace Andrew.DiscountDemo.Core
{
    public class Product
    {
        public int Id;
        public string SKU;
        public string Name;

        // default price, 會從 PriceList 判定出合適的價格數值
        //[Obsolete("use GetPrice() instead")]
        public decimal Price
        {
            get
            {
                PriceItem selected_price = null;
                foreach (var p in PriceList.Where(p => p.LocationId==0).OrderBy(p => p.Since))
                {
                    if (p.Since <= Program.SystemTimeProvider.GetUtcNow())
                    {
                        //price = p.price;
                        selected_price = p;
                    }
                }

                if (selected_price == null)
                {
                    throw new Exception($"Price not found for {this.SKU} {this.Name}");
                }
                // Console.WriteLine($"- {this.SKU} {this.Name} price: {selected_price.PriceValue} (since: {selected_price.Since}, {selected_price.Notes})");
                this.PriceNotes = selected_price.Notes;
                return selected_price.PriceValue;
            }

            [Obsolete("use PriceList instead")]
            set
            {
                this.PriceList.Clear();
                this.PriceList.Add(new PriceItem()
                {
                    PriceValue = value,
                    Since = DateTimeOffset.MinValue,
                    Notes = "default price"
                });
            }
        }
        public string PriceNotes { get; private set; }

        public HashSet<string> Tags;
        
        public List<PriceItem> PriceList = new List<PriceItem>();

        //public Dictionary<string, decimal> PriceList { get; private set; } = new Dictionary<string, decimal>();

        public string TagsValue
        {
            get
            {
                if (Tags == null || Tags.Count == 0) return "";
                return ", Tags: " + string.Join(",", Tags.Select(t => '#' + t));
            }
        }



        public class PriceItem
        {
            public decimal PriceValue;
            public DateTimeOffset Since;
            public string Notes;
            public int LocationId = 0;
        }
    }
}
