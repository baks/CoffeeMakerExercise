using System;
using CoffeeMaker.Events;
using CoffeeMaker.Hardware;
using CoffeeMaker.Hardware.Status;
using NSubstitute;
using NUnit.Framework;

namespace CoffeeMaker.Tests
{
    public class PotWatcherTests
    {
        private ICoffeeMakerAPI coffeeMakerApi;
        private IObserver<CoffeeInPot> coffeeInPotObserver;
        private IObserver<PotEmpty> potEmptyObserver; 
        private IObserver<PotRemoved> potRemovedObserver;
        private IObserver<PotReturned> potReturnedObserver;   
        private PotWatcher sut;

        [SetUp]
        public void PerTestSetUp()
        {
            coffeeMakerApi = Substitute.For<ICoffeeMakerAPI>();
            potEmptyObserver = Substitute.For<IObserver<PotEmpty>>();
            coffeeInPotObserver = Substitute.For<IObserver<CoffeeInPot>>();
            potRemovedObserver = Substitute.For<IObserver<PotRemoved>>();
            potReturnedObserver = Substitute.For<IObserver<PotReturned>>();
            sut = new PotWatcher(coffeeMakerApi);

            sut.Subscribe(coffeeInPotObserver);
            sut.Subscribe(potEmptyObserver);
            sut.Subscribe(potRemovedObserver);
            sut.Subscribe(potReturnedObserver);
        }

        [Test]
        public void NotifiesAboutPotRemovedFromWarmerPlate()
        {
            RemovePot();

            potRemovedObserver.Received(1).OnNext(Arg.Any<PotRemoved>());
        }

        [Test]
        public void NotifiesAboutPotReturnedToWarmerPlateWhenNotEmpty()
        {
            RemovePot();
            ReturnNotEmptyPot();

            potReturnedObserver.Received(1).OnNext(Arg.Any<PotReturned>());
        }

        [Test]
        public void NotifiesAboutPotReturnedToWarmerPlateWhenEmpty()
        {
            RemovePot();
            ReturnEmptyPot();

            potReturnedObserver.Received(1).OnNext(Arg.Any<PotReturned>());
        }

        [Test]
        public void NotifiesAboutPotReturnedWhenPotWasNotRemoved()
        {
            ReturnNotEmptyPot();

            potReturnedObserver.Received(1).OnNext(Arg.Any<PotReturned>());
        }

        [Test]
        public void DoesNotNotifyMoreThanOnceAboutPotRemoved()
        {
            RemovePot();
            sut.CheckPotPosition();
            sut.CheckPotPosition();

            potRemovedObserver.Received(1).OnNext(Arg.Any<PotRemoved>());
        }

        [Test]
        public void DoesNotNotifyMoreThanOnceAboutPotReturned()
        {
            RemovePot();

            ReturnNotEmptyPot();
            sut.CheckPotPosition();
            sut.CheckPotPosition();

            potReturnedObserver.Received(1).OnNext(Arg.Any<PotReturned>());
        }

        [Test]
        public void NotifiesAboutPotEmpty()
        {
            CoffeeInPot();
            PotEmpty();

            potEmptyObserver.Received(1).OnNext(Arg.Any<PotEmpty>());
        }

        [Test]
        public void NotifiesAboutCoffeeInPot()
        {
            CoffeeInPot();

            coffeeInPotObserver.Received(1).OnNext(Arg.Any<CoffeeInPot>());
        }

        [Test]
        public void DoesNotNotifyMoreThanOnceAboutPotEmpty()
        {
            CoffeeInPot();
            PotEmpty();
            sut.CheckPotContent();
            sut.CheckPotContent();

            potEmptyObserver.Received(1).OnNext(Arg.Any<PotEmpty>());
        }

        [Test]
        public void DoesNotNotifyMoreThanOnceAboutCoffeeInPot()
        {
            CoffeeInPot();
            sut.CheckPotContent();
            sut.CheckPotContent();

            coffeeInPotObserver.Received(1).OnNext(Arg.Any<CoffeeInPot>());
        }

        private void RemovePot()
        {
            coffeeMakerApi.GetWarmerPlateStatus().Returns(WarmerPlateStatus.WARMER_EMPTY);
            sut.CheckPotPosition();
        }

        private void ReturnNotEmptyPot()
        {
            coffeeMakerApi.GetWarmerPlateStatus().Returns(WarmerPlateStatus.POT_NOT_EMPTY);
            sut.CheckPotPosition();
        }

        private void ReturnEmptyPot()
        {
            coffeeMakerApi.GetWarmerPlateStatus().Returns(WarmerPlateStatus.POT_EMPTY);
            sut.CheckPotPosition();
        }

        private void CoffeeInPot()
        {
            coffeeMakerApi.GetWarmerPlateStatus().Returns(WarmerPlateStatus.POT_NOT_EMPTY);
            sut.CheckPotContent();
        }

        private void PotEmpty()
        {
            coffeeMakerApi.GetWarmerPlateStatus().Returns(WarmerPlateStatus.POT_EMPTY);
            sut.CheckPotContent();
        }
    }
}
