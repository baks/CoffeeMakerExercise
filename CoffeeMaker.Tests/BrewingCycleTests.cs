using Automatonymous;
using CoffeeMaker.Hardware;
using CoffeeMaker.Hardware.Status;
using NSubstitute;
using NUnit.Framework;

namespace CoffeeMaker.Tests
{
    [TestFixture]
    public class BrewingCycleTests : AutomatonymousStateMachine<BrewingCycleTests>
    {
        public State NotChangedByTest { get; private set; }
        public State Boiling { get; private set; }

        private ICoffeeMakerAPI coffeeMakerApi;
        private IStartBrewingRequest startBrewingRequest;
        private IBrewingCycleListener brewingCycleListener;
        private BrewingCycle sut;

        [SetUp]
        public void PerTestSetUp()
        {
            ResetStateToDefault(NotChangedByTest);
            coffeeMakerApi = Substitute.For<ICoffeeMakerAPI>();
            startBrewingRequest = Substitute.For<IStartBrewingRequest>();
            brewingCycleListener = Substitute.For<IBrewingCycleListener>();
            sut = new BrewingCycle(coffeeMakerApi);
            sut.AddListener(brewingCycleListener);
        }

        [Test]
        public void MakesCoffee()
        {
            coffeeMakerApi.When(api => api.SetBoilerState(BoilerState.BOILER_ON))
                .Then(Boiling)
                .Do(ci =>
                {
                    sut.BoilerEmpty();
                });
            coffeeMakerApi.When(api => api.SetBoilerState(BoilerState.BOILER_OFF))
                .Expect(Boiling);

            StartBrewingCycle();

            coffeeMakerApi.Received(1).SetBoilerState(BoilerState.BOILER_ON);
            coffeeMakerApi.Received().SetReliefValveState(ReliefValveState.VALVE_CLOSED);
            coffeeMakerApi.Received(1).SetBoilerState(BoilerState.BOILER_OFF);
        }

        [Test]
        public void NotifiesThatWarmerIsEmpty()
        {
            coffeeMakerApi.GetWarmerPlateStatus().Returns(WarmerPlateStatus.WARMER_EMPTY);

            sut.Start(startBrewingRequest);

            startBrewingRequest.Received(1).CannotStartBrewingBecausePotIsNotInWarmerPlate();
        }

        [Test]
        public void NotifiesThatBoilerIsEmpty()
        {
            coffeeMakerApi.GetWarmerPlateStatus().Returns(WarmerPlateStatus.POT_EMPTY);
            coffeeMakerApi.GetBoilerStatus().Returns(BoilerStatus.BOILER_EMPTY);

            sut.Start(startBrewingRequest);

            startBrewingRequest.Received(1).CannotStartBrewingBecauseBoilerIsEmpty();
        }

        [Test]
        public void StartsBrewingCycle()
        {
            StartBrewingCycle();

            coffeeMakerApi.Received(1).SetBoilerState(BoilerState.BOILER_ON);
        }

        [Test]
        public void PausesBrewingCycle()
        {
            StartBrewingCycle();

            sut.Pause();

            coffeeMakerApi.Received(1).SetReliefValveState(ReliefValveState.VALVE_OPEN);
            coffeeMakerApi.Received(1).SetBoilerState(BoilerState.BOILER_OFF);
        }

        [Test]
        public void ResumesBrewingCycle()
        {
            StartBrewingCycle();

            sut.Pause();

            coffeeMakerApi.ClearReceivedCalls();

            sut.Resume();

            coffeeMakerApi.Received(1).SetReliefValveState(ReliefValveState.VALVE_CLOSED);
            coffeeMakerApi.Received(1).SetBoilerState(BoilerState.BOILER_ON);
        }

        [Test]
        public void NotifiesAboutBrewingCycleCompleted()
        {
            StartBrewingCycle();

            sut.BoilerEmpty();

            brewingCycleListener.Received(1).BrewingCycleCompleted();
        }

        [Test]
        public void DoesNotNotifyAboutBrewingCycleCompletedWhenCycleNotStarted()
        {
            sut.BoilerEmpty();

            brewingCycleListener.DidNotReceive().BrewingCycleCompleted();
        }

        [Test]
        public void DoesNotPauseBrewingWhenCycleNotStarted()
        {
            sut.Pause();

            coffeeMakerApi.DidNotReceive().SetReliefValveState(ReliefValveState.VALVE_OPEN);
            coffeeMakerApi.DidNotReceive().SetBoilerState(BoilerState.BOILER_OFF);
        }

        [Test]
        public void DoesNotResumeBrewingCycleWhenCycleNotPaused()
        {
            StartBrewingCycle();

            coffeeMakerApi.ClearReceivedCalls();

            sut.Resume();

            coffeeMakerApi.DidNotReceive().SetReliefValveState(ReliefValveState.VALVE_CLOSED);
            coffeeMakerApi.DidNotReceive().SetBoilerState(BoilerState.BOILER_ON);
        }

        [Test]
        public void DoesNotResumeBrewingCycleWhenCycleNotStarted()
        {
            sut.Resume();

            coffeeMakerApi.DidNotReceive().SetReliefValveState(ReliefValveState.VALVE_CLOSED);
            coffeeMakerApi.DidNotReceive().SetBoilerState(BoilerState.BOILER_ON);
        }

        [Test]
        public void NotifiesAboutBrewingCycleStarted()
        {
            StartBrewingCycle();

            brewingCycleListener.Received(1).BrewingCycleStarted();
        }

        private void StartBrewingCycle()
        {
            coffeeMakerApi.GetBoilerStatus().Returns(BoilerStatus.BOILER_NOT_EMPTY);
            coffeeMakerApi.GetWarmerPlateStatus().Returns(WarmerPlateStatus.POT_EMPTY);

            sut.Start(startBrewingRequest);
        }

        private static void ResetStateToDefault(State defaultState)
        {
            WhenCalledExtensions.CurrentState = defaultState;
        }
    }
}
