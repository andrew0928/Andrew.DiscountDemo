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
        public decimal Price
        {
            get;
            set;
            //get
            //{
            //    try
            //    { 
            //        return this.PriceList.Where(p => this.Tags.Contains(p.Key)).Select(p => p.Value).Min();
            //    }
            //    catch
            //    {
            //        // todo: error handling
            //    }
            //    return this.PriceList["default-price"];
            //}
            //set
            //{
            //    this.PriceList["default-price"] = value;
            //}
        }
        public HashSet<string> Tags;
        

        public Dictionary<string, decimal> PriceList { get; private set; } = new Dictionary<string, decimal>();

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
