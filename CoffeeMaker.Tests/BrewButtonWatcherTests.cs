using CoffeeMaker.Hardware;
using CoffeeMaker.Hardware.Status;
using NSubstitute;
using NUnit.Framework;

namespace CoffeeMaker.Tests
{
    public class BrewButtonWatcherTests
    {
        private ICoffeeMakerAPI coffeeMakerApi;
        private IBrewButtonListener brewButtonListener;
        private BrewButtonWatcher sut;

        [SetUp]
        public void PerTestSetUp()
        {
            coffeeMakerApi = Substitute.For<ICoffeeMakerAPI>();
            brewButtonListener = Substitute.For<IBrewButtonListener>();
            sut = new BrewButtonWatcher(coffeeMakerApi, brewButtonListener);
        }

        [Test]
        public void NotifiesAboutBrewButtonPushed()
        {
            coffeeMakerApi.GetBrewButtonStatus().Returns(BrewButtonStatus.BREW_BUTTON_PUSHED);
            sut.CheckBrewButton();

            brewButtonListener.Received(1).BrewButtonPushed();
        }

        [Test]
        public void NotifiesAboutBrewButtonPushedAtEveryCheck()
        {
            coffeeMakerApi.GetBrewButtonStatus().Returns(BrewButtonStatus.BREW_BUTTON_PUSHED);
            sut.CheckBrewButton();
            sut.CheckBrewButton();
            sut.CheckBrewButton();

            brewButtonListener.Received(3).BrewButtonPushed();
        }
    }
}
