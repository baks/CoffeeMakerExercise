using System;
using System.Threading.Tasks;
using Automatonymous;
using CoffeeMaker.Hardware;
using CoffeeMaker.Hardware.Status;

namespace CoffeeMaker
{
    public sealed class PotWatcherState
    {
        public State CurrentState { get; set; }
    }

    public sealed class PotWatcher : AutomatonymousStateMachine<PotWatcherState>, IDisposable
    {
        private readonly ICoffeeMakerAPI coffeeMakerApi;
        private readonly IPotContentListener potContentListener;
        private readonly IPotPositionListener potPositionListener;
        private readonly PotWatcherState potWatcherState;

        private bool watch;

        private bool potEmpty;

        public PotWatcher(ICoffeeMakerAPI coffeeMakerApi, IPotContentListener potContentListener, IPotPositionListener potPositionListener)
        {
            this.coffeeMakerApi = coffeeMakerApi;
            this.potContentListener = potContentListener;
            this.potPositionListener = potPositionListener;
            this.potWatcherState = new PotWatcherState();
            this.potEmpty = true;

            InstanceState(x => x.CurrentState);

            State(() => PotInWarmer);
            State(() => WarmerEmpty);

            Event(() => PotRemoved);
            Event(() => PotReturned);

            Initially(
                When(PotRemoved).Then(NotifyPotRemoved).TransitionTo(WarmerEmpty),
                When(PotReturned).Then(NotifyPotReturned).TransitionTo(PotInWarmer));

            During(PotInWarmer, When(PotRemoved).Then(NotifyPotRemoved).TransitionTo(WarmerEmpty));
            During(WarmerEmpty, When(PotReturned).Then(NotifyPotReturned).TransitionTo(PotInWarmer));
        }

        public State PotInWarmer { get; private set; }
        public State WarmerEmpty { get; private set; }

        public Event PotRemoved { get; private set; }
        public Event PotReturned { get; private set; }

        public void Start()
        {
            this.watch = true;
            Task.Factory.StartNew(Watch);
        }

        public void CheckPotPosition()
        {
            switch (coffeeMakerApi.GetWarmerPlateStatus())
            {
                case WarmerPlateStatus.WARMER_EMPTY:
                {
                    this.RaiseEvent(potWatcherState, PotRemoved);
                    break;
                }
                case WarmerPlateStatus.POT_EMPTY:
                case WarmerPlateStatus.POT_NOT_EMPTY:
                {
                    this.RaiseEvent(potWatcherState, PotReturned);
                    break;
                }
                default:
                {
                    break;
                }
            }
        }

        public void CheckPotContent()
        {
            if (coffeeMakerApi.GetWarmerPlateStatus() == WarmerPlateStatus.POT_EMPTY)
            {
                if (!potEmpty)
                {
                    potContentListener.PotEmpty();
                }
                potEmpty = true;
            }
            if (coffeeMakerApi.GetWarmerPlateStatus() == WarmerPlateStatus.POT_NOT_EMPTY)
            {
                if (potEmpty)
                {
                    potContentListener.CoffeeInPot();
                }
                potEmpty = false;
            }
        }

        public void Dispose()
        {
            this.watch = false;
        }

        private void Watch()
        {
            while (this.watch)
            {
                CheckPotPosition();
                CheckPotContent();
                Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        private void NotifyPotRemoved(BehaviorContext<PotWatcherState> context)
        {
            potPositionListener.PotRemovedFromWarmerPlate();
        }

        private void NotifyPotReturned(BehaviorContext<PotWatcherState> context)
        {
            potPositionListener.PotReturnedToWarmerPlate();
        }
    }
}
