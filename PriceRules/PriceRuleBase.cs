using Andrew.DiscountDemo.Core;
using Andrew.DiscountDemo.DiscountRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
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

        private static int _seed = 0;
        protected PriceRuleBase()
        {
            this.Id = _seed++;
        }
    }

    public class SetPrice
    {
        public int Id;
        public PriceRuleBase Rule;

        public int LineItemId;              // affected line item
        public decimal NewCheckoutPrice;    // new price
        public string[] AppendTags;         // tags to be added
    }



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
                .Intersect(this.MainProductCollection).ToArray();
            if (m.Length == 0) yield break;


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
                if (sp != null) yield return sp;
            }
            yield break;
        }


        public class PriceItem
        {
            public string MainProductSKU;
            public string AddOnProductSKU;
            public decimal Price;
        }
    }
}
