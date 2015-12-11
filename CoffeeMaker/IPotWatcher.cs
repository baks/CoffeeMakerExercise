namespace CoffeeMaker
{
    public interface IPotWatcher
    {
        void Notify(IPotPositionListener positionListener);
        void Unsubscribe(IPotPositionListener positionListener);
    }
}