using System;
using Automatonymous;
using NSubstitute;
using NSubstitute.Core;

namespace CoffeeMaker.Tests
{
    public static class WhenCalledExtensions
    {
        public static State CurrentState;

        public static WhenCalled<T> Then<T>(this WhenCalled<T> whenCalled, State newState) where T:class
        {
            whenCalled.Do(info => CurrentState = newState);
            return whenCalled;
        }

        public static ConfiguredCall Then(this ConfiguredCall configuredCall, State newState)
        {
            configuredCall.AndDoes(info => CurrentState = newState);
            return configuredCall;
        }

        public static WhenCalled<T> Expect<T>(this WhenCalled<T> whenCalled, State expectedState)
        {
            whenCalled.Do(info =>
            {
                if (!CurrentState.Equals(expectedState))
                {
                    throw new Exception($" Expected {expectedState}, but was {CurrentState}");
                }
            });
            return whenCalled;
        }
    }
}