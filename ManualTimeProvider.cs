using System;

namespace Andrew.DiscountDemo
{
    public class ManualTimeProvider : TimeProvider
    {
        private DateTimeOffset _now;

        public ManualTimeProvider(DateTimeOffset now)
        {
            this._now = now;
        }
        public override DateTimeOffset GetUtcNow()
        {
            return this._now;
        }
    }
}
