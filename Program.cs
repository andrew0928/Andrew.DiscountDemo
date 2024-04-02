using Andrew.DiscountDemo.Core;
using Andrew.DiscountDemo.DiscountRules;
using Andrew.DiscountDemo.PriceRules;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andrew.DiscountDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            CartContext cart = new CartContext();
            POS pos = new POS();

            // todo: obsolete
            //cart.PurchasedItems.AddRange(LoadProducts(@"..\..\..\products5.json"));
            LoadProducts(@"..\..\..\products5.json")
                .ToList()
                .ForEach(p => cart.AddLineItem(p));
            pos.ActivedDiscountRules.AddRange(LoadDiscountRules());
            pos.ActivedPriceRules.AddRange(LoadPriceRules());

            pos.CheckoutProcess(cart);

            Console.WriteLine($"購買商品:");
            Console.WriteLine($"---------------------------------------------------");
            //foreach(var p in cart.PurchasedItems)
            foreach(var i in cart.LineItems)
            {
                Console.WriteLine($"- {i.ProductId,02}, [{i.Product.SKU}] {i.CheckoutPrice,8:C}, {i.Product.Name} {i.TagsValue}");
            }
            Console.WriteLine();

            Console.WriteLine($"優惠價:");
            Console.WriteLine($"---------------------------------------------------");
            foreach(var p in cart.AppliedSetPrices)
            {
                var item = cart.LineItems.FirstOrDefault(x => x.Id == p.LineItemId);
                Console.WriteLine($"- 優惠價({p.Rule.Name}): {p.NewCheckoutPrice,8:C}, {item.ProductId,02}, [{item.Product.SKU}] {item.Product.Price,8:C} -> {p.NewCheckoutPrice,8:C} {string.Join(", ", p.AppendTags)}");
            }
            Console.WriteLine();

            Console.WriteLine($"折扣:");
            Console.WriteLine($"---------------------------------------------------");
            foreach(var d in cart.AppliedDiscounts)
            {
                Console.WriteLine($"- 折抵 {d.Amount,8:C}, {d.Rule.Name} ({d.Rule.Note})");
                foreach (var p in d.LineItems) Console.WriteLine($"  * 符合: {p.Id, 02}, [{p.Product.SKU}], {p.Product.Name} {p.TagsValue}");
                Console.WriteLine();
            }
            Console.WriteLine();

            Console.WriteLine($"---------------------------------------------------");
            Console.WriteLine($"結帳金額:   {cart.TotalPrice:C}");
        }




        static int _seed = 0;
        static IEnumerable<Product> LoadProducts(string filename = @"products.json")
        {
            foreach(var p in JsonConvert.DeserializeObject<Product[]>(File.ReadAllText(filename)))
            {
                _seed++;
                p.Id = _seed;
                yield return p;
            }
        }



        static IEnumerable<DiscountRuleBase> LoadDiscountRules()
        {
            //yield return new BuyMoreBoxesDiscountRule(2, 12);   // 買 2 箱，折扣 12%
            //yield return new TotalPriceDiscountRule(1000, 100); // 滿 1000 折 100
            //yield break;

            yield return new DiscountRule1("衛生紙", 6, 100, "ex");
            yield return new DiscountRule3("雞湯塊", 50);
            yield return new DiscountRule4("同商品加購優惠", 10, "ex");
            yield return new DiscountRule6("熱銷飲品", 12);

            yield return new DiscountRule7();

            yield break;
        }

        static IEnumerable<PriceRuleBase> LoadPriceRules()
        {
            yield return new AddOnPriceRule(
                "鋼鐵人加價購",
                new string[] { "FAMI-003" },
                new AddOnPriceRule.PriceItem[] {
                    new AddOnPriceRule.PriceItem() { MainProductSKU = null, AddOnProductSKU = "FAMI-101", Price = 99.00m },
                    new AddOnPriceRule.PriceItem() { MainProductSKU = null, AddOnProductSKU = "FAMI-102", Price = 149.00m },
                    new AddOnPriceRule.PriceItem() { MainProductSKU = null, AddOnProductSKU = "FAMI-103", Price = 199.00m },
                });

            yield break;
        }
    }
}
