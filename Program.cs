using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Andrew.DiscountDemo
{
    public enum DiscountType{
        MultipleDiscountAble = 1,
        SingleOneDiscount = 2,
        IsDiscounted = 4
    }

    class Program
    {
        static void Main(string[] args)
        {
            CartContext cart = new CartContext();
            POS pos = new POS();

            cart.PurchasedItems.AddRange(LoadProducts(@"products3.json"));
            pos.ActivedRules.AddRange(LoadRules());

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

        static IEnumerable<RuleBase> LoadRules()
        {
            //yield return new BuyMoreBoxesDiscountRule(2, 12);   // 買 2 箱，折扣 12%
            //yield return new TotalPriceDiscountRule(1000, 100); // 滿 1000 折 100
            //yield break;
            IEnumerable<RuleBase> rules= new List<RuleBase>(){
                new DiscountRule1("衛生紙", 6, 100){Priority = 4},
                new DiscountRule3("雞湯塊", 50){Priority = 3},
                new DiscountRule4("同商品加購優惠", 10){Priority = 2},
                new DiscountRule6("熱銷飲品", 12){Priority = 1}
            };
            return rules.OrderBy(z=>z.Priority);
        }
    }

    public class CartContext
    {
        public List<Product> GetNonDiscountProduct(){
            return PurchasedItems.Where(z=> z.DiscountType != (DiscountType.SingleOneDiscount | DiscountType.IsDiscounted))
                                 .ToList();
        }
        public readonly List<Product> PurchasedItems = new  List<Product>();
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

            cart.TotalPrice = cart.GetNonDiscountProduct().Select(p => p.Price).Sum();
            foreach (var rule in this.ActivedRules)
            {
                var discounts = rule.Process(cart).ToList();
                cart.AppliedDiscounts.AddRange(discounts);
                cart.TotalPrice -= discounts.Select(d => d.Amount).Sum();
                discounts.ForEach(x=>x.SetDiscountStaus());
            }
            return true;
        }
    }
    
    public class Product
    {
        public int Id;
        public string SKU;
        public string Name;
        public decimal Price;
        public HashSet<string> Tags;

        public DiscountType DiscountType { get; set; }
        public string TagsValue { 
            get
            {
                if (this.Tags == null || this.Tags.Count == 0) return "";
                return ", Tags: " + string.Join(",", this.Tags.Select(t => '#' + t));
            }
        }
    }
    
    public class Discount
    {
        public int Id;
        public RuleBase Rule;
        public Product[] Products;
        public decimal Amount;

        public void SetDiscountStaus(){
            foreach(var p in Products){
                p.DiscountType |= DiscountType.IsDiscounted;
            }
        }
    }

    public abstract class RuleBase
    {
        public int Id;
        public string Name;
        public string Note;
        public int Priority { get; set; }
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

            foreach (var p in cart.GetNonDiscountProduct())
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

    public class TotalPriceDiscountRule : RuleBase
    {
        public readonly decimal MinDiscountPrice = 0;
        public readonly decimal DiscountAmount = 0;

        public TotalPriceDiscountRule(decimal minPrice, decimal discount)
        {
            this.Name = $"折價券滿 {minPrice} 抵用 {discount}";
            this.Note = $"每次交易限用一次";
            this.MinDiscountPrice = minPrice;
            this.DiscountAmount = discount;
        }

        public override IEnumerable<Discount> Process(CartContext cart)
        {
            if (cart.TotalPrice > this.MinDiscountPrice) yield return new Discount()
            {
                Amount = this.DiscountAmount,
                Rule = this,
                Products = cart.PurchasedItems.ToArray()
            };
        }
    }


    public class DiscountRule1 : RuleBase
    {
        private string TargetTag;
        private int MinCount;
        private decimal DiscountAmount;

        public DiscountRule1(string targetTag, int minBuyCount, decimal discountAmount)
        {
            this.Name = "滿件折扣1";
            this.Note = $"{targetTag}滿{minBuyCount}件折{discountAmount}";
            this.TargetTag = targetTag;
            this.MinCount = minBuyCount;
            this.DiscountAmount = discountAmount;
        }

        public override IEnumerable<Discount> Process(CartContext cart)
        {
            List<Product> matched = new List<Product>();
            foreach(var p in cart.GetNonDiscountProduct().Where( p => p.Tags.Contains(this.TargetTag) ))
            {
                matched.Add(p);
                if (matched.Count == this.MinCount)
                {
                    yield return new Discount()
                    {
                        Amount = this.DiscountAmount,
                        Products = matched.ToArray(),
                        Rule = this
                    };

                    matched.Clear();
                }
            }
        }
    }
    public class DiscountRule3 : RuleBase
    {
        private string TargetTag;
        private int PercentOff;
        public DiscountRule3(string targetTag, int percentOff)
        {
            this.Name = "滿件折扣3";
            this.Note = $"{targetTag}第二件{10-percentOff/10}折";

            this.TargetTag = targetTag;
            this.PercentOff = percentOff;
        }
        public override IEnumerable<Discount> Process(CartContext cart)
        {
            List<Product> matched = new List<Product>();
            foreach (var p in cart.GetNonDiscountProduct().Where(p => p.Tags.Contains(this.TargetTag)))
            {
                matched.Add(p);
                if (matched.Count == 2)
                {
                    yield return new Discount()
                    {
                        Amount = p.Price * this.PercentOff / 100,
                        Products = matched.ToArray(),
                        Rule = this
                    };

                    matched.Clear();
                }
            }
        }
    }
    public class DiscountRule4 : RuleBase
    {
        private string TargetTag;
        private decimal DiscountAmount;

        public DiscountRule4(string tag, decimal amount)
        {
            this.Name = "同商品加購優惠";
            this.Note = $"加{amount}元多一件";
            this.TargetTag = tag;
            this.DiscountAmount = amount;
        }
        public override IEnumerable<Discount> Process(CartContext cart)
        {
            List<Product> matched = new List<Product>();
            foreach (var sku in cart.GetNonDiscountProduct().Where(p=>p.Tags.Contains(this.TargetTag)).Select(p=>p.SKU).Distinct())
            {
                matched.Clear();
                foreach(var p in cart.GetNonDiscountProduct().Where(p=>p.SKU == sku))
                {
                    matched.Add(p);
                    if (matched.Count  == 2)
                    {
                        yield return new Discount()
                        {
                            Products = matched.ToArray(),
                            Amount = this.DiscountAmount,
                            Rule = this
                        };

                        matched.Clear();
                    }
                }
            }
        }
    }

    public class DiscountRule6 : RuleBase
    {
        private string TargetTag;
        private int PercentOff;
        public DiscountRule6(string targetTag, int percentOff)
        {
            this.Name = "滿件折扣6";
            this.Note = $"滿{targetTag}二件結帳{10 - percentOff / 10}折";

            this.TargetTag = targetTag;
            this.PercentOff = percentOff;
        }
        public override IEnumerable<Discount> Process(CartContext cart)
        {
            List<Product> matched = new List<Product>();
            foreach (var p in cart.GetNonDiscountProduct().Where(p => p.Tags.Contains(this.TargetTag)).OrderByDescending(p=>p.Price))
            {
                matched.Add(p);
                if (matched.Count == 2)
                {
                    yield return new Discount()
                    {
                        Amount = matched.Sum(p => p.Price) * this.PercentOff / 100,
                        Products = matched.ToArray(),
                        Rule = this
                    };

                    matched.Clear();
                }
            }
        }
    }
}
