namespace CoffeeMaker
{
    public interface IWarmingCycle
    {
        void Start();
        void Stop();
        void Pause();
        void Resume();
    }
}