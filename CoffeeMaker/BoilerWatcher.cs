using System;
using System.Threading.Tasks;
using CoffeeMaker.Hardware;
using CoffeeMaker.Hardware.Status;

namespace CoffeeMaker
{
    public class BoilerWatcher : IDisposable
    {
        private readonly ICoffeeMakerAPI coffeeMakerApi;
        private readonly IBoilerListener boilerListener;

        private bool watch;

        public BoilerWatcher(ICoffeeMakerAPI coffeeMakerApi, IBoilerListener boilerListener)
        {
            this.coffeeMakerApi = coffeeMakerApi;
            this.boilerListener = boilerListener;
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
                boilerListener.BoilerEmpty();
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
    }
}
