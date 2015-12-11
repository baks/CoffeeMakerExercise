using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoffeeMaker.Events;
using CoffeeMaker.Hardware;
using CoffeeMaker.Hardware.Status;
using CoffeeMaker.Infrastructure;

namespace CoffeeMaker
{
    public sealed class BrewButtonWatcher : IObservable<BrewButtonPushed>, IDisposable
    {
        private readonly ICoffeeMakerAPI coffeeMakerApi;
        private readonly IList<IObserver<BrewButtonPushed>> brewButtonPushedObservers;

        private bool watch = false;

        public BrewButtonWatcher(ICoffeeMakerAPI coffeeMakerApi)
        {
            this.coffeeMakerApi = coffeeMakerApi;
            this.brewButtonPushedObservers = new List<IObserver<BrewButtonPushed>>();
        }

        public void Start()
        {
            this.watch = true;
            Task.Factory.StartNew(Watch);
        }

        public void CheckBrewButton()
        {
            if (coffeeMakerApi.GetBrewButtonStatus() == BrewButtonStatus.BREW_BUTTON_PUSHED)
            {
                Subscriber.NotifyObserversAbout(brewButtonPushedObservers, new BrewButtonPushed());
            }
        }

        public void Dispose()
        {
            watch = false;
        }

        private void Watch()
        {
            while (this.watch)
            {
                CheckBrewButton();
                Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        public IDisposable Subscribe(IObserver<BrewButtonPushed> observer)
        {
            Subscriber.Subscribe(brewButtonPushedObservers, observer);
            return Unsubscriber.CreateUnsubscriber(brewButtonPushedObservers, observer);
        }
    }
}
