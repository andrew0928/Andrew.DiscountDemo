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
            Console.WriteLine($"Total: {CheckoutProcess(products.ToArray()):C}");
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

        static IEnumerable<Product> LoadProducts()
        {
            return JsonConvert.DeserializeObject<Product[]>(File.ReadAllText(@"products.json"));
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
