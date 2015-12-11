using System;
using CoffeeMaker.Events;
using CoffeeMaker.Hardware;
using CoffeeMaker.Hardware.Status;
using NSubstitute;
using NUnit.Framework;

namespace CoffeeMaker.Tests
{
    public class BrewButtonWatcherTests
    {
        private ICoffeeMakerAPI coffeeMakerApi;
        private IObserver<BrewButtonPushed> brewButtonPushedObserver; 
        private BrewButtonWatcher sut;

        [SetUp]
        public void PerTestSetUp()
        {
            coffeeMakerApi = Substitute.For<ICoffeeMakerAPI>();
            brewButtonPushedObserver = Substitute.For<IObserver<BrewButtonPushed>>();
            sut = new BrewButtonWatcher(coffeeMakerApi);
            sut.Subscribe(brewButtonPushedObserver);
        }

        [Test]
        public void NotifiesAboutBrewButtonPushed()
        {
            coffeeMakerApi.GetBrewButtonStatus().Returns(BrewButtonStatus.BREW_BUTTON_PUSHED);
            sut.CheckBrewButton();

            brewButtonPushedObserver.Received(1).OnNext(Arg.Any<BrewButtonPushed>());
        }

        [Test]
        public void NotifiesAboutBrewButtonPushedAtEveryCheck()
        {
            coffeeMakerApi.GetBrewButtonStatus().Returns(BrewButtonStatus.BREW_BUTTON_PUSHED);
            sut.CheckBrewButton();
            sut.CheckBrewButton();
            sut.CheckBrewButton();

            brewButtonPushedObserver.Received(3).OnNext(Arg.Any<BrewButtonPushed>());
        }
    }
}
