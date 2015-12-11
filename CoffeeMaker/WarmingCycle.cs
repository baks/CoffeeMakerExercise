using CoffeeMaker.Hardware;
using CoffeeMaker.Hardware.Status;

namespace CoffeeMaker
{
    public class WarmingCycle : IWarmingCycle
    {
        private readonly ICoffeeMakerAPI coffeeMakerApi;

        private bool warmingCycleInProgress;

        public WarmingCycle(ICoffeeMakerAPI coffeeMakerApi)
        {
            this.coffeeMakerApi = coffeeMakerApi;
            this.warmingCycleInProgress = false;
        }

        public void Start()
        {
            warmingCycleInProgress = true;
            coffeeMakerApi.SetWarmerState(WarmerState.WARMER_ON);
        }

        public void Stop()
        {
            coffeeMakerApi.SetWarmerState(WarmerState.WARMER_OFF);
            warmingCycleInProgress = false;
        }

        public void Pause()
        {
            if (warmingCycleInProgress)
            {
                coffeeMakerApi.SetWarmerState(WarmerState.WARMER_OFF);
            }
        }

        public void Resume()
        {
            if (warmingCycleInProgress)
            {
                if (coffeeMakerApi.GetWarmerPlateStatus() == WarmerPlateStatus.POT_NOT_EMPTY)
                {
                    coffeeMakerApi.SetWarmerState(WarmerState.WARMER_ON);
                }
            }
        }
    }
}
