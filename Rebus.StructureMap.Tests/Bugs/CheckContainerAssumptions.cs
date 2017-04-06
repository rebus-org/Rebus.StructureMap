using System.Linq;
using NUnit.Framework;
using StructureMap;

namespace Rebus.StructureMap.Tests.Bugs
{
    [TestFixture]
    public class CheckContainerAssumptions
    {
        [Test]
        public void WorksAsAdvertised_1()
        {
            var container = new Container();

            container.Configure(c =>
            {
                c.For<IAmEmptyAndGeneric<ISomeInterface>>().Use<FirstClass>().Transient();
                c.For<IAmEmptyAndGeneric<SomeMessage>>().Use<SecondClass>().Transient();
            });

            var handlers = container.GetAllInstances<IAmEmptyAndGeneric<SomeMessage>>().ToArray();

            Assert.That(handlers.Length, Is.EqualTo(2));
        }

        [Test]
        public void WorksAsAdvertised_2()
        {
            var container = new Container();

            container.Configure(c =>
            {
                // two handler types handling same interface
                c.For<IAmEmptyAndGeneric<ISomeInterface>>().Use<FirstClass>().Transient();
                c.For<IAmEmptyAndGeneric<ISomeInterface>>().Use<ThirdClass>().Transient();
            });

            var handlers = container.GetAllInstances<IAmEmptyAndGeneric<SomeMessage>>().ToArray();

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