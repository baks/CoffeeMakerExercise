using CoffeeMaker.Hardware;
using CoffeeMaker.Hardware.Status;

namespace CoffeeMaker
{
    public class CoffeeMaker : IBrewButtonListener, IBrewingCycleListener, IPotContentListener, IPotPositionListener
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

        public void BrewButtonPushed()
        {
            if (!isBrewing)
            {
                brewingCycle.Start(new StartBrewingRequest());
            }
        }

        public void BrewingCycleStarted()
        {
            isBrewing = true;
            coffeeMakerApi.SetIndicatorState(IndicatorState.INDICATOR_OFF);
        }

        public void BrewingCycleCompleted()
        {
            coffeeMakerApi.SetIndicatorState(IndicatorState.INDICATOR_ON);
            isBrewing = false;
        }

        public void PotEmpty()
        {
            if (!isBrewing)
            {
                coffeeMakerApi.SetIndicatorState(IndicatorState.INDICATOR_OFF);
                warmingCycle.Stop();
                isWarming = false;
            }
        }

        public void CoffeeInPot()
        {
            isWarming = true;
            warmingCycle.Start();
        }

        public void PotRemovedFromWarmerPlate()
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

        public void PotReturnedToWarmerPlate()
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
    }
}
