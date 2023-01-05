using SampleDomain;

namespace Trading {
    // if stock drops below 10% from peak, sell.
    // service will receive a series of price changed events with a price and timestamp
    // when the stock drops below 10% of peak value in 15 min, sell.
    // prices need a time window to stick, typically 5 seconds


    public class Order : AggregateRoot {
        List<priceEntry> _priceTracking = new();

        public Order(Guid orderId, long initialOffering, IClock clock) {
            Raise(new OrderMsgs.Opened(orderId, initialOffering, clock.GetUtc.Ticks));
        }

        public Order() { }

        public void UpdatePrice(long newPrice, IClock clock) {
            var dte = clock.GetUtc;

            Raise(new OrderMsgs.PriceChanged(Id, newPrice, dte.Ticks));

            var validPrices = _priceTracking
                .Where(pv => pv.AtTimeTicks <= dte.Add(TimeSpan.FromSeconds(-5)).Ticks)
                .ToArray();

            var peakValue = validPrices.Length > 0
                ? validPrices.Max(pv => pv.Price)
                : 0;

            var tenPercentLess = peakValue * 0.9M;
            if (newPrice < tenPercentLess) {
                Raise(new OrderMsgs.Sell(Id, newPrice));
            }
        }


        private void Apply(OrderMsgs.Opened msg) {
            Id = msg.OrderId;
            _priceTracking.Add(new priceEntry(msg.AtTimeTicks, msg.InitialOffering));
        }

        private void Apply(OrderMsgs.PriceChanged msg) {
            _priceTracking.Add(new priceEntry(msg.AtTimeTicks, msg.Price));
        }

        private void Apply(OrderMsgs.Sell _) {
            // no-op
        }

        class priceEntry {
            public readonly long AtTimeTicks;
            public readonly long Price;

            public priceEntry(long atTimeTicks, long price) {
                AtTimeTicks = atTimeTicks;
                Price = price;
            }
        }
    }

    public class OrderMsgs {
        public class Opened : Event {
            public readonly Guid OrderId;
            public readonly long InitialOffering;
            public readonly long AtTimeTicks;

            public Opened(Guid orderId, long initialOffering, long atTimeTicks) {
                OrderId = orderId;
                InitialOffering = initialOffering;
                AtTimeTicks = atTimeTicks;
            }
        }

        public class PriceChanged : Event {
            public readonly Guid OrderId;
            public readonly long Price;
            public readonly long AtTimeTicks;

            public PriceChanged(Guid orderId, long price, long atTimeTicks) {
                OrderId = orderId;
                Price = price;
                AtTimeTicks = atTimeTicks;
            }
        }

        public class Sell : Event {
            public readonly Guid OrderId;
            public readonly long PriceToSellAt;

            public Sell(Guid orderId, long priceToSellAt) {
                OrderId = orderId;
                PriceToSellAt = priceToSellAt;
            }
        }
    }

    public interface IClock {
        DateTime GetUtc { get; }
    }
}