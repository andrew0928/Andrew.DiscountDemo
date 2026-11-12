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

            cart.PurchasedItems.AddRange(LoadProducts(@"..\..\..\products4.json"));
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

            yield return new DiscountRule1("衛生紙", 6, 100, "ex");
            yield return new DiscountRule3("雞湯塊", 50);
            yield return new DiscountRule4("同商品加購優惠", 10, "ex");
            yield return new DiscountRule6("熱銷飲品", 12);

            yield return new DiscountRule7();

            yield break;
        }
    }

    public class CartContext
    {
        public readonly List<Product> PurchasedItems = new List<Product>();
        public readonly List<Discount> AppliedDiscounts = new List<Discount>();
        public decimal TotalPrice = 0m;

        public IEnumerable<Product> GetVisiblePurchasedItems(string exclusiveTag)
        {
            if (string.IsNullOrEmpty(exclusiveTag)) return this.PurchasedItems;
            return this.PurchasedItems.Where(p => !p.Tags.Contains(exclusiveTag));
        }
    }

    public class POS
    {
        public readonly List<RuleBase> ActivedRules = new List<RuleBase>();

        /// <summary>
        /// Checkout Process
        /// </summary>
        /// <history>
        /// 2021/06/09, lozenlin, modify, 修正當 Product 加上 rule.ExclusiveTag 之後，下次執行的 discounts 結果有變，造成結算總額錯誤的問題
        /// </history>
        public bool CheckoutProcess(CartContext cart)
        {
            // reset cart
            cart.AppliedDiscounts.Clear();

            cart.TotalPrice = cart.PurchasedItems.Select(p => p.Price).Sum();
            foreach (var rule in this.ActivedRules)
            {
                var discounts = rule.Process(cart);
                Discount[] discountsAry = discounts.ToArray();  //2021/06/09, lozenlin, modify
                cart.AppliedDiscounts.AddRange(discountsAry);  //2021/06/09, lozenlin, modify
                if (rule.ExclusiveTag != null)
                {
                    foreach (var d in discountsAry) //2021/06/09, lozenlin, modify
                    {
                        foreach (var p in d.Products) p.Tags.Add(rule.ExclusiveTag);
                    }
                }
                cart.TotalPrice -= discountsAry.Select(d => d.Amount).Sum();   //2021/06/09, lozenlin, modify
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
    }

    public abstract class RuleBase
    {
        public int Id;
        public string Name;
        public string Note;

        public string ExclusiveTag = null;

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

            foreach (var p in cart.GetVisiblePurchasedItems(this.ExclusiveTag))
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

        public DiscountRule1(string targetTag, int minBuyCount, decimal discountAmount, string exclusiveTag = null)
        {
            this.Name = "滿件折扣1";
            this.Note = $"{targetTag}滿{minBuyCount}件折{discountAmount}";
            this.TargetTag = targetTag;
            this.MinCount = minBuyCount;
            this.DiscountAmount = discountAmount;
            this.ExclusiveTag = exclusiveTag;
        }

        public override IEnumerable<Discount> Process(CartContext cart)
        {
            List<Product> matched = new List<Product>();
            foreach(var p in cart.GetVisiblePurchasedItems(this.ExclusiveTag).Where( p => p.Tags.Contains(this.TargetTag) ))
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
            foreach (var p in cart.GetVisiblePurchasedItems(this.ExclusiveTag).Where(p => p.Tags.Contains(this.TargetTag)))
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

        public DiscountRule4(string tag, decimal amount, string exclusiveTag = null)
        {
            this.Name = "同商品加購優惠";
            this.Note = $"加{amount}元多一件";
            this.TargetTag = tag;
            this.DiscountAmount = amount;
            this.ExclusiveTag = exclusiveTag;
        }
        public override IEnumerable<Discount> Process(CartContext cart)
        {
            List<Product> matched = new List<Product>();
            foreach (var sku in cart.GetVisiblePurchasedItems(this.ExclusiveTag).Where(p=>p.Tags.Contains(this.TargetTag)).Select(p=>p.SKU).Distinct())
            {
                matched.Clear();
                foreach(var p in cart.GetVisiblePurchasedItems(this.ExclusiveTag).Where(p=>p.SKU == sku))
                {
                    matched.Add(p);
                    if (matched.Count  == 2)
                    {
                        yield return new Discount()
                        {
                            Products = matched.ToArray(),
                            Amount = p.Price - this.DiscountAmount, //2021/06/09, lozenlin, modify, 修正加價購折抵金額錯誤的問題
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
            foreach (var p in cart.GetVisiblePurchasedItems(this.ExclusiveTag).Where(p => p.Tags.Contains(this.TargetTag)).OrderByDescending(p=>p.Price))
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

    public class DiscountRule7 : RuleBase
    {
        private (string drink, string food, decimal price)[] _discount_table = new (string, string, decimal)[]
        {
            ("超值配飲料39", "超值配鮮食39", 39m),
            ("超值配飲料49", "超值配鮮食59", 49m),
            ("超值配飲料49", "超值配鮮食49", 49m),
            ("超值配飲料59", "超值配鮮食59", 59m),
            ("超值配飲料59", "超值配鮮食49", 59m),
        };

        public DiscountRule7(string exclusiveTag = null)
        {
            this.Name = "配對折扣";
            this.Note = $"餐餐超值選 39/49/59 優惠";

            this.ExclusiveTag = exclusiveTag;
        }

        public override IEnumerable<Discount> Process(CartContext cart)
        {
            List<Product> purchased_items = new List<Product>(cart.GetVisiblePurchasedItems(this.ExclusiveTag));

            foreach(var d in this._discount_table)
            {
                var drinks = purchased_items.Where(p => p.Tags.Contains(d.drink)).OrderByDescending(p => p.Price).ToArray();
                var foods = purchased_items.Where(p => p.Tags.Contains(d.food)).OrderByDescending(p => p.Price).ToArray();

                if (drinks.Count() == 0) continue;
                if (foods.Count() == 0) continue;

                for (int i = 0; true; i++)
                {
                    if (drinks.Length <= i) break;
                    if (foods.Length <= i) break;

                    if (purchased_items.Contains(drinks[i]) == false) break;
                    if (purchased_items.Contains(foods[i]) == false) break;


                    purchased_items.Remove(drinks[i]);
                    purchased_items.Remove(foods[i]);
                    yield return new Discount()
                    {
                        Rule = this,
                        Products = new Product[] { drinks[i], foods[i] },
                        Amount = drinks[i].Price + foods[i].Price - d.price
                    };
                }
            }
        }
    }
}
