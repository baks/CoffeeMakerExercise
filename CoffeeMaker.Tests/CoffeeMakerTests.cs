using CoffeeMaker.Events;
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
            sut.OnNext(new BrewButtonPushed());

            brewingCycle.Received(1).Start(Arg.Any<IStartBrewingRequest>());
        }

        [Test]
        public void TurnsLightOffWhenBrewingCycleStarts()
        {
            sut.OnNext(new BrewingCycleStarted());

            AssertLightTurnedOff();
        }

        [Test]
        public void TurnsLightOnWhenBrewingCycleCompletes()
        {
            sut.OnNext(new BrewingCycleCompleted());

            AssertLightTurnedOn();
        }

        [Test]
        public void TurnsLightOffWhenPotEmptyAfterBrewingCycleCompleted()
        {
            sut.OnNext(new PotEmpty());

            AssertLightTurnedOff();
        }

        [Test]
        public void PausesBrewingCycleWhenPotRemovedFromWarmerPlate()
        {
            sut.OnNext(new BrewingCycleStarted());
            sut.OnNext(new PotRemoved());

            brewingCycle.Received(1).Pause();
        }

        [Test]
        public void ResumesBrewingCycleWhenPotReturnedToWarmerPlate()
        {
            sut.OnNext(new BrewingCycleStarted());
            sut.OnNext(new PotReturned());

            brewingCycle.Received(1).Resume();
        }

        [Test]
        public void DoesNotPauseBrewingCycleWhenNotBrewing()
        {
            sut.OnNext(new PotRemoved());

            brewingCycle.DidNotReceive().Pause();
        }

        [Test]
        public void DoesNotResumeBrewingCycleWhenNotBrewing()
        {
            sut.OnNext(new PotReturned());

            brewingCycle.DidNotReceive().Resume();
        }

        [Test]
        public void DoesNotStartAnotherBrewingCycleWhileBrewing()
        {
            sut.OnNext(new BrewButtonPushed());
            sut.OnNext(new BrewingCycleStarted());

            sut.OnNext(new BrewButtonPushed());

            brewingCycle.Received(1).Start(Arg.Any<IStartBrewingRequest>());
        }

        [Test]
        public void DoesNotTurnOffLightWhileBrewing()
        {
            sut.OnNext(new BrewingCycleStarted());
            coffeeMakerApi.ClearReceivedCalls();

            sut.OnNext(new PotEmpty());

            coffeeMakerApi.DidNotReceive().SetIndicatorState(IndicatorState.INDICATOR_OFF);
        }

        [Test]
        public void WarmsCoffeeWhenCoffeeInPot()
        {
            sut.OnNext(new CoffeeInPot());

            warmingCycle.Received(1).Start();
        }

        [Test]
        public void StopWarmingCoffeeWhenPotEmpty()
        {
            sut.OnNext(new PotEmpty());

            warmingCycle.Received(1).Stop();
        }

        [Test]
        public void PausesWarmingCycleWhenPotRemoved()
        {
            sut.OnNext(new CoffeeInPot());
            sut.OnNext(new PotRemoved());

            warmingCycle.Received(1).Pause();
        }

        [Test]
        public void DoesNotPauseWarmingCycleWhenCycleNotStarted()
        {
            sut.OnNext(new PotRemoved());

            warmingCycle.DidNotReceive().Pause();
        }

        [Test]
        public void ResumesWarmingCycleWhenPotReturned()
        {
            sut.OnNext(new CoffeeInPot());
            sut.OnNext(new PotRemoved());
            sut.OnNext(new PotReturned());

            warmingCycle.Received(1).Resume();
        }

        [Test]
        public void DoesNotResumeWarmingCycleWhenCycleNotStarted()
        {
            sut.OnNext(new PotReturned());

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
