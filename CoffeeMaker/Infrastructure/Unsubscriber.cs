using System;
using System.Collections.Generic;

namespace CoffeeMaker.Infrastructure
{
    internal static class Unsubscriber
    {
        public static IDisposable CreateUnsubscriber<T>(IList<IObserver<T>> observers, IObserver<T> observer)
        {
            return new Unsubscriber<T>(observers, observer);
        }
    }

    internal class Unsubscriber<T> : IDisposable
    {
        private readonly IList<IObserver<T>> _observers;
        private readonly IObserver<T> _observer;

        public Unsubscriber(IList<IObserver<T>> observers, IObserver<T> observer)
        {
            _observers = observers;
            _observer = observer;
        }

        public void Dispose()
        {
            if (_observer != null && _observers.Contains(_observer))
                _observers.Remove(_observer);

        }
    }
}
