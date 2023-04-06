using System.Runtime.ExceptionServices;

namespace RoslynCat.Controllers
{
     [DebuggerNonUserCode]
    public static class Helpers{
        public static T Invoke<T>(this MethodBase method, object target, params object[] parameters)
        {
            try
            {
                return (T)method.Invoke(target, parameters);
            }
            catch (TargetInvocationException ex)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                return default(T);
            }
        }
        public static void RecordExecutionTime(Action action){
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            action.Invoke();
            stopwatch.Stop();
            Console.WriteLine($"Execution time: {stopwatch.ElapsedMilliseconds} ms");
        }
    }
}