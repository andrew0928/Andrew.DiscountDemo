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

            cart.PurchasedItems.AddRange(LoadProducts());
            pos.ActivedRules.AddRange(LoadRules());

            pos.CheckoutProcess(cart);

            Console.WriteLine($"購買商品:");
            Console.WriteLine($"---------------------------------------------------");
            foreach(var p in cart.PurchasedItems)
            {
                Console.WriteLine($"- {p.Name}      {p.Price:C}");
            }
            Console.WriteLine();

            Console.WriteLine($"折扣:");
            Console.WriteLine($"---------------------------------------------------");
            foreach(var d in cart.AppliedDiscounts)
            {
                Console.WriteLine($"- {d.Rule.Name} ({d.Rule.Note}), discount: {d.Amount}");
            }
            Console.WriteLine();

            Console.WriteLine($"---------------------------------------------------");
            Console.WriteLine($"Total Price:   {cart.TotalPrice:C}");
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

    public class CartContext
    {
        public readonly List<Product> PurchasedItems = new List<Product>();
        public readonly List<Discount> AppliedDiscounts = new List<Discount>();
        public decimal TotalPrice = 0m;
    }

    public class POS
    {
        public readonly List<RuleBase> ActivedRules = new List<RuleBase>();

        public bool CheckoutProcess(CartContext cart)
        {
            // reset cart
            cart.AppliedDiscounts.Clear();

            cart.TotalPrice = cart.PurchasedItems.Select(p => p.Price).Sum();
            foreach (var rule in this.ActivedRules)
            {
                var discounts = rule.Process(cart);
                cart.AppliedDiscounts.AddRange(discounts);
                cart.TotalPrice -= discounts.Select(d => d.Amount).Sum();
            }
            return true;
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
        public RuleBase Rule;
        public Product[] Products;
        public decimal Amount;
    }

    public abstract class RuleBase
    {
        public int Id;
        public string Name;
        public string Note;
        public abstract IEnumerable<Discount> Process(CartContext cart);
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

        public override IEnumerable<Discount> Process(CartContext cart)
        {
            List<Product> matched_products = new List<Product>();

            foreach (var p in cart.PurchasedItems)
            {
                matched_products.Add(p);

                if (matched_products.Count == this.BoxCount)
                {
                    // 符合折扣
                    yield return new Discount()
                    {
                        Amount = matched_products.Select(p => p.Price).Sum() * this.PercentOff / 100,
                        Products = matched_products.ToArray(),
                        Rule = this,
                    };
                    matched_products.Clear();
                }
            }
        }
    }
}
