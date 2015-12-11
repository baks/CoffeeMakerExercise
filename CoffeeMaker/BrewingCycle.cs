using System;
using System.Collections.Generic;
using Automatonymous;
using CoffeeMaker.Events;
using CoffeeMaker.Hardware;
using CoffeeMaker.Hardware.Status;
using CoffeeMaker.Infrastructure;

namespace CoffeeMaker
{
    public sealed class BrewingCycleState
    {
        public State CurrentState { get; set; }
    }

    public sealed class BrewingCycle : AutomatonymousStateMachine<BrewingCycleState>, IObservable<BrewingCycleStarted>, IObservable<BrewingCycleCompleted>, IBrewingCycle, IObserver<BoilerEmpty>
    {
        private readonly ICoffeeMakerAPI coffeeMakerApi;
        private readonly IList<IObserver<BrewingCycleStarted>> brewingCycleStartedObservers;
        private readonly IList<IObserver<BrewingCycleCompleted>> brewingCycleCompletedObservers;
        private readonly BrewingCycleState state;

        public BrewingCycle(ICoffeeMakerAPI coffeeMakerApi)
        {
            this.coffeeMakerApi = coffeeMakerApi;
            this.brewingCycleStartedObservers = new List<IObserver<BrewingCycleStarted>>();
            this.brewingCycleCompletedObservers = new List<IObserver<BrewingCycleCompleted>>();
            this.state = new BrewingCycleState();

            InstanceState(x => x.CurrentState);

            State(() => BrewingCycleInProgress);

            Event(() => StartCycle);
            Event(() => BoilerIsEmpty);

            Initially(When(StartCycle)
                .TransitionTo(BrewingCycleInProgress));

            During(BrewingCycleInProgress, 
                When(BoilerIsEmpty)
                    .Then(EndBrewingCycle)
                    .TransitionTo(ReadyToStartCycle));

            During(BrewingCyclePaused, 
                When(BoilerIsEmpty)
                    .Then(EndBrewingCycle)
                    .TransitionTo(ReadyToStartCycle));

            During(BrewingCycleInProgress,
                When(PauseCycle)
                    .Then(PauseBrewingCycle)
                    .TransitionTo(BrewingCyclePaused));

            During(BrewingCyclePaused,
                When(ResumeCycle)
                    .Then(ResumeBrewingCycle)
                    .TransitionTo(BrewingCycleInProgress));
        }

        public State ReadyToStartCycle { get; private set; }
        public State BrewingCycleInProgress { get; private set; }
        public State BrewingCyclePaused { get; private set; }

        public Event StartCycle { get; private set; }
        public Event BoilerIsEmpty { get; private set; }
        public Event PauseCycle { get; private set; }
        public Event ResumeCycle { get; private set; }

        public void Start(IStartBrewingRequest startBrewingRequest)
        {
            if (CanStartBrewingCycle(startBrewingRequest))
            {
                Subscriber.NotifyObserversAbout(brewingCycleStartedObservers, new BrewingCycleStarted());
                this.RaiseEvent(state, StartCycle);
                coffeeMakerApi.SetReliefValveState(ReliefValveState.VALVE_CLOSED);
                coffeeMakerApi.SetBoilerState(BoilerState.BOILER_ON);
            }
        }

        public void Pause()
        {
            this.RaiseEvent(state, PauseCycle);
        }

        public void Resume()
        {
            this.RaiseEvent(state, ResumeCycle);
        }

        private void ResumeBrewingCycle(BehaviorContext<BrewingCycleState> context)
        {
            coffeeMakerApi.SetReliefValveState(ReliefValveState.VALVE_CLOSED);
            coffeeMakerApi.SetBoilerState(BoilerState.BOILER_ON);
        }

        private void PauseBrewingCycle(BehaviorContext<BrewingCycleState> context)
        {
            coffeeMakerApi.SetReliefValveState(ReliefValveState.VALVE_OPEN);
            coffeeMakerApi.SetBoilerState(BoilerState.BOILER_OFF);
        }

        private void EndBrewingCycle(BehaviorContext<BrewingCycleState> context)
        {
            coffeeMakerApi.SetReliefValveState(ReliefValveState.VALVE_CLOSED);
            coffeeMakerApi.SetBoilerState(BoilerState.BOILER_OFF);
            Subscriber.NotifyObserversAbout(brewingCycleCompletedObservers, new BrewingCycleCompleted());
        }

        private bool CanStartBrewingCycle(IStartBrewingRequest startBrewingRequest)
        {
            if (coffeeMakerApi.GetWarmerPlateStatus() == WarmerPlateStatus.WARMER_EMPTY)
            {
                startBrewingRequest.CannotStartBrewingBecausePotIsNotInWarmerPlate();
                return false;
            }
            if (coffeeMakerApi.GetBoilerStatus() != BoilerStatus.BOILER_NOT_EMPTY)
            {
                startBrewingRequest.CannotStartBrewingBecauseBoilerIsEmpty();
                return false;
            }

            return true;
        }

        public void OnNext(BoilerEmpty value)
        {
            this.RaiseEvent(state, BoilerIsEmpty);
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
        }

        public IDisposable Subscribe(IObserver<BrewingCycleStarted> observer)
        {
            Subscriber.Subscribe(brewingCycleStartedObservers, observer);
            return Unsubscriber.CreateUnsubscriber(brewingCycleStartedObservers, observer);
        }

        public IDisposable Subscribe(IObserver<BrewingCycleCompleted> observer)
        {
            Subscriber.Subscribe(brewingCycleCompletedObservers, observer);
            return Unsubscriber.CreateUnsubscriber(brewingCycleCompletedObservers, observer);
        }
    }
}