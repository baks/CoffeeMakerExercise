using System;

namespace CoffeeMaker
{
    public interface IPotPositionListener
    {
        void PotRemovedFromWarmerPlate();

        void PotReturnedToWarmerPlate();
    }
}