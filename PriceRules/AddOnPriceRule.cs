using Andrew.DiscountDemo.Core;
using System.Collections.Generic;
using System.Linq;

namespace Andrew.DiscountDemo.PriceRules
{
    // 加價購
    public class AddOnPriceRule : PriceRuleBase
    {
        public AddOnPriceRule(string name, string[] main_products, PriceItem[] price_lists) : base()
        {
            this.Name = name;
            this.MainProductCollection = new HashSet<string>(main_products);
            this.PriceList = price_lists;
        }


        // 主購商品清單
        public HashSet<string> MainProductCollection = new HashSet<string>();

        // 加價購商品清單 / 價格表
        public PriceItem[] PriceList = new PriceItem[]
        {
            new PriceItem { MainProductSKU = "", AddOnProductSKU = "", Price = 10 },
        };


        public override IEnumerable<SetPrice> Process(CartContext cart)
        {
            // 1. 確認購物車中有沒有主購商品 (MainProductCollection)
            var m = cart.GetVisibleLineItems(this.ExclusiveTag)
                .Select(p => p.Product.SKU)
                //.Intersect(this.MainProductCollection).ToArray();
                .Where(sku => this.MainProductCollection.Contains(sku))
                .ToArray();
            if (m.Length == 0) yield break;

            var addon_quota = m.Length;

            // 2. 掃描購物車中的商品，找出符合加價購的商品
            // 3. 設定新價格
            var mset = new HashSet<string>(m);
            foreach (var item in cart.GetVisibleLineItems(this.ExclusiveTag))
            {
                SetPrice sp = null;
                foreach(var pitem in this.PriceList)
                {
                    if (pitem.MainProductSKU != null && !mset.Contains(pitem.MainProductSKU)) continue;
                    if (pitem.AddOnProductSKU != null && pitem.AddOnProductSKU != item.Product.SKU) continue;

                    if (sp == null) sp = new SetPrice()
                    {
                        Rule = this,
                        LineItemId = item.Id
                    };

                    if (sp.NewCheckoutPrice == 0 || sp.NewCheckoutPrice > pitem.Price)
                    {
                        sp.NewCheckoutPrice = pitem.Price;
                        sp.AppendTags = new string[] { $"{this.Name}::AddOnProduct" };
                    }
                }
                if (sp != null && addon_quota > 0)
                {
                    addon_quota--;
                    yield return sp;
                }
            }
            yield break;
        }


        public class PriceItem
        {
            // 看起來不需要, 固定是 *
            public string MainProductSKU;
            public string AddOnProductSKU;
            public decimal Price;
        }
    }
}
