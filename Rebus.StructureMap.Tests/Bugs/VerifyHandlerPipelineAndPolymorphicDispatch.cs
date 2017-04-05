using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using NUnit.Framework;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Handlers;
using Rebus.Tests.Contracts;
using Rebus.Transport.InMem;
using StructureMap;

namespace Rebus.StructureMap.Tests.Bugs
{
    [TestFixture]
    public class VerifyHandlerPipelineAndPolymorphicDispatch : FixtureBase
    {
        Container _container;
        IBus _bus;

        protected override void SetUp()
        {
            _container = new Container();

            Using(_container);

            _bus = Configure.With(new StructureMapContainerAdapter(_container))
                .Transport(t => t.UseInMemoryTransport(new InMemNetwork(), "sm-test"))
                .Start();
        }

        [Test]
        public async Task ItWorks()
        {
            var events = new ConcurrentQueue<string>();

            _container.Configure(c =>
            {
                c.For<ConcurrentQueue<string>>().Use(events);
                c.For<IHandleMessages<SomeMessage>>().Use<Handler1>();
                c.For<IHandleMessages<ISomeInterface>>().Use<Handler2>();
            });

            await _bus.SendLocal(new SomeMessage());

            await Task.Delay(1000);

            Assert.That(events.ToArray(), Is.EqualTo(new[]
            {
                "Handled by Handler1",
                "Handled by Handler2",
            }));
        }

        public interface ISomeInterface { }

        public class SomeMessage : ISomeInterface { }

        public class Handler1 : IHandleMessages<SomeMessage>
        {
            readonly ConcurrentQueue<string> _events;

            public Handler1(ConcurrentQueue<string> events) => _events = events;

            public async Task Handle(SomeMessage message)
            {
                Console.WriteLine(@"
Executing Handler1
");

                _events.Enqueue("Handled by Handler1");
            }
        }

        public class Handler2 : IHandleMessages<ISomeInterface>
        {
            readonly ConcurrentQueue<string> _events;

            public Handler2(ConcurrentQueue<string> events) => _events = events;

            public async Task Handle(ISomeInterface message)
            {
                Console.WriteLine(@"
Executing Handler2
");
                _events.Enqueue("Handled by Handler2");
            }
        }
    }
}