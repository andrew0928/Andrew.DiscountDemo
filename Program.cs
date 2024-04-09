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




    public class Program
    {
        public static TimeProvider SystemTimeProvider = new ManualTimeProvider(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.FromHours(8)));

        static void Main(string[] args)
        {
            CartContext cart = new CartContext();
            POS pos = new POS();

            // todo: obsolete
            //cart.PurchasedItems.AddRange(LoadProducts(@"..\..\..\products5.json"));
            LoadProducts()
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




        //static int _seed = 0;
        //static IEnumerable<Product> LoadProducts(string filename = @"products.json")
        //{
        //    foreach(var p in JsonConvert.DeserializeObject<Product[]>(File.ReadAllText(filename)))
        //    {
        //        _seed++;
        //        p.Id = _seed;
        //        yield return p;
        //    }
        //}






        static IEnumerable<Product> LoadProducts()
        {
            int _seed = 1;

            yield return new Product()
            {
                Id = _seed++,
                SKU = "FAMI-003",
                Name = "龍蝦風味沙拉三明治",
                Price = 39.00m,
                Tags = new HashSet<string>() { "超值配鮮食49" }
            };

            yield return new Product()
            {
                Id = _seed++,
                SKU = "FAMI-011",
                Name = "單品咖啡 熱美式小杯",
                Price = 50.00m,
                Tags = new HashSet<string>() { "超值配飲料59" }
            };


            //{
            //  "sku": "FAMI-005",
            //  "name": "大口法香烤雞飯糰",
            //  "price": 39,
            //  "tags": [ "超值配鮮食59" ]
            //},

            yield return new Product()
            {
                Id = _seed++,
                SKU = "FAMI-005",
                Name = "大口法香烤雞飯糰",
                Price = 39.00m,
                Tags = new HashSet<string>() { "超值配鮮食59" }
            };

            //{
            //  "sku": "FAMI-012",
            //  "name": "中杯熱拿鐵",
            //  "price": 45,
            //  "tags": [ "超值配飲料59" ]
            //},


            yield return new Product()
            {
                Id = _seed++,
                SKU = "FAMI-012",
                Name = "中杯熱拿鐵",
                Price = 45.00m,
                Tags = new HashSet<string>() { "超值配飲料59" }
            };

            //{
            //  "sku": "FAMI-101",
            //  "name": "鋼鐵人公仔1",
            //  "price": 399,
            //  "tags": [ "鋼鐵人紀念公仔" ]
            //},

            yield return new Product()
            {
                Id = _seed++,
                SKU = "FAMI-101",
                Name = "鋼鐵人公仔1",
                Price = 399.00m,
                Tags = new HashSet<string>() { "鋼鐵人紀念公仔" }
            };

            //{
            //  "sku": "FAMI-102",
            //  "name": "鋼鐵人公仔2",
            //  "price": 499,
            //  "tags": [ "鋼鐵人紀念公仔" ]
            //},

            yield return new Product()
            {
                Id = _seed++,
                SKU = "FAMI-102",
                Name = "鋼鐵人公仔2",
                Price = 499.00m,
                Tags = new HashSet<string>() { "鋼鐵人紀念公仔" }
            };


            //{
            //  "sku": "FAMI-103",
            //  "name": "鋼鐵人公仔3",
            //  "price": 599,
            //  "tags": [ "鋼鐵人紀念公仔" ]
            //}

            yield return new Product()
            {
                Id = _seed++,
                SKU = "FAMI-103",
                Name = "鋼鐵人公仔3",
                //Price = 599.00m,
                Tags = new HashSet<string>() { "鋼鐵人紀念公仔" },
                PriceList = new List<Product.PriceItem>()
                {
                    new Product.PriceItem() { PriceValue = 599.00m, Since = new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(8)), Notes = "default price" },
                    new Product.PriceItem() { PriceValue = 549.00m, Since = new DateTimeOffset(new DateTime(2023, 7, 1), TimeSpan.FromHours(8)), Notes = "2023 summer special price" },
                    new Product.PriceItem() { PriceValue = 599.00m, Since = new DateTimeOffset(new DateTime(2024, 7, 1), TimeSpan.FromHours(8)), Notes = "back to default price" },
                }
            };
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
                "指定商品 - 鋼鐵人加價購",
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
