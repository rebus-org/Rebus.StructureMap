using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Handlers;
using Rebus.Retry.Simple;
using Rebus.Tests.Contracts;
using Rebus.Tests.Contracts.Extensions;
using Rebus.Transport.InMem;
using StructureMap;

namespace Rebus.StructureMap.Tests.Bugs
{
    [TestFixture]
    public class CheckSecondLevelRetries : FixtureBase
    {
        Container _container;

        protected override void SetUp()
        {
            _container = new Container();

            Using(_container);

            Configure.With(new StructureMapContainerAdapter(_container))
                .Transport(t => t.UseInMemoryTransport(new InMemNetwork(), "2nd-level-goodness"))
                .Options(o => o.SimpleRetryStrategy(secondLevelRetriesEnabled: true, maxDeliveryAttempts: 1))
                .Start();
        }

        [Test]
        public async Task WorksAsExpected()
        {
            var gotTheFailedMessageAsExpected = new ManualResetEvent(false);

            _container.Configure(c =>
            {
                c.For<ManualResetEvent>().Use(gotTheFailedMessageAsExpected);
                c.For<IHandleMessages<SomeMessage>>().Use<SomeMessageHandler>();
                c.For<IHandleMessages<IFailed<SomeMessage>>>().Use<SomeFailedMessageHandler>();
            });

            await _container.GetInstance<IBus>().SendLocal(new SomeMessage());

            gotTheFailedMessageAsExpected.WaitOrDie(TimeSpan.FromSeconds(5));
        }

        class SomeMessage { }

        class SomeMessageHandler : IHandleMessages<SomeMessage>
        {
            public Task Handle(SomeMessage message) => throw new AccessViolationException("I won't have it");
        }

        class SomeFailedMessageHandler : IHandleMessages<IFailed<SomeMessage>>
        {
            readonly ManualResetEvent _wasCalled;

            public SomeFailedMessageHandler(ManualResetEvent wasCalled) => _wasCalled = wasCalled;

            public async Task Handle(IFailed<SomeMessage> message) => _wasCalled.Set();
        }
    }
}