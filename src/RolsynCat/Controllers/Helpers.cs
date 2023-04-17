using System.Collections.Immutable;
using System.Runtime.ExceptionServices;

namespace RoslynCat.Controllers
{
    /// <summary>
    /// 程序的帮助类，开发中……
    /// </summary>
    [DebuggerNonUserCode]
    public static class Helpers
    {
        /// <summary>
        /// 反射调用指定方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="target"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static T Invoke<T>(this MethodBase method,object target,params object[] parameters) {
            try {
                return (T)method.Invoke(target,parameters);
            }
            catch (TargetInvocationException ex) {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                return default(T);
            }
        }

        /// <summary>
        /// 记录方法的执行时间
        /// </summary>
        /// <param name="action">Fun<Task>类型的委托：async () =>{}</Task></param>
        public static async void RecordExecutionTimeAsync(Func<Task> asyncAction) {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            await asyncAction.Invoke();
            stopwatch.Stop();
            Console.WriteLine($"Execution time: {stopwatch.ElapsedMilliseconds} ms");
        }

        /// <summary>
        /// 转换不可变数组
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="array"></param>
        /// <param name="mapper"></param>
        /// <returns></returns>
        public static ImmutableArray<TOut> SelectAsArray<TIn, TOut>(this ImmutableArray<TIn> array,Func<TIn,TOut> mapper) {
            if (array.IsDefaultOrEmpty) {
                return ImmutableArray<TOut>.Empty;
            }
            var builder = ImmutableArray.CreateBuilder<TOut>(array.Length);
            foreach (var e in array) {
                builder.Add(mapper(e));
            }
            return builder.MoveToImmutable();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static ImmutableArray<T> ToImmutableAndClear<T>(this ImmutableArray<T>.Builder builder) {
            if (builder.Capacity == builder.Count) {
                return builder.MoveToImmutable();
            }
            else {
                var result = builder.ToImmutable();
                builder.Clear();
                return result;
            }
        }
    }
}