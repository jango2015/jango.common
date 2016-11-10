namespace Jango.Common.Utilities
{
    using System;
    public class Exceptions
    {
        public static void Eat(Action action)
        {
            try
            {
                action();
            }
            catch (Exception)
            {
            }
        }

        public static T Eat<T>(Func<T> action, T defaultVaule = default(T))
        {
            try
            {
                return action();
            }
            catch (Exception)
            {
                return defaultVaule;
            }
        }
    }
}
