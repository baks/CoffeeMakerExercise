using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoffeeMaker.Events;
using CoffeeMaker.Hardware;
using CoffeeMaker.Hardware.Status;
using CoffeeMaker.Infrastructure;

namespace CoffeeMaker
{
    public class BoilerWatcher : IObservable<BoilerEmpty>, IDisposable
    {
        private readonly ICoffeeMakerAPI coffeeMakerApi;
        private readonly IList<IObserver<BoilerEmpty>> boilerEmptyObservers;

        private bool watch;

        public BoilerWatcher(ICoffeeMakerAPI coffeeMakerApi)
        {
            this.coffeeMakerApi = coffeeMakerApi;
            this.boilerEmptyObservers = new List<IObserver<BoilerEmpty>>();
        }

        public void Start()
        {
            this.watch = true;
            Task.Factory.StartNew(Watch);
        }

        public void CheckBoilerContent()
        {
            if (coffeeMakerApi.GetBoilerStatus() == BoilerStatus.BOILER_EMPTY)
            {
                Subscriber.NotifyObserversAbout(boilerEmptyObservers, new BoilerEmpty());
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
                CheckBoilerContent();
                Task.Delay(TimeSpan.FromSeconds(1));
            }
        }

        public IDisposable Subscribe(IObserver<BoilerEmpty> observer)
        {
            Subscriber.Subscribe(boilerEmptyObservers, observer);
            return Unsubscriber.CreateUnsubscriber(boilerEmptyObservers, observer);
        }
    }
}
