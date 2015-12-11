namespace CoffeeMaker
{
    public interface IBrewingCycle
    {
        void Start(IStartBrewingRequest startBrewingRequest);
        void Pause();
        void Resume();
    }
}
