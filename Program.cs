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

            cart.PurchasedItems.AddRange(LoadProducts(@"..\..\..\products3.json"));
            pos.ActivedRules.AddRange(LoadRules());

            pos.CheckoutProcess(cart);

            Console.WriteLine($"�ʶR�ӫ~:");
            Console.WriteLine($"---------------------------------------------------");
            foreach (var p in cart.PurchasedItems)
            {
                Console.WriteLine($"- {p.Id,02}, [{p.SKU}] {p.Price,8:C}, {p.Name} {p.TagsValue}");
            }
            Console.WriteLine();

            Console.WriteLine($"�馩:");
            Console.WriteLine($"---------------------------------------------------");
            foreach (var d in cart.AppliedDiscounts)
            {
                Console.WriteLine($"- ��� {d.Amount,8:C}, {d.Rule.Name} ({d.Rule.Note})");
                foreach (var p in d.Products) Console.WriteLine($"  * �ŦX: {p.Id,02}, [{p.SKU}], {p.Name} {p.TagsValue}");
                Console.WriteLine();
            }
            Console.WriteLine();

            Console.WriteLine($"---------------------------------------------------");
            Console.WriteLine($"���b���B:   {cart.TotalPrice:C}");
        }


        static int _seed = 0;
        static IEnumerable<Product> LoadProducts(string filename = @"products.json")
        {
            foreach (var p in JsonConvert.DeserializeObject<Product[]>(File.ReadAllText(filename)))
            {
                _seed++;
                p.Id = _seed;
                yield return p;
            }
        }

        static IEnumerable<RuleBase> LoadRules()
        {
            //yield return new BuyMoreBoxesDiscountRule(2, 12);   // �R 2 �c�A�馩 12%
            //yield return new TotalPriceDiscountRule(1000, 100); // �� 1000 �� 100
            //yield break;

            yield return new DiscountRule1("�åͯ�", 6, 100);
            yield return new DiscountRule3("������", 50);
            yield return new DiscountRule4("�P�ӫ~�[���u�f", 10);
            yield return new DiscountRule6("���P���~", 12);
            yield return new DiscountRule5(new List<SpecialOffer>()
            {
                new SpecialOffer()
                {
                    Tags = new[]{ "���w�A��" , "���w����" },
                    Amount = 39
                },
                new SpecialOffer()
                {
                    Tags = new[]{ "���w�A��" , "���w����" },
                    Amount = 49
                },new SpecialOffer()
                {
                    Tags = new[]{ "���w�A��" , "���w����" },
                    Amount = 59
                }
            });
            yield break;

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
        public string SKU;
        public string Name;
        public decimal Price;
        public HashSet<string> Tags;

        public string TagsValue
        {
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

            this.Name = $"�� {this.BoxCount} �c���b {100 - this.PercentOff} ��!";
            this.Note = "���P���~ �����u�f";
        }

        public override IEnumerable<Discount> Process(CartContext cart)
        {
            List<Product> matched_products = new List<Product>();

            foreach (var p in cart.PurchasedItems)
            {
                matched_products.Add(p);

                if (matched_products.Count == this.BoxCount)
                {
                    // �ŦX�馩
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
            this.Name = $"����麡 {minPrice} ��� {discount}";
            this.Note = $"�C��������Τ@��";
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
            this.Name = "����馩1";
            this.Note = $"{targetTag}��{minBuyCount}���{discountAmount}";
            this.TargetTag = targetTag;
            this.MinCount = minBuyCount;
            this.DiscountAmount = discountAmount;
        }

        public override IEnumerable<Discount> Process(CartContext cart)
        {
            List<Product> matched = new List<Product>();
            foreach (var p in cart.PurchasedItems.Where(p => p.Tags.Contains(this.TargetTag)))
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
            this.Name = "����馩3";
            this.Note = $"{targetTag}�ĤG��{10 - percentOff / 10}��";

            this.TargetTag = targetTag;
            this.PercentOff = percentOff;
        }
        public override IEnumerable<Discount> Process(CartContext cart)
        {
            List<Product> matched = new List<Product>();
            foreach (var p in cart.PurchasedItems.Where(p => p.Tags.Contains(this.TargetTag)))
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
            this.Name = "�P�ӫ~�[���u�f";
            this.Note = $"�[{amount}���h�@��";
            this.TargetTag = tag;
            this.DiscountAmount = amount;
        }
        public override IEnumerable<Discount> Process(CartContext cart)
        {
            List<Product> matched = new List<Product>();
            foreach (var sku in cart.PurchasedItems.Where(p => p.Tags.Contains(this.TargetTag)).Select(p => p.SKU).Distinct())
            {
                matched.Clear();
                foreach (var p in cart.PurchasedItems.Where(p => p.SKU == sku))
                {
                    matched.Add(p);
                    if (matched.Count == 2)
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
            this.Name = "����馩6";
            this.Note = $"��{targetTag}�G�󵲱b{10 - percentOff / 10}��";

            this.TargetTag = targetTag;
            this.PercentOff = percentOff;
        }
        public override IEnumerable<Discount> Process(CartContext cart)
        {
            List<Product> matched = new List<Product>();
            foreach (var p in cart.PurchasedItems.Where(p => p.Tags.Contains(this.TargetTag)).OrderByDescending(p => p.Price))
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

    public class DiscountRule5 : RuleBase
    {
        private IEnumerable<SpecialOffer> _specialOffer;
        public DiscountRule5(IEnumerable<SpecialOffer> specialOffersList)
        {
            this.Name = "�\�\�W�Ȱt";
            this.Note = $"���w�A�� + ���w���� �S�� ( 39��, 49��, 59�� )";
            _specialOffer = specialOffersList;
        }
        public override IEnumerable<Discount> Process(CartContext cart)
        {
            foreach (var purchasedItem in cart.PurchasedItems.OrderByDescending(z => z.Price))
            {
                var matchOffer = _specialOffer.FirstOrDefault(m => m.TagTable.Any(z => purchasedItem.Tags.Contains(z)));

                foreach (var tag in purchasedItem.Tags)
                {
                    if (matchOffer != null && matchOffer.ProductQueue.TryGetValue(tag, out var queue))
                    {
                        queue.Enqueue(new Product()
                        {
                            Name = purchasedItem.Name,
                            Price = purchasedItem.Price,
                            Id = purchasedItem.Id
                        });

                        if (matchOffer.ProductQueue.All(z => z.Value.Count > 0))
                        {
                            var products = matchOffer.ProductQueue.Select(x => x.Value.Dequeue()).ToList();
                            yield return new Discount()
                            {
                                Amount = products.Sum(x => x.Price) - matchOffer.Amount,
                                Products = products.ToArray(),
                                Rule = this
                            };
                        }
                    }
                }
            }
        }
    }
    public class SpecialOffer
    {
        private HashSet<string> _tagTable;
        public HashSet<string> TagTable
        {
            get
            {
                return _tagTable = (_tagTable ?? Tags.Select(tag => tag + Amount).ToHashSet());
            }
        }
        public string[] Tags { get; set; }
        public decimal Amount { get; set; }
        private Dictionary<string, Queue<Product>> _productQueue;
        public Dictionary<string, Queue<Product>> ProductQueue
        {
            get
            {
                return _productQueue = (_productQueue ?? TagTable.ToDictionary(x => x, x => new Queue<Product>()));
            }
        }
    }
}