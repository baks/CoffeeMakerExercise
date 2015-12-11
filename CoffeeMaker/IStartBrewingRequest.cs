namespace CoffeeMaker
{
    public interface IStartBrewingRequest
    {
        void CannotStartBrewingBecauseBoilerIsEmpty();
        void CannotStartBrewingBecausePotIsNotInWarmerPlate();
    }
}