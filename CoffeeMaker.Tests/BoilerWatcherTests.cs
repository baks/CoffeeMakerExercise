using System;
using CoffeeMaker.Events;
using CoffeeMaker.Hardware;
using CoffeeMaker.Hardware.Status;
using NSubstitute;
using NUnit.Framework;

namespace CoffeeMaker.Tests
{
    public class BoilerWatcherTests
    {
        private ICoffeeMakerAPI coffeeMakerApi;
        private IObserver<BoilerEmpty> boilerEmptyObserver; 
        private BoilerWatcher sut;

        [SetUp]
        public void PerTestSetUp()
        {
            coffeeMakerApi = Substitute.For<ICoffeeMakerAPI>();
            boilerEmptyObserver = Substitute.For<IObserver<BoilerEmpty>>();
            sut = new BoilerWatcher(coffeeMakerApi);
            sut.Subscribe(boilerEmptyObserver);
        }

        [Test]
        public void NotifiesAboutBoilerEmpty()
        {
            coffeeMakerApi.GetBoilerStatus().Returns(BoilerStatus.BOILER_EMPTY);
            sut.CheckBoilerContent();

            boilerEmptyObserver.Received(1).OnNext(Arg.Any<BoilerEmpty>());
        }

        [Test]
        public void DoesNotNotifyAboutBoilerEmptyWhenBoilerNotEmpty()
        {
            coffeeMakerApi.GetBoilerStatus().Returns(BoilerStatus.BOILER_NOT_EMPTY);
            sut.CheckBoilerContent();

            boilerEmptyObserver.DidNotReceive().OnNext(Arg.Any<BoilerEmpty>());
        }
    }
}
