using System;
using System.Threading.Tasks;
using CoffeeMaker.Hardware;
using CoffeeMaker.Hardware.Status;

namespace CoffeeMaker
{
    public sealed class BrewButtonWatcher : IDisposable
    {
        private readonly ICoffeeMakerAPI coffeeMakerApi;
        private readonly IBrewButtonListener listener;

        private bool watch = false;

        public BrewButtonWatcher(ICoffeeMakerAPI coffeeMakerApi, IBrewButtonListener listener)
        {
            this.coffeeMakerApi = coffeeMakerApi;
            this.listener = listener;
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
                listener.BrewButtonPushed();
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
    }
}
