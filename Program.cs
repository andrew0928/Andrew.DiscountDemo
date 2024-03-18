using Andrew.DiscountDemo.Core;
using Andrew.DiscountDemo.DiscountRules;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Andrew.DiscountDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            CartContext cart = new CartContext();
            POS pos = new POS();

            cart.PurchasedItems.AddRange(LoadProducts(@"..\..\..\products5.json"));
            pos.ActivedRules.AddRange(LoadDiscountRules());

            pos.CheckoutProcess(cart);

            Console.WriteLine($"購買商品:");
            Console.WriteLine($"---------------------------------------------------");
            foreach(var p in cart.PurchasedItems)
            {
                Console.WriteLine($"- {p.Id,02}, [{p.SKU}] {p.Price,8:C}, {p.Name} {p.TagsValue}");
            }
            Console.WriteLine();

            Console.WriteLine($"折扣:");
            Console.WriteLine($"---------------------------------------------------");
            foreach(var d in cart.AppliedDiscounts)
            {
                Console.WriteLine($"- 折抵 {d.Amount,8:C}, {d.Rule.Name} ({d.Rule.Note})");
                foreach (var p in d.Products) Console.WriteLine($"  * 符合: {p.Id, 02}, [{p.SKU}], {p.Name} {p.TagsValue}");
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
    }
}
