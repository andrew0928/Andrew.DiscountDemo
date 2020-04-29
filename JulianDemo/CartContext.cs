using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace JulianDemo
{
    public class CartContext
    {
        public readonly List<Product> PurchasedItems = new List<Product>();
        public decimal TotalPrice = 0m;
    }

    public class Product
    {
        public int Id;
        public string SKU;
        public string Name;
        public decimal Price;
        public decimal Discount;
        public HashSet<string> Tags;
        public bool IsDiscounted = false;
        public string Note = "";


        public string TagsValue
        {
            get
            {
                if (this.Tags == null || this.Tags.Count == 0) return "";
                return ", Tags: " + string.Join(",", this.Tags.Select(t => '#' + t));
            }
        }
    }

    public class POS
    {
        public readonly List<RuleBase> ActivedRules = new List<RuleBase>();

        public bool CheckoutProcess(CartContext cart)
        {
            foreach (var rule in this.ActivedRules)
            {
                rule.Process(cart);
            }

            cart.TotalPrice = cart.PurchasedItems.Select(p => p.Price - p.Discount).Sum();
            return true;
        }
    }

    public abstract class RuleBase
    {
        public int Id;
        public string Name;
        public string Note;
        public string TargetTag;
        public abstract void Process(CartContext cart);
    }

    public class DiscountRule4 : RuleBase
    {
        private decimal BuyTwoGetOneSpecialPrice;

        public DiscountRule4(string tag, decimal amount)
        {
            this.Name = "同商品加購優惠";
            this.Note = $"加{amount}元多一件";
            this.TargetTag = tag;
            this.BuyTwoGetOneSpecialPrice = amount;
        }

        public override void Process(CartContext cart)
        {
            List<Product> matched = new List<Product>();
            foreach (var sku in cart.PurchasedItems
                .Where(p => p.Tags.Contains(this.TargetTag) && !p.IsDiscounted)
                .Select(p => p.SKU)
                .Distinct())
            {
                matched.Clear();
                foreach (var p in cart.PurchasedItems.Where(p => p.SKU == sku))
                {
                    matched.Add(p);
                    if (matched.Count == 2)
                    {
                        matched.Last().Discount = matched.Last().Price - BuyTwoGetOneSpecialPrice;
                        matched.ForEach(m =>
                        {
                            m.IsDiscounted = true;
                            m.Note += m.Note == "" ? "" : ";";
                            m.Note += this.Note;
                        });
                        matched.Clear();
                    }
                }
            }
        }
    }

    public class DiscountRule6 : RuleBase
    {
        private int PercentOff;

        public DiscountRule6(string targetTag, int percentOff)
        {
            this.Name = "滿件折扣6";
            this.Note = $"滿{targetTag}二件結帳{10 - percentOff / 10}折";

            this.TargetTag = targetTag;
            this.PercentOff = percentOff;
        }

        public override void Process(CartContext cart)
        {
            var items = cart.PurchasedItems
                .Where(p => p.Tags.Contains(this.TargetTag) && !p.IsDiscounted)
                .OrderByDescending(p => p.Price - p.Discount);
            var count = items.Count() / 2;
            foreach (var product in items)
            {
                if (count > 0)
                {
                    product.Discount += (product.Price - product.Discount) * this.PercentOff / 100;
                    product.Note += product.Note == "" ? "" : ";";
                    product.Note += this.Note;
                    count--;
                }

                product.IsDiscounted = true;
            }
        }
    }

    public class ComplexDiscountRule : RuleBase
    {
        private readonly RuleBase _discount1;
        private readonly RuleBase _discount2;

        public ComplexDiscountRule(RuleBase discount1, RuleBase discount2)
        {
            _discount1 = discount1;
            _discount2 = discount2;
            this.Note = $"{discount1.Note};{discount2.Note}";
        }

        public override void Process(CartContext cart)
        {
            var productsWithDoubleDiscount =
                cart.PurchasedItems.Where(p =>
                        p.Tags.Contains(_discount1.TargetTag) && p.Tags.Contains(_discount2.TargetTag) &&
                        !p.IsDiscounted)
                    .ToList();


            _discount1.Process(cart);

            foreach (var product in productsWithDoubleDiscount)
            {
                product.IsDiscounted = false;
            }

            _discount2.Process(cart);
        }
    }


    public class ComboDiscount : RuleBase
    {
        public ComboDiscount(string targetTag)
        {
            this.TargetTag = targetTag;
        }

        public override void Process(CartContext cart)
        {
            var prices = new List<int>(){39,49,59};
            var drinkDict = new Dictionary<int, Queue<Product>>();
            var foodDict = new Dictionary<int, Queue<Product>>();

            foreach (var price in prices)
            {
                drinkDict[price] = new Queue<Product>(cart.PurchasedItems
                                             .Where(p => p.Tags.Contains($"{this.TargetTag}/{price}/飲料") && !p.IsDiscounted)
                                             .OrderByDescending(p => p.Price - p.Discount));
                
                foodDict[price] = new Queue<Product>(cart.PurchasedItems
                                               .Where(p => p.Tags.Contains($"{this.TargetTag}/{price}/鮮食") && !p.IsDiscounted)
                                               .OrderByDescending(p => p.Price - p.Discount));
            }

            var drinks = drinkDict[39];
            while (drinkDict.Count != 0 && foodDict[39].Count != 0)
            {
                var drink = drinks.Dequeue();
                ProcessComboDrink(drink, 39);
                var food = foodDict[39].Dequeue();
                ProcessComboFood(food, 39);
            }

            drinks = drinkDict[49];
            while (drinks.Count != 0 && foodDict[49].Count != 0 || foodDict[59].Count != 0)
            {
                var drink = drinks.Dequeue();
                ProcessComboDrink(drink, 49);
                var food = foodDict[59].Count > 0 ? foodDict[59].Dequeue() : foodDict[49].Dequeue();
                ProcessComboFood(food,49);
            }

            drinks = drinkDict[59];
            while (drinks.Count != 0 && foodDict[49].Count != 0 || foodDict[59].Count != 0)
            {
                var drink = drinks.Dequeue();
                ProcessComboDrink(drink, 59);
                var food = foodDict[49].Count > 0 ? foodDict[49].Dequeue() : foodDict[59].Dequeue();
                ProcessComboFood(food,59);
            }
        }

        private void ProcessComboFood(Product food, int comboPrice)
        {
            ProcessComboItem(food, comboPrice);
            food.Discount = food.Price;
        }

        private  void ProcessComboItem(Product food, int comboPrice)
        {
            food.IsDiscounted = true;
            food.Note += food.Note == "" ? "" : ";";
            food.Note += $"{this.TargetTag}{comboPrice}";
        }

        private void ProcessComboDrink(Product drink, int comboPrice)
        {
            ProcessComboItem(drink,comboPrice);
            drink.Discount = drink.Price - comboPrice;
        }
    }
}