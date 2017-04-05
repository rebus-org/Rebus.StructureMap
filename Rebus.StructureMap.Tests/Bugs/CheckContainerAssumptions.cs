using System.Linq;
using NUnit.Framework;
using StructureMap;

namespace Rebus.StructureMap.Tests.Bugs
{
    [TestFixture]
    public class CheckContainerAssumptions
    {
        [Test]
        public void WorksAsAdvertised()
        {
            var container = new Container();

            container.Configure(c =>
            {
                c.For<IAmEmptyAndGeneric<ISomeInterface>>().Use<FirstClass>().Transient();
                //c.For<IAmEmptyAndGeneric<SomeMessage>>().Use<SecondClass>().Transient();
            });

            var handlers = container.GetAllInstances<IAmEmptyAndGeneric<SomeMessage>>().ToArray();

            Assert.That(handlers.Length, Is.EqualTo(2));
        }

        class FirstClass : IAmEmptyAndGeneric<ISomeInterface> { }

        class SecondClass : IAmEmptyAndGeneric<SomeMessage> { }

        public interface ISomeInterface { }

        public class SomeMessage : ISomeInterface { }

        public interface IAmEmptyAndGeneric<T> { }
    }
}