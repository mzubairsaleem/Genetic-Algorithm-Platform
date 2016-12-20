using System.Threading;

namespace System {

    public static class Lazy
    {
        public static Lazy<T> New<T>(Func<T> valueFactory, LazyThreadSafetyMode mode = LazyThreadSafetyMode.ExecutionAndPublication)
        {
            return new Lazy<T>(valueFactory,mode);
        }
    }    
}