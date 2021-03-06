﻿namespace IoCComparison.AutoregisterTests
{
    using System.Collections.Generic;
    using System.Linq;
    using Autofac;
    using AutoregisteredClasses.Interfaces;
    using AutoregisteredClasses.Services;
    using AutoregisteredClasses.Validators;
    using IoCComparison.AutoregisterTests.TypeExtensions;

    using NUnit.Framework;

    [TestFixture]
    public class AutofacTest
    {
        [Test]
        public void CanMakeBusinessProcess()
        {
            ContainerBuilder builder = new ContainerBuilder();

            builder.RegisterAssemblyTypes(typeof(CustomerService).Assembly)
                .AsImplementedInterfaces()
                .Except<BusinessProcess>(rb => rb.AsSelf());

            IContainer container = builder.Build();

            BusinessProcess businessProcess = container.Resolve<BusinessProcess>();

            Assert.IsNotNull(businessProcess);
        }

        [Test]
        public void CanMakeBusinessProcessTwoScans()
        {
            ContainerBuilder builder = new ContainerBuilder();

            // not as good as the Except<T>(rb => ...) syntax
            // go through the target assembly twice 
            // - once for object without interfaces, once for those with interfaces
            builder.RegisterAssemblyTypes(typeof(BusinessProcess).Assembly).Where(t => ! t.HasInterfaces());
            builder.RegisterAssemblyTypes(typeof(CustomerService).Assembly).AsImplementedInterfaces();
            IContainer container = builder.Build();

            BusinessProcess businessProcess = container.Resolve<BusinessProcess>();

            Assert.IsNotNull(businessProcess);
        }

        [Test]
        public void CanMakeBusinessProcessOneScan()
        {
            ContainerBuilder builder = new ContainerBuilder();

            // not as good as the Except<T>(rb => ...) syntax
            // instead of redundant scans, this version has redundant registrations
            // e.g. CustomerService is registered as both CustomerService and ICustomerService,
            // even though we only care about the latter
            builder.RegisterAssemblyTypes(typeof(CustomerService).Assembly)
                .AsImplementedInterfaces().AsSelf();
            IContainer container = builder.Build();

            BusinessProcess businessProcess = container.Resolve<BusinessProcess>();

            Assert.IsNotNull(businessProcess);
        }

        [Test]
        public void CanMakeSingletonInstance()
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterAssemblyTypes(typeof(CustomerService).Assembly)
                .AsImplementedInterfaces().AsSelf()
                .SingleInstance();
            IContainer container = builder.Build();

            BusinessProcess businessProcess1 = container.Resolve<BusinessProcess>();
            BusinessProcess businessProcess2 = container.Resolve<BusinessProcess>();

            Assert.AreSame(businessProcess1, businessProcess2);
        }

        [Test]
        public void CanMakeTransientInstance()
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterAssemblyTypes(typeof(CustomerService).Assembly)
                .AsImplementedInterfaces().AsSelf();
            IContainer container = builder.Build();

            BusinessProcess businessProcess1 = container.Resolve<BusinessProcess>();
            BusinessProcess businessProcess2 = container.Resolve<BusinessProcess>();

            Assert.AreNotSame(businessProcess1, businessProcess2);
        }


        [Test]
        public void CanMakeTransientInstanceWithSingletonDependencies()
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterAssemblyTypes(typeof (CustomerService).Assembly)
                .Where(t => t.HasInterfaces())
                .AsImplementedInterfaces().SingleInstance()
                .Except<BusinessProcess>(rb => rb.AsSelf());
            
            IContainer container = builder.Build();

            BusinessProcess businessProcess1 = container.Resolve<BusinessProcess>();
            BusinessProcess businessProcess2 = container.Resolve<BusinessProcess>();

            Assert.AreNotSame(businessProcess1, businessProcess2);
            Assert.AreSame(businessProcess1.CustomerService, businessProcess2.CustomerService);
            Assert.AreSame(businessProcess1.OrderService, businessProcess2.OrderService);
        }

        [Test]
        public void CanGetAllValidators()
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterAssemblyTypes(typeof(IValidator).Assembly).AsImplementedInterfaces();
            IContainer container = builder.Build();

            var validators = container.Resolve<IEnumerable<IValidator>>();

            Assert.IsNotNull(validators);
            Assert.AreEqual(3, validators.Count());
        }

        [Test]
        public void CanFilterOutValidatorRegistrationsWithWhere()
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterAssemblyTypes(typeof(IValidator).Assembly)
                .Where(t => t != typeof(FailValidator))
                .AsImplementedInterfaces();
            IContainer container = builder.Build();

            // excluding the FailValidator should leave 2 of them
            var validators = container.Resolve<IEnumerable<IValidator>>();

            Assert.IsNotNull(validators);
            Assert.AreEqual(2, validators.Count());
        }

        [Test]
        public void CanFilterOutValidatorRegistrationsWithExcept()
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterAssemblyTypes(typeof(IValidator).Assembly)
                .AsImplementedInterfaces()
                .Except<FailValidator>();
            IContainer container = builder.Build();

            // excluding the FailValidator should leave 2 of them
            var validators = container.Resolve<IEnumerable<IValidator>>();

            Assert.IsNotNull(validators);
            Assert.AreEqual(2, validators.Count());
        }

    }
}
