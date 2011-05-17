﻿using Castle.Core;
using Castle.Windsor;
using Castle.MicroKernel.Registration;
using NUnit.Framework;

namespace IoCComparison
{
    using Castle.Facilities.FactorySupport;

    [TestFixture]
    public class WindsorTest
    {
        [Test]
        public void CanMakeSweetShopWithVanillaJellybeansWithOldSyntax()
        {
            WindsorContainer container = new WindsorContainer();
            // disable the deprecated! unclean! warning that we know the "AddComponent" method will emit 
            #pragma warning disable 612,618
            container.AddComponent<SweetShop>();
            container.AddComponent<SweetVendingMachine>();
            container.AddComponent<IJellybeanDispenser, VanillaJellybeanDispenser>();
            #pragma warning restore 612,618

            SweetShop sweetShop = container.Resolve<SweetShop>();

            Assert.AreEqual(Jellybean.Vanilla, sweetShop.DispenseJellyBean());
        }

        [Test]
        public void CanMakeSweetShopWithVanillaJellybeans()
        {
            WindsorContainer container = new WindsorContainer();
            container.Register(
                Component.For<SweetShop>(),
                Component.For<SweetVendingMachine>(),
                Component.For<IJellybeanDispenser>().ImplementedBy<VanillaJellybeanDispenser>());

            SweetShop sweetShop = container.Resolve<SweetShop>();

            Assert.AreEqual(Jellybean.Vanilla, sweetShop.DispenseJellyBean());
        }

        [Test]
        public void CanMakeSweetShopWithStrawberryJellybeans()
        {
            WindsorContainer container = new WindsorContainer();
            container.Register(
                Component.For<SweetShop>(),
                Component.For<SweetVendingMachine>(),
                Component.For<IJellybeanDispenser>().ImplementedBy<StrawberryJellybeanDispenser>());

            SweetShop sweetShop = container.Resolve<SweetShop>();

            Assert.AreEqual(Jellybean.Strawberry, sweetShop.DispenseJellyBean());
        }
        
        [Test]
        public void JellybeanDispenserHasNewInstanceEachTime()
        {
            WindsorContainer container = new WindsorContainer();
            // windsor seems to default to retained singletons, 
            // so we add LifestyleType.Transient to make them new each time
            container.Register(
                Component.For<SweetShop>().LifeStyle.Is(LifestyleType.Transient),
                Component.For<SweetVendingMachine>().LifeStyle.Is(LifestyleType.Transient),
                Component.For<IJellybeanDispenser>().ImplementedBy<VanillaJellybeanDispenser>()
                    .LifeStyle.Is(LifestyleType.Transient));


            SweetShop sweetShop = container.Resolve<SweetShop>();
            SweetShop sweetShop2 = container.Resolve<SweetShop>();

            Assert.IsFalse(ReferenceEquals(sweetShop, sweetShop2), "Root objects are equal");
            Assert.IsFalse(ReferenceEquals(sweetShop.SweetVendingMachine, sweetShop2.SweetVendingMachine), "Contained objects are equal");
            Assert.IsFalse(ReferenceEquals(sweetShop.SweetVendingMachine.JellybeanDispenser, sweetShop2.SweetVendingMachine.JellybeanDispenser), "services are equal");
        }

        [Test]
        public void CanMakeSingletonJellybeanDispenser()
        {
            WindsorContainer container = new WindsorContainer();

            container.Register(
                Component.For<SweetShop>().LifeStyle.Is(LifestyleType.Transient),
                Component.For<SweetVendingMachine>().LifeStyle.Is(LifestyleType.Transient),
                Component.For<IJellybeanDispenser>().ImplementedBy<VanillaJellybeanDispenser>()
                    .LifeStyle.Is(LifestyleType.Singleton));

            SweetShop sweetShop = container.Resolve<SweetShop>();
            SweetShop sweetShop2 = container.Resolve<SweetShop>();

            Assert.IsFalse(ReferenceEquals(sweetShop, sweetShop2), "Root objects are equal");
            Assert.IsFalse(ReferenceEquals(sweetShop.SweetVendingMachine, sweetShop2.SweetVendingMachine), "Contained objects are equal");

            // should be same service
            Assert.IsTrue(ReferenceEquals(sweetShop.SweetVendingMachine.JellybeanDispenser, sweetShop2.SweetVendingMachine.JellybeanDispenser), "services are not equal");
        }
        
        [Test]
        public void CanMakeAniseedRootObject()
        {
            WindsorContainer container = new WindsorContainer();
            container.Register(Component.For<SweetShop>().ImplementedBy<AniseedSweetShop>());

            SweetShop sweetShop = container.Resolve<SweetShop>();

            Assert.AreEqual(Jellybean.Aniseed, sweetShop.DispenseJellyBean());
        }

        [Test]
        public void CanUseAnyJellybeanDispenser()
        {
            WindsorContainer container = new WindsorContainer();
            container.Register(
                Component.For<SweetShop>(),
                Component.For<SweetVendingMachine>(),
                Component.For<IJellybeanDispenser>()
                    .ImplementedBy<AnyJellybeanDispenser>()
                    .Parameters(Parameter.ForKey("jellybean").Eq("Lemon")));
                
            SweetShop sweetShop = container.Resolve<SweetShop>();

            Assert.AreEqual(Jellybean.Lemon, sweetShop.DispenseJellyBean());
        }

        [Test]
        public void CanUseConstructedObject()
        {
            WindsorContainer container = new WindsorContainer();
            container.Register(
                Component.For<SweetShop>(),
                Component.For<SweetVendingMachine>(),
                Component.For<IJellybeanDispenser>().Instance(new AnyJellybeanDispenser(Jellybean.Cocoa)));

            SweetShop sweetShop = container.Resolve<SweetShop>();

            Assert.AreEqual(Jellybean.Cocoa, sweetShop.DispenseJellyBean());
        }

        [Test]
        public void CanUseObjectFactory()
        {
            WindsorContainer container = new WindsorContainer();
            container.AddFacility<FactorySupportFacility>();
            container.Register(
                Component.For<IJellybeanDispenser>()
                    .UsingFactoryMethod((c, t) => new AnyJellybeanDispenser(Jellybean.Orange)),
                Component.For<SweetShop>(),
                Component.For<SweetVendingMachine>());

            SweetShop sweetShop = container.Resolve<SweetShop>();

            Assert.AreEqual(Jellybean.Orange, sweetShop.DispenseJellyBean());
        }
    }

}
