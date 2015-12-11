using CoffeeMaker.Hardware;
using CoffeeMaker.Hardware.Status;
using NSubstitute;
using NUnit.Framework;

namespace CoffeeMaker.Tests
{
    public class WarmingCycleTests
    {
        private ICoffeeMakerAPI coffeeMakerApi;
        private WarmingCycle sut;

        [SetUp]
        public void PerTestSetUp()
        {
            coffeeMakerApi = Substitute.For<ICoffeeMakerAPI>();
            sut = new WarmingCycle(coffeeMakerApi);
        }

        [Test]
        public void WarmsCoffee()
        {
            sut.Start();

            coffeeMakerApi.Received(1).SetWarmerState(WarmerState.WARMER_ON);
        }

        [Test]
        public void StopsWarmingCoffee()
        {
            sut.Stop();

            coffeeMakerApi.Received(1).SetWarmerState(WarmerState.WARMER_OFF);
        }

        [Test]
        public void PausesWarming()
        {
            sut.Start();
            sut.Pause();

            coffeeMakerApi.Received(1).SetWarmerState(WarmerState.WARMER_OFF);
        }

        [Test]
        public void ResumesWarming()
        {
            sut.Start();
            sut.Resume();

            coffeeMakerApi.Received(1).SetWarmerState(WarmerState.WARMER_ON);
        }

        [Test]
        public void DoesNotResumeWarmingWhenPotEmpty()
        {
            coffeeMakerApi.GetWarmerPlateStatus().Returns(WarmerPlateStatus.POT_EMPTY);
            sut.Start();
            coffeeMakerApi.ClearReceivedCalls();

            sut.Resume();

            coffeeMakerApi.DidNotReceive().SetWarmerState(WarmerState.WARMER_ON);
        }

        [Test]
        public void DoesNotPauseCycleWhenCycleNotStarted()
        {
            sut.Pause();

            coffeeMakerApi.DidNotReceive().SetWarmerState(WarmerState.WARMER_OFF);
        }

        [Test]
        public void DoesNotResumeCycleWhenCycleNotStarted()
        {
            coffeeMakerApi.GetWarmerPlateStatus().Returns(WarmerPlateStatus.POT_NOT_EMPTY);
            sut.Resume();

            coffeeMakerApi.DidNotReceive().SetWarmerState(WarmerState.WARMER_ON);
        }
    }
}
