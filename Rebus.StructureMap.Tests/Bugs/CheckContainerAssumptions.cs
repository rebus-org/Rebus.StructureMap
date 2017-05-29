using System.Linq;
using NUnit.Framework;
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
                c.For<IAmEmptyAndGeneric<ISomeInterface>>().Use<FirstClass>().Transient();
                c.For<IAmEmptyAndGeneric<SomeMessage>>().Use<SecondClass>().Transient();
            });

            var handlers = _container.GetAllInstances<IAmEmptyAndGeneric<SomeMessage>>().ToArray();

            Assert.That(handlers.Length, Is.EqualTo(2));
        }

        [Test]
        public void WorksAsAdvertised_2()
        {
            _container.Configure(c =>
            {
                // two handler types handling same interface
                c.For<IAmEmptyAndGeneric<ISomeInterface>>().Use<FirstClass>().Transient();
                c.For<IAmEmptyAndGeneric<ISomeInterface>>().Use<ThirdClass>().Transient();
            });

            var handlers = _container.GetAllInstances<IAmEmptyAndGeneric<SomeMessage>>().ToArray();

            Assert.That(handlers.Length, Is.EqualTo(2));
        }

        class FirstClass : IAmEmptyAndGeneric<ISomeInterface> { }

        class SecondClass : IAmEmptyAndGeneric<SomeMessage> { }

        class ThirdClass : IAmEmptyAndGeneric<ISomeInterface> { }

        public interface ISomeInterface { }

        public class SomeMessage : ISomeInterface { }

        public interface IAmEmptyAndGeneric<T> { }
    }
}