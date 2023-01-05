using SampleDomain;

using Shouldly;

using Xunit;

namespace Trading.Tests {
    public class Orders_should {
        Guid _orderId = Guid.NewGuid();
        TestClock _clock = new();

        [Fact]
        public void be_opened() {
            var startingValue = 204L;
            var openedTimeTicks = _clock.GetUtc.Ticks;

            var order = new Order(_orderId, startingValue, _clock);

            var events = ((IEventSource)order).TakeEvents();
            events.Count.ShouldBe(1);

            var opened = events.OfType<OrderMsgs.Opened>().Single();
            opened.ShouldSatisfyAllConditions(
                o => o.AtTimeTicks.ShouldBe(openedTimeTicks),
                o => o.InitialOffering.ShouldBe(startingValue),
                o => o.OrderId.ShouldBe(_orderId)
            );
        }

        [Fact]
        public void record_a_price_change() {
            var price = 234L;
            var opened = new OrderMsgs.Opened(_orderId, price, _clock.GetUtc.Ticks);
            var order = new Order();
            ((IEventSource)order).Hydrate(new IEvent[] { opened });

            _clock.Advance(TimeSpan.FromMicroseconds(1));
            order.UpdatePrice(price - 1, _clock);

            var events = ((IEventSource)order).TakeEvents();
            events.Count.ShouldBe(1);

            var changed = events.OfType<OrderMsgs.PriceChanged>().Single();
            changed.ShouldSatisfyAllConditions(
                c => c.AtTimeTicks.ShouldBe(_clock.GetUtc.Ticks),
                c => c.OrderId.ShouldBe(_orderId),
                c => c.Price.ShouldBe(price - 1)
            );
        }

        [Fact]
        public void issue_a_sell() {
            var price = 234L;
            var opened = new OrderMsgs.Opened(_orderId, price, _clock.GetUtc.Ticks);
            var order = new Order();
            ((IEventSource)order).Hydrate(new IEvent[] { opened });

            _clock.Advance(TimeSpan.FromMinutes(1));
            order.UpdatePrice(price - 1, _clock);
            _clock.Advance(TimeSpan.FromMinutes(3));
            order.UpdatePrice(price + 10, _clock);
            _clock.Advance(TimeSpan.FromMinutes(4));
            order.UpdatePrice(price + 20, _clock);

            _clock.Advance(TimeSpan.FromMinutes(16));
            order.UpdatePrice(price - 100, _clock);

            var events = ((IEventSource)order).TakeEvents();
            events.Count.ShouldBe(5);

            var changed = events.OfType<OrderMsgs.Sell>().Single();
            changed.ShouldSatisfyAllConditions(
                c => c.OrderId.ShouldBe(_orderId)
            );
        }
    }
}
