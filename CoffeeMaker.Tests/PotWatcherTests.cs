using CoffeeMaker.Hardware;
using CoffeeMaker.Hardware.Status;
using NSubstitute;
using NUnit.Framework;

namespace CoffeeMaker.Tests
{
    public class PotWatcherTests
    {
        private ICoffeeMakerAPI coffeeMakerApi;
        private IPotContentListener potContentListener;
        private IPotPositionListener potPositionListener;
        private PotWatcher sut;

        [SetUp]
        public void PerTestSetUp()
        {
            coffeeMakerApi = Substitute.For<ICoffeeMakerAPI>();
            potContentListener = Substitute.For<IPotContentListener>();
            potPositionListener = Substitute.For<IPotPositionListener>();
            sut = new PotWatcher(coffeeMakerApi, potContentListener, potPositionListener);
        }

        [Test]
        public void NotifiesAboutPotRemovedFromWarmerPlate()
        {
            RemovePot();

            potPositionListener.Received(1).PotRemovedFromWarmerPlate();
        }

        [Test]
        public void NotifiesAboutPotReturnedToWarmerPlateWhenNotEmpty()
        {
            RemovePot();
            ReturnNotEmptyPot();

            potPositionListener.Received(1).PotReturnedToWarmerPlate();
        }

        [Test]
        public void NotifiesAboutPotReturnedToWarmerPlateWhenEmpty()
        {
            RemovePot();
            ReturnEmptyPot();

            potPositionListener.Received(1).PotReturnedToWarmerPlate();
        }

        [Test]
        public void NotifiesAboutPotReturnedWhenPotWasNotRemoved()
        {
            ReturnNotEmptyPot();

            potPositionListener.Received(1).PotReturnedToWarmerPlate();
        }

        [Test]
        public void DoesNotNotifyMoreThanOnceAboutPotRemoved()
        {
            RemovePot();
            sut.CheckPotPosition();
            sut.CheckPotPosition();

            potPositionListener.Received(1).PotRemovedFromWarmerPlate();
        }

        [Test]
        public void DoesNotNotifyMoreThanOnceAboutPotReturned()
        {
            RemovePot();

            ReturnNotEmptyPot();
            sut.CheckPotPosition();
            sut.CheckPotPosition();

            potPositionListener.Received(1).PotReturnedToWarmerPlate();
        }

        [Test]
        public void NotifiesAboutPotEmpty()
        {
            CoffeeInPot();
            PotEmpty();

            potContentListener.Received(1).PotEmpty();
        }

        [Test]
        public void NotifiesAboutCoffeeInPot()
        {
            CoffeeInPot();

            potContentListener.Received(1).CoffeeInPot();
        }

        [Test]
        public void DoesNotNotifyMoreThanOnceAboutPotEmpty()
        {
            CoffeeInPot();
            PotEmpty();
            sut.CheckPotContent();
            sut.CheckPotContent();

            potContentListener.Received(1).PotEmpty();
        }

        [Test]
        public void DoesNotNotifyMoreThanOnceAboutCoffeeInPot()
        {
            CoffeeInPot();
            sut.CheckPotContent();
            sut.CheckPotContent();

            potContentListener.Received(1).CoffeeInPot();
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
