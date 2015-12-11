using CoffeeMaker.Hardware;
using CoffeeMaker.Hardware.Status;

namespace ConsoleCoffeeMaker
{
    public class CoffeeMakerInMemoryAPI : ICoffeeMakerAPI
    {
        public CoffeeMakerInMemoryAPI()
        {
            WarmerPlateStatus = WarmerPlateStatus.POT_EMPTY;
            BoilerState = BoilerState.BOILER_OFF;
            BoilerStatus = BoilerStatus.BOILER_EMPTY;
            ReliefValveState = ReliefValveState.VALVE_CLOSED;
            WarmerState = WarmerState.WARMER_OFF;
            BrewButtonStatus = BrewButtonStatus.BREW_BUTTON_NOT_PUSHED;
            IndicatorState = IndicatorState.INDICATOR_OFF;
        }

        public WarmerPlateStatus WarmerPlateStatus { get; set; }
        public BoilerStatus BoilerStatus { get; set; }
        public BrewButtonStatus BrewButtonStatus { get; set; }
        public BoilerState BoilerState { get; private set; }
        public WarmerState WarmerState { get; private set; }
        public IndicatorState IndicatorState { get; private set; }
        public ReliefValveState ReliefValveState { get; private set; }

        public WarmerPlateStatus GetWarmerPlateStatus()
        {
            return WarmerPlateStatus;
        }

        public BoilerStatus GetBoilerStatus()
        {
            return BoilerStatus;
        }

        public BrewButtonStatus GetBrewButtonStatus()
        {
            return BrewButtonStatus;
        }

        public void SetBoilerState(BoilerState boilerStatus)
        {
            BoilerState = boilerStatus;
        }

        public void SetWarmerState(WarmerState warmerState)
        {
            WarmerState = warmerState;
        }

        public void SetIndicatorState(IndicatorState indicatorState)
        {
            IndicatorState = indicatorState;
        }

        public void SetReliefValveState(ReliefValveState reliefValveState)
        {
            ReliefValveState = reliefValveState;
        }
    }
}
