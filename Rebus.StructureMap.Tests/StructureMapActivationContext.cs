using System;
using System.Linq;
#if NETSTANDARD1_6
using System.Reflection;
#endif
using Rebus.Activation;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Handlers;
using Rebus.Tests.Contracts.Activation;
using StructureMap;

namespace Rebus.StructureMap.Tests
{
    public class StructureMapActivationContext : IActivationContext
    {
        public IHandlerActivator CreateActivator(Action<IHandlerRegistry> configureHandlers, out IActivatedContainer container)
        {
            var structureMapContainer = new Container();
            configureHandlers(new HandlerRegistry(structureMapContainer));

            container = new ActivatedContainer(structureMapContainer);
            return new StructureMapContainerAdapter(structureMapContainer);
        }

        public IBus CreateBus(Action<IHandlerRegistry> configureHandlers, Func<RebusConfigurer, RebusConfigurer> configureBus, out IActivatedContainer container)
        {
            var structureMapContainer = new Container();
            configureHandlers(new HandlerRegistry(structureMapContainer));

            container = new ActivatedContainer(structureMapContainer);

            return configureBus(Configure.With(new StructureMapContainerAdapter(structureMapContainer))).Start();
        }

        private class HandlerRegistry : IHandlerRegistry
        {
            private readonly Container _container;

            public HandlerRegistry(Container container)
            {
                _container = container;
            }

            public IHandlerRegistry Register<THandler>() where THandler : class, IHandleMessages
            {
                _container.Configure(c =>
                {
                    foreach (var handler in GetHandlerInterfaces(typeof(THandler)))
                    {
                        Console.WriteLine($"IHandleMessages<{handler.GetGenericArguments().First().Name}> => {typeof(THandler).Name}");
                        c.For(handler).Use(typeof(THandler)).Transient();
                    }
                });

                return this;
            }

            Type[] GetHandlerInterfaces(Type type)
            {
#if NETSTANDARD1_6
            return type.GetTypeInfo().GetInterfaces()
                .Where(i => i.GetTypeInfo().IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleMessages<>))
                .ToArray();
#else
                return type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleMessages<>))
                    .ToArray();
#endif
            }
        }

        private class ActivatedContainer : IActivatedContainer
        {
            private readonly Container _container;

            public ActivatedContainer(Container container)
            {
                _container = container;
            }

            public void Dispose()
            {
                _container.Dispose();
            }

            public IBus ResolveBus()
            {
                return _container.GetInstance<IBus>();
            }
        }
    }
}