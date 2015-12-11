using System;
using System.Collections.Generic;

namespace CoffeeMaker.Infrastructure
{
    internal static class Subscriber
    {
        public static void Subscribe<T>(IList<IObserver<T>> observers, IObserver<T> observer)
        {
            if (!observers.Contains(observer))
            {
                observers.Add(observer);
            }
        }

        public static void NotifyObserversAbout<T>(IList<IObserver<T>> observers, T about)
        {
            foreach (var observer in observers)
            {
                observer.OnNext(about);
            }
        }
    }
}