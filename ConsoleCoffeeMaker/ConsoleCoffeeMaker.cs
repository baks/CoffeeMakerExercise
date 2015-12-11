using System;
using System.Collections.Generic;
using System.Linq;
using CoffeeMaker;
using CoffeeMaker.Hardware;
using CoffeeMaker.Hardware.Status;

namespace ConsoleCoffeeMaker
{
    public class ConsoleCoffeeMaker
    {
        private readonly CoffeeMakerInMemoryAPI coffeeMakerApi;
        private readonly Dictionary<ConsoleCommand, Action> commands;

        private bool isRunning = false;

        public ConsoleCoffeeMaker(CoffeeMakerInMemoryAPI coffeeMakerApi)
        {
            if (coffeeMakerApi == null)
            {
                throw new ArgumentNullException(nameof(coffeeMakerApi));
            }
            this.coffeeMakerApi = coffeeMakerApi;

            this.commands = new Dictionary<ConsoleCommand, Action>
            {
                {new ConsoleCommand("HELP", "displays help"), DisplayHelp },
                {new ConsoleCommand("DISPLAY", "displays coffee maker hardware state"), DisplayCoffeeMakerState },
                {new ConsoleCommand("EXIT", "exit Mark IV Coffee maker"), Exit},
                {new ConsoleCommand("PUSH", "pushes Brew Button"), PushButton},
                {new ConsoleCommand("FILL", "fills boiler with water"), FillWater},
                {new ConsoleCommand("EMPTY BOILER", "clear boiler"), EmptyBoiler },
                {new ConsoleCommand("TAKE POT", "takes pot from warmer plate"), TakePot},
                {new ConsoleCommand("RETURN EMPTY", "puts empty pot at warmer plate"), ReturnEmptyPot},
                {new ConsoleCommand("RETURN NOT EMPTY", "puts not empty pot at warmer plate"), ReturnNotEmptyPot}
            };
        }

        public void Start()
        {
            this.isRunning = true;

            var brewingCycle = new BrewingCycle(coffeeMakerApi);
            var coffeeMaker = new CoffeeMaker.CoffeeMaker(coffeeMakerApi, brewingCycle, new WarmingCycle(coffeeMakerApi));
            brewingCycle.AddListener(coffeeMaker);
            using (var brewButtonWatcher = new BrewButtonWatcher(coffeeMakerApi, coffeeMaker))
            {
                using (var potWatcher = new PotWatcher(coffeeMakerApi, coffeeMaker, coffeeMaker))
                {
                    using (var boilerWatcher = new BoilerWatcher(coffeeMakerApi, brewingCycle))
                    {
                        boilerWatcher.Start();
                        brewButtonWatcher.Start();
                        potWatcher.Start();

                        InteractionLoop();
                    }
                }
            }
        }

        private void InteractionLoop()
        {
            while (isRunning)
            {
                var commandText = Console.ReadLine();
                var consoleCommand = commands.FirstOrDefault(c => c.Key.IsCommandFor(commandText));
                if (
                    !EqualityComparer<KeyValuePair<ConsoleCommand, Action>>.Default.Equals(consoleCommand,
                        default(KeyValuePair<ConsoleCommand, Action>)))
                {
                    var action = consoleCommand.Value;
                    action();
                }
                else
                {
                    DisplayHelp();
                }
            }
        }


        private void DisplayHelp()
        {
            Console.WriteLine(string.Join(Environment.NewLine,
                commands.Keys.Select(c => c.ToString())));
        }

        private void Exit()
        {
            isRunning = false;
        }

        private void PushButton()
        {
            coffeeMakerApi.BrewButtonStatus = BrewButtonStatus.BREW_BUTTON_PUSHED;
        }

        private void FillWater()
        {
            coffeeMakerApi.BoilerStatus = BoilerStatus.BOILER_NOT_EMPTY;
        }

        private void EmptyBoiler()
        {
            coffeeMakerApi.BoilerStatus = BoilerStatus.BOILER_EMPTY;
        }

        private void TakePot()
        {
            coffeeMakerApi.WarmerPlateStatus = WarmerPlateStatus.WARMER_EMPTY;
        }

        private void ReturnNotEmptyPot()
        {
            coffeeMakerApi.WarmerPlateStatus = WarmerPlateStatus.POT_NOT_EMPTY;
        }

        private void ReturnEmptyPot()
        {
            coffeeMakerApi.WarmerPlateStatus = WarmerPlateStatus.POT_EMPTY;
        }

        private void DisplayCoffeeMakerState()
        {
            Console.WriteLine($"Boiler status : {coffeeMakerApi.BoilerStatus}");
            Console.WriteLine($"Brew button status : {coffeeMakerApi.BrewButtonStatus}");
            Console.WriteLine($"Warmer plate status : {coffeeMakerApi.WarmerPlateStatus}");
            Console.WriteLine($"Boiler state : {coffeeMakerApi.BoilerState}");
            Console.WriteLine($"Indicator state : {coffeeMakerApi.IndicatorState}");
            Console.WriteLine($"Relief valve state : {coffeeMakerApi.ReliefValveState}");
            Console.WriteLine($"Warmer state : {coffeeMakerApi.WarmerState}");
        }
    }
}
