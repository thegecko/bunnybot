using System;
using System.Timers;

namespace org.theGecko.Utilities
{
    /// <summary>
    /// Based on ideas from Gustavo G. Duarte (http://duartes.org/gustavo)
    /// </summary>
    public static class DelegateUtil
    {
        public static void SetTimeout(this Action function, double timeout)
        {
            Timer timer = new Timer(timeout);

            timer.Elapsed += delegate
            {
                function();
                timer.Stop();
            };

            timer.Start();
        }

        public static T SetTimeout<T>(this Func<T> function, double timeout)
        {
            FunctionWrapper<T> wrapper = new FunctionWrapper<T>(function);
            Timer timer = new Timer(timeout);

            timer.Elapsed += delegate
            {
                wrapper.Start();
                timer.Stop();                 
            };
    
            timer.Start();
            return wrapper.Result;
        }

        private class FunctionWrapper<T>
        {
            private readonly Func<T> _function;
            private T _result;

            public T Result
            {
                get { return _result; }
            }

            public FunctionWrapper(Func<T> function)
            {
                _function = function;
            }

            public void Start()
            {
                _result = _function();
            }
        }
    }
}
