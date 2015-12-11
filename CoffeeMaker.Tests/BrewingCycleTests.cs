using System;
using Automatonymous;
using CoffeeMaker.Events;
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
        private IObserver<BrewingCycleStarted> brewingCycleStartedObserver;
        private IObserver<BrewingCycleCompleted> brewingCycleCompletedObserver;  
        private BrewingCycle sut;

        [SetUp]
        public void PerTestSetUp()
        {
            ResetStateToDefault(NotChangedByTest);
            coffeeMakerApi = Substitute.For<ICoffeeMakerAPI>();
            startBrewingRequest = Substitute.For<IStartBrewingRequest>();
            brewingCycleStartedObserver = Substitute.For<IObserver<BrewingCycleStarted>>();
            brewingCycleCompletedObserver = Substitute.For<IObserver<BrewingCycleCompleted>>();
            sut = new BrewingCycle(coffeeMakerApi);
            sut.Subscribe(brewingCycleStartedObserver);
            sut.Subscribe(brewingCycleCompletedObserver);
        }

        [Test]
        public void MakesCoffee()
        {
            coffeeMakerApi.When(api => api.SetBoilerState(BoilerState.BOILER_ON))
                .Then(Boiling)
                .Do(ci =>
                {
                    sut.OnNext(new BoilerEmpty());
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

            sut.OnNext(new BoilerEmpty());

            brewingCycleCompletedObserver.Received(1).OnNext(Arg.Any<BrewingCycleCompleted>());
        }

        [Test]
        public void DoesNotNotifyAboutBrewingCycleCompletedWhenCycleNotStarted()
        {
            sut.OnNext(new BoilerEmpty());

            brewingCycleCompletedObserver.DidNotReceive().OnNext(Arg.Any<BrewingCycleCompleted>());
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

            brewingCycleStartedObserver.Received(1).OnNext(Arg.Any<BrewingCycleStarted>());
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
