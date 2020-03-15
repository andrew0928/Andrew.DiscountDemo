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
            var products = LoadProducts();
            foreach(var p in products)
            {
                Console.WriteLine($"- {p.Name}      {p.Price:C}");
            }
            Console.WriteLine($"Total: {CheckoutProcess(products.ToArray(), LoadRules().ToArray()):C}");
        }


        static decimal CheckoutProcess(Product[] products)
        {
            decimal amount = 0;
            foreach(var p in products)
            {
                amount += p.Price;
            }
            return amount;
        }

        static decimal CheckoutProcess(Product[] products, RuleBase[] rules)
        {
            List<Discount> discounts = new List<Discount>();

            foreach(var rule in rules)
            {
                discounts.AddRange(rule.Process(products));
            }

            decimal amount_without_discount = CheckoutProcess(products);
            decimal total_discount = 0;

            foreach(var discount in discounts)
            {
                total_discount += discount.Amount;
                Console.WriteLine($"- 符合折扣 [{discount.RuleName}], 折抵 {discount.Amount} 元");
            }

            return amount_without_discount - total_discount;
        }



        static IEnumerable<Product> LoadProducts()
        {
            return JsonConvert.DeserializeObject<Product[]>(File.ReadAllText(@"products.json"));
        }

        static IEnumerable<RuleBase> LoadRules()
        {
            yield return new BuyMoreBoxesDiscountRule(2, 12);   // 買 2 箱，折扣 12%
        }
    }



    public class BuyMoreBoxesDiscountRule : RuleBase
    {
        public readonly int BoxCount = 0;
        public readonly int PercentOff = 0;

        public BuyMoreBoxesDiscountRule(int boxes, int percentOff)
        {
            this.BoxCount = boxes;
            this.PercentOff = percentOff;

            this.Name = $"任 {this.BoxCount} 箱結帳 {100 - this.PercentOff} 折!";
            this.Note = "熱銷飲品 限時優惠";
        }

        public override IEnumerable<Discount> Process(Product[] products)
        {
            List<Product> matched_products = new List<Product>();

            foreach(var p in products)
            {
                matched_products.Add(p);

                if (matched_products.Count == this.BoxCount)
                {
                    // 符合折扣
                    yield return new Discount()
                    {
                        Amount = matched_products.Select(p => p.Price).Sum() * this.PercentOff / 100,
                        Products = matched_products.ToArray(),
                        RuleName = this.Name,
                    };
                    matched_products.Clear();
                }
            }
        }

    }


    

    public class Product
    {
        public int Id;
        public string Name;
        public decimal Price;
        public HashSet<string> Tags;
    }
    
    public class Discount
    {
        public int Id;
        public string RuleName;
        public Product[] Products;
        public decimal Amount;
    }

    public abstract class RuleBase
    {
        public int Id;
        public string Name;
        public string Note;
        public abstract IEnumerable<Discount> Process(Product[] products);
    }
}
