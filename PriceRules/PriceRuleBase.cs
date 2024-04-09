using Andrew.DiscountDemo.Core;
using Andrew.DiscountDemo.DiscountRules;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace Andrew.DiscountDemo.PriceRules
{
    public abstract class PriceRuleBase
    {
        public int Id;
        public string Name;
        public string Note;

        public string ExclusiveTag = null;

        public abstract IEnumerable<SetPrice> Process(CartContext cart);

        private static int _seed = 0;
        protected PriceRuleBase()
        {
            this.Id = _seed++;
        }
    }

    public class SetPrice
    {
        public int Id;
        public PriceRuleBase Rule;

        public int LineItemId;              // affected line item
        public decimal NewCheckoutPrice;    // new price
        public string[] AppendTags;         // tags to be added
    }
}
