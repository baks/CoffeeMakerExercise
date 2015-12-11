using System;
using Automatonymous;
using CoffeeMaker.Hardware;
using CoffeeMaker.Hardware.Status;

namespace CoffeeMaker
{
    public sealed class BrewingCycleState
    {
        public State CurrentState { get; set; }
    }

    public class NullBrewingCycleListener : IBrewingCycleListener
    {
        public void BrewingCycleStarted()
        {
        }

        public void BrewingCycleCompleted()
        {
        }
    }

    public sealed class BrewingCycle : AutomatonymousStateMachine<BrewingCycleState>, IBrewingCycle, IBoilerListener
    {
        private readonly ICoffeeMakerAPI coffeeMakerApi;
        private readonly BrewingCycleState state;

        private IBrewingCycleListener brewingCycleListener;

        public BrewingCycle(ICoffeeMakerAPI coffeeMakerApi)
        {
            this.coffeeMakerApi = coffeeMakerApi;
            this.brewingCycleListener = new NullBrewingCycleListener();
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

        public void AddListener(IBrewingCycleListener brewingCycleListener)
        {
            if (brewingCycleListener == null)
            {
                throw new ArgumentNullException(nameof(brewingCycleListener));
            }
            if (this.brewingCycleListener.GetType() != typeof(NullBrewingCycleListener))
            {
                throw new InvalidOperationException("Cannot add new listener after assigning other");
            }
            this.brewingCycleListener = brewingCycleListener;
        }

        public void Start(IStartBrewingRequest startBrewingRequest)
        {
            if (CanStartBrewingCycle(startBrewingRequest))
            {
                brewingCycleListener.BrewingCycleStarted();
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

        public void BoilerEmpty()
        {
            this.RaiseEvent(state, BoilerIsEmpty);
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
            brewingCycleListener.BrewingCycleCompleted();
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
    }
}