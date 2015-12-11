using System;
using CoffeeMaker.Events;
using CoffeeMaker.Hardware;
using CoffeeMaker.Hardware.Status;

namespace CoffeeMaker
{
    public class CoffeeMaker : IObserver<BrewButtonPushed>, IObserver<BrewingCycleStarted>,
        IObserver<BrewingCycleCompleted>, IObserver<PotRemoved>, IObserver<PotReturned>, IObserver<CoffeeInPot>,
        IObserver<PotEmpty>
    {
        private readonly IBrewingCycle brewingCycle;
        private readonly IWarmingCycle warmingCycle;
        private readonly ICoffeeMakerAPI coffeeMakerApi;

        private bool isBrewing;
        private bool isWarming;

        public CoffeeMaker(ICoffeeMakerAPI coffeeMakerApi, IBrewingCycle brewingCycle, IWarmingCycle warmingCycle)
        {
            this.coffeeMakerApi = coffeeMakerApi;
            this.brewingCycle = brewingCycle;
            this.warmingCycle = warmingCycle;
            this.isBrewing = false;
            this.isWarming = false;
        }

        public void OnNext(CoffeeInPot value)
        {
            isWarming = true;
            warmingCycle.Start();
        }

        public void OnNext(PotEmpty value)
        {
            if (!isBrewing)
            {
                coffeeMakerApi.SetIndicatorState(IndicatorState.INDICATOR_OFF);
                warmingCycle.Stop();
                isWarming = false;
            }
        }

        public void OnNext(PotRemoved value)
        {
            if (isBrewing)
            {
                brewingCycle.Pause();
            }
            if (isWarming)
            {
                warmingCycle.Pause();
            }
        }

        public void OnNext(PotReturned value)
        {
            if (isBrewing)
            {
                brewingCycle.Resume();
            }
            if (isWarming)
            {
                warmingCycle.Resume();
            }
        }

        public void OnNext(BrewingCycleStarted value)
        {
            isBrewing = true;
            coffeeMakerApi.SetIndicatorState(IndicatorState.INDICATOR_OFF);
        }

        public void OnNext(BrewingCycleCompleted value)
        {
            coffeeMakerApi.SetIndicatorState(IndicatorState.INDICATOR_ON);
            isBrewing = false;
        }

        public void OnNext(BrewButtonPushed value)
        {
            if (!isBrewing)
            {
                brewingCycle.Start(new StartBrewingRequest());
            }
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
        }

        void IObserver<BrewingCycleCompleted>.OnError(Exception error)
        {
        }

        void IObserver<BrewingCycleCompleted>.OnCompleted()
        {
        }

        void IObserver<BrewingCycleStarted>.OnError(Exception error)
        {
        }

        void IObserver<BrewingCycleStarted>.OnCompleted()
        {
        }

        void IObserver<PotReturned>.OnError(Exception error)
        {
        }

        void IObserver<PotReturned>.OnCompleted()
        {
        }

        void IObserver<PotRemoved>.OnError(Exception error)
        {
        }

        void IObserver<PotRemoved>.OnCompleted()
        {
        }

        void IObserver<PotEmpty>.OnError(Exception error)
        {
        }

        void IObserver<PotEmpty>.OnCompleted()
        {
        }

        void IObserver<CoffeeInPot>.OnError(Exception error)
        {
        }

        void IObserver<CoffeeInPot>.OnCompleted()
        {
        }
    }
}
