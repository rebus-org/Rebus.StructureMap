using System;
using System.Linq;
using NUnit.Framework;
using Rebus.Extensions;
using Rebus.Tests.Contracts;
using StructureMap;

namespace Rebus.StructureMap.Tests.Bugs
{
    [TestFixture]
    public class CheckContainerAssumptions : FixtureBase
    {
        Container _container;

        protected override void SetUp()
        {
            _container = new Container();

            Using(_container);
        }

        [Test]
        public void WorksAsAdvertised_1()
        {
            _container.Configure(c =>
            {
                c.For<IAmGeneric<ISomeInterface>>().Use<FirstClass>().Transient();
                c.For<IAmGeneric<SomeMessage>>().Use<SecondClass>().Transient();
            });

            var handlers = _container.GetAllInstances<IAmGeneric<SomeMessage>>().ToArray();

            Assert.That(handlers.Length, Is.EqualTo(2), $@"Did not get the two expected instances - got these:

{string.Join(Environment.NewLine, handlers.Select(h => $"     {h.GetType().GetSimpleAssemblyQualifiedName()}"))}

That was weird.
");
        }

        [Test]
        public void WorksAsAdvertised_2()
        {
            _container.Configure(c =>
            {
                // two handler types handling same interface
                c.For<IAmGeneric<ISomeInterface>>().Use<FirstClass>().Transient();
                c.For<IAmGeneric<ISomeInterface>>().Use<ThirdClass>().Transient();
            });

            var handlers = _container.GetAllInstances<IAmGeneric<SomeMessage>>().ToArray();

            Assert.That(handlers.Length, Is.EqualTo(2));
        }

        public class FirstClass : IAmGeneric<ISomeInterface> { }

        public class SecondClass : IAmGeneric<SomeMessage> { }

        public class ThirdClass : IAmGeneric<ISomeInterface> { }

        public interface ISomeInterface { }

        public class SomeMessage : ISomeInterface { }

        public interface IAmGeneric<in T> { }
    }
}