using CoffeeMaker.Hardware;
using CoffeeMaker.Hardware.Status;
using NSubstitute;
using NUnit.Framework;

namespace CoffeeMaker.Tests
{
    public class BoilerWatcherTests
    {
        private ICoffeeMakerAPI coffeeMakerApi;
        private IBoilerListener boilerListener;
        private BoilerWatcher sut;

        [SetUp]
        public void PerTestSetUp()
        {
            coffeeMakerApi = Substitute.For<ICoffeeMakerAPI>();
            boilerListener = Substitute.For<IBoilerListener>();
            sut = new BoilerWatcher(coffeeMakerApi, boilerListener);
        }

        [Test]
        public void NotifiesAboutBoilerEmpty()
        {
            coffeeMakerApi.GetBoilerStatus().Returns(BoilerStatus.BOILER_EMPTY);
            sut.CheckBoilerContent();

            boilerListener.Received(1).BoilerEmpty();
        }

        [Test]
        public void DoesNotNotifyAboutBoilerEmptyWhenBoilerNotEmpty()
        {
            coffeeMakerApi.GetBoilerStatus().Returns(BoilerStatus.BOILER_NOT_EMPTY);
            sut.CheckBoilerContent();

            boilerListener.DidNotReceive().BoilerEmpty();
        }
    }
}
