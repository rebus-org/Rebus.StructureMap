﻿using System;
using System.Linq;
using Rebus.Activation;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Handlers;
using Rebus.Tests.Contracts.Activation;
using StructureMap;

namespace Rebus.StructureMap.Tests
{
    public class StructureMapContainerAdapterFactory : IActivationContext
    {
        public IHandlerActivator CreateActivator(Action<IHandlerRegistry> configureHandlers, out IActivatedContainer container)
        {
            var sm = new Container();

            configureHandlers.Invoke(new HandlerRegistry(sm));

            container = new ActivatedContainer(sm);

            return new StructureMapContainerAdapter(sm);
        }

        public IBus CreateBus(Action<IHandlerRegistry> configureHandlers, Func<RebusConfigurer, RebusConfigurer> configureBus, out IActivatedContainer container)
        {
            var sm = new Container();

            configureHandlers.Invoke(new HandlerRegistry(sm));
            container = new ActivatedContainer(sm);

            return configureBus(Configure.With(new StructureMapContainerAdapter(sm))).Start();
        }

        class ActivatedContainer : IActivatedContainer
        {
            readonly Container _container;

            public ActivatedContainer(Container container)
            {
                _container = container;
            }

            public void Dispose() => _container.Dispose();

            public IBus ResolveBus() => _container.GetInstance<IBus>();
        }

        class HandlerRegistry : IHandlerRegistry
        {
            readonly Container _container;

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
                return type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandleMessages<>))
                    .ToArray();
            }
        }
    }
}