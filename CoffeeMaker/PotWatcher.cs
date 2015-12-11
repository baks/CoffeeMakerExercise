using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Automatonymous;
using CoffeeMaker.Events;
using CoffeeMaker.Hardware;
using CoffeeMaker.Hardware.Status;
using CoffeeMaker.Infrastructure;

namespace CoffeeMaker
{
    public sealed class PotWatcherState
    {
        public State CurrentState { get; set; }
    }

    public sealed class PotWatcher : AutomatonymousStateMachine<PotWatcherState>, IObservable<CoffeeInPot>,
        IObservable<PotEmpty>, IObservable<PotRemoved>, IObservable<PotReturned>, IDisposable
    {
        private readonly ICoffeeMakerAPI coffeeMakerApi;
        private readonly PotWatcherState potWatcherState;

        private readonly IList<IObserver<CoffeeInPot>> coffeeInPotObservers;
        private readonly IList<IObserver<PotEmpty>> potEmptyObservers;
        private readonly IList<IObserver<PotRemoved>> potRemovedObservers;
        private readonly IList<IObserver<PotReturned>> potReturnedObservers;

        private bool watch;

        private bool potEmpty;

        public PotWatcher(ICoffeeMakerAPI coffeeMakerApi)
        {
            this.coffeeMakerApi = coffeeMakerApi;
            this.coffeeInPotObservers = new List<IObserver<CoffeeInPot>>();
            this.potEmptyObservers = new List<IObserver<PotEmpty>>();
            this.potRemovedObservers = new List<IObserver<PotRemoved>>();
            this.potReturnedObservers = new List<IObserver<PotReturned>>();
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
                    Subscriber.NotifyObserversAbout(potEmptyObservers, new PotEmpty());
                }
                potEmpty = true;
            }
            if (coffeeMakerApi.GetWarmerPlateStatus() == WarmerPlateStatus.POT_NOT_EMPTY)
            {
                if (potEmpty)
                {
                    Subscriber.NotifyObserversAbout(coffeeInPotObservers, new CoffeeInPot());
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
            Subscriber.NotifyObserversAbout(potRemovedObservers, new PotRemoved());
        }

        private void NotifyPotReturned(BehaviorContext<PotWatcherState> context)
        {
            Subscriber.NotifyObserversAbout(potReturnedObservers, new PotReturned());
        }

        public IDisposable Subscribe(IObserver<CoffeeInPot> observer)
        {
            Subscriber.Subscribe(coffeeInPotObservers, observer);
            return Unsubscriber.CreateUnsubscriber(coffeeInPotObservers, observer);
        }

        public IDisposable Subscribe(IObserver<PotEmpty> observer)
        {
            Subscriber.Subscribe(potEmptyObservers, observer);
            return Unsubscriber.CreateUnsubscriber(potEmptyObservers, observer);
        }

        public IDisposable Subscribe(IObserver<PotRemoved> observer)
        {
            Subscriber.Subscribe(potRemovedObservers, observer);
            return Unsubscriber.CreateUnsubscriber(potRemovedObservers, observer);
        }

        public IDisposable Subscribe(IObserver<PotReturned> observer)
        {
            Subscriber.Subscribe(potReturnedObservers, observer);
            return Unsubscriber.CreateUnsubscriber(potReturnedObservers, observer);
        }
    }
}
