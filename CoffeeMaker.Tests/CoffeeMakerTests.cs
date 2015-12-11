using CoffeeMaker.Hardware;
using CoffeeMaker.Hardware.Status;
using NSubstitute;
using NUnit.Framework;

namespace CoffeeMaker.Tests
{
    [TestFixture]
    public class CoffeeMakerTests
    {
        private IBrewingCycle brewingCycle;
        private ICoffeeMakerAPI coffeeMakerApi;
        private IWarmingCycle warmingCycle;
        private CoffeeMaker sut;

        [SetUp]
        public void PerTestSetUp()
        {
            brewingCycle = Substitute.For<IBrewingCycle>();
            coffeeMakerApi = Substitute.For<ICoffeeMakerAPI>();
            warmingCycle = Substitute.For<IWarmingCycle>();
            sut = new CoffeeMaker(coffeeMakerApi, brewingCycle, warmingCycle);
        }

        [Test]
        public void PushingBrewButtonStartsBrewCycle()
        {
            sut.BrewButtonPushed();

            brewingCycle.Received(1).Start(Arg.Any<IStartBrewingRequest>());
        }

        [Test]
        public void TurnsLightOffWhenBrewingCycleStarts()
        {
            sut.BrewingCycleStarted();

            AssertLightTurnedOff();
        }

        [Test]
        public void TurnsLightOnWhenBrewingCycleCompletes()
        {
            sut.BrewingCycleCompleted();

            AssertLightTurnedOn();
        }

        [Test]
        public void TurnsLightOffWhenPotEmptyAfterBrewingCycleCompleted()
        {
            sut.PotEmpty();

            AssertLightTurnedOff();
        }

        [Test]
        public void PausesBrewingCycleWhenPotRemovedFromWarmerPlate()
        {
            sut.BrewingCycleStarted();
            sut.PotRemovedFromWarmerPlate();

            brewingCycle.Received(1).Pause();
        }

        [Test]
        public void ResumesBrewingCycleWhenPotReturnedToWarmerPlate()
        {
            sut.BrewingCycleStarted();
            sut.PotReturnedToWarmerPlate();

            brewingCycle.Received(1).Resume();
        }

        [Test]
        public void DoesNotPauseBrewingCycleWhenNotBrewing()
        {
            sut.PotRemovedFromWarmerPlate();

            brewingCycle.DidNotReceive().Pause();
        }

        [Test]
        public void DoesNotResumeBrewingCycleWhenNotBrewing()
        {
            sut.PotReturnedToWarmerPlate();

            brewingCycle.DidNotReceive().Resume();
        }

        [Test]
        public void DoesNotStartAnotherBrewingCycleWhileBrewing()
        {
            sut.BrewButtonPushed();
            sut.BrewingCycleStarted();

            sut.BrewButtonPushed();

            brewingCycle.Received(1).Start(Arg.Any<IStartBrewingRequest>());
        }

        [Test]
        public void DoesNotTurnOffLightWhileBrewing()
        {
            sut.BrewingCycleStarted();
            coffeeMakerApi.ClearReceivedCalls();

            sut.PotEmpty();

            coffeeMakerApi.DidNotReceive().SetIndicatorState(IndicatorState.INDICATOR_OFF);
        }

        [Test]
        public void WarmsCoffeeWhenCoffeeInPot()
        {
            sut.CoffeeInPot();

            warmingCycle.Received(1).Start();
        }

        [Test]
        public void StopWarmingCoffeeWhenPotEmpty()
        {
            sut.PotEmpty();

            warmingCycle.Received(1).Stop();
        }

        [Test]
        public void PausesWarmingCycleWhenPotRemoved()
        {
            sut.CoffeeInPot();
            sut.PotRemovedFromWarmerPlate();

            warmingCycle.Received(1).Pause();
        }

        [Test]
        public void DoesNotPauseWarmingCycleWhenCycleNotStarted()
        {
            sut.PotRemovedFromWarmerPlate();

            warmingCycle.DidNotReceive().Pause();
        }

        [Test]
        public void ResumesWarmingCycleWhenPotReturned()
        {
            sut.CoffeeInPot();
            sut.PotRemovedFromWarmerPlate();
            sut.PotReturnedToWarmerPlate();

            warmingCycle.Received(1).Resume();
        }

        [Test]
        public void DoesNotResumeWarmingCycleWhenCycleNotStarted()
        {
            sut.PotReturnedToWarmerPlate();

            warmingCycle.DidNotReceive().Resume();
        }

        private void AssertLightTurnedOff()
        {
            coffeeMakerApi.SetIndicatorState(IndicatorState.INDICATOR_OFF);
        }

        private void AssertLightTurnedOn()
        {
            coffeeMakerApi.SetIndicatorState(IndicatorState.INDICATOR_ON);
        }
    }
}
