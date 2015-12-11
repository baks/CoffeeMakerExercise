using CoffeeMaker.Hardware.Status;

namespace ConsoleCoffeeMaker
{
    class Program
    {
        static void Main(string[] args)
        {
            var api = new CoffeeMakerInMemoryAPI();

            var consoleCoffeeMaker = new ConsoleCoffeeMaker(api);

            consoleCoffeeMaker.Start();
        }
    }
}
