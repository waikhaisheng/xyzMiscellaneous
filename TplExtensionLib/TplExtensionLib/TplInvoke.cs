using System;
using System.Threading.Tasks;

namespace TplExtensionLib
{
    public class TplInvoke
    {
        #region General Methods
        private T RunFunc<T1, T2, T>(T1 input1, T2 input2, DelegateTypeEnum delegateTypeEnum, object delegateType)
        {
            if (delegateTypeEnum == DelegateTypeEnum.ACTION)
            {
                if (delegateType is Action)
                {
                    Action iaction;
                    iaction = delegateType as Action;
                    iaction();
                    return default(T);
                }
                if (delegateType is Action<T1>)
                {
                    Action<T1> iaction;
                    iaction = delegateType as Action<T1>;
                    iaction(input1);
                    return default(T);
                }
                if (delegateType is Action<T1, T>)
                {
                    Action<T1, T2> iaction;
                    iaction = delegateType as Action<T1, T2>;
                    iaction(input1, input2);
                    return default(T);
                }

                throw new ArgumentException("Cannot get Action type.");
            }

            if (delegateTypeEnum == DelegateTypeEnum.FUNC)
            {

                if (delegateType is Func<T>)
                {
                    Func<T> ifunc;
                    ifunc = delegateType as Func<T>;
                    T result = ifunc();
                    return result;
                }
                if (delegateType is Func<T1, T>)
                {
                    Func<T1, T> ifunc;
                    ifunc = delegateType as Func<T1, T>;
                    T result = ifunc(input1);
                    return result;
                }
                if (delegateType is Func<T1, T2, T>)
                {
                    Func<T1, T2, T> ifunc;
                    ifunc = delegateType as Func<T1, T2, T>;
                    T result = ifunc(input1, input2);
                    return result;
                }

                throw new ArgumentException("Cannot get Func type.");
            }

            throw new ArgumentException("Cannot get type.");
        }
        private T[] ComputeInvoke<T1, T2, T>(T1 input1, T2 input2, DelegateTypeEnum delegateTypeEnum, params object[] funcArray)
        {
            Task<T>[] task = new Task<T>[funcArray.Length];
            ParallelOptions opt = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount - 1 };
            Parallel.For(0, funcArray.Length, opt, i =>
            {
                task[i] = Task.Run(() =>
                {
                    T r = RunFunc<T1, T2, T>(input1, input2, delegateTypeEnum, funcArray[i]);
                    return r;
                });
            });

            Task.WhenAll(task);

            if (delegateTypeEnum == DelegateTypeEnum.FUNC)
            {
                //Compute results
                T[] l = new T[funcArray.Length];
                Parallel.For(0, funcArray.Length, opt, i =>
                {
                    l[i] = task[i].Result;
                });

                return l;
            }

            return default(T[]);
        }

        //Async
        private async Task<T[]> ComputeInvokeAsync<T1, T2, T>(T1 input1, T2 input2, DelegateTypeEnum delegateTypeEnum, params object[] funcArray)
        {
            Task<T>[] task = new Task<T>[funcArray.Length];
            ParallelOptions opt = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount - 1 };
            Parallel.For(0, funcArray.Length, opt, i =>
            {
                task[i] = Task.Run(() =>
                {
                    T r = RunFunc<T1, T2, T>(input1, input2, delegateTypeEnum, funcArray[i]);
                    return r;
                });
            });

            await Task.WhenAll(task);

            if (delegateTypeEnum == DelegateTypeEnum.FUNC)
            {
                //Compute results
                T[] l = new T[funcArray.Length];
                Parallel.For(0, funcArray.Length, opt, i =>
                {
                    l[i] = task[i].Result;
                });

                return l;
            }

            return default(T[]);
        }
        #endregion

        #region Func methods
        //Public methods
        public T[] Invoke<T>(params Func<T>[] funcArray)
        {
            if (funcArray == null || funcArray.Length < 1)
                throw new ArgumentException("Func cannot be null or empty.");

            return ComputeInvoke<T, T, T>(default(T), default(T), DelegateTypeEnum.FUNC, funcArray);
        }
        public async Task<T[]> InvokeAsync<T>(params Func<T>[] funcArray)
        {
            if (funcArray == null || funcArray.Length < 1)
                throw new ArgumentException("Func cannot be null or empty.");

            return await ComputeInvokeAsync<T, T, T>(default(T), default(T), DelegateTypeEnum.FUNC, funcArray);
        }
        public T[] Invoke<T1, T>(T1 input1, params Func<T1, T>[] funcArray)
        {
            if (input1 == null)
                throw new ArgumentException("Input cannot be null.");
            if (funcArray == null || funcArray.Length < 1)
                throw new ArgumentException("Func cannot be null or empty.");

            return ComputeInvoke<T1, T, T>(input1, default(T), DelegateTypeEnum.FUNC, funcArray);
        }
        public async Task<T[]> InvokeAsync<T1, T>(T1 input1, params Func<T1, T>[] funcArray)
        {
            if (input1 == null)
                throw new ArgumentException("Input cannot be null.");
            if (funcArray == null || funcArray.Length < 1)
                throw new ArgumentException("Func cannot be null or empty.");

            return await ComputeInvokeAsync<T1, T, T>(input1, default(T), DelegateTypeEnum.FUNC, funcArray);
        }
        public T[] Invoke<T1, T2, T>(T1 input1, T2 input2, params Func<T1, T2, T>[] funcArray)
        {
            if (input1 == null || input2 == null)
                throw new ArgumentException("Input cannot be null.");
            if (funcArray == null || funcArray.Length < 1)
                throw new ArgumentException("Func cannot be null or empty.");

            return ComputeInvoke<T1, T2, T>(input1, input2, DelegateTypeEnum.FUNC, funcArray);
        }
        public async Task<T[]> InvokeAsync<T1, T2, T>(T1 input1, T2 input2, params Func<T1, T2, T>[] funcArray)
        {
            if (input1 == null || input2 == null)
                throw new ArgumentException("Input cannot be null.");
            if (funcArray == null || funcArray.Length < 1)
                throw new ArgumentException("Func cannot be null or empty.");

            return await ComputeInvokeAsync<T1, T2, T>(input1, input2, DelegateTypeEnum.FUNC, funcArray);
        }
        #endregion

        #region Action methods
        //Public methods
        public void Invoke<T>(params Action[] actionArray)
        {
            Parallel.Invoke(actionArray);
        }
        public async Task InvokeAsync<T>(params Action[] actionArray)
        {
            if (actionArray == null || actionArray.Length < 1)
                throw new ArgumentException("Action cannot be null or empty.");

            await ComputeInvokeAsync<T, T, T>(default(T), default(T), DelegateTypeEnum.ACTION, actionArray);
        }
        public void Invoke<T>(T input1, params Action<T>[] actionArray)
        {
            if (input1 == null)
                throw new ArgumentException("Input cannot be null.");
            if (actionArray == null || actionArray.Length < 1)
                throw new ArgumentException("Action cannot be null or empty.");

            ComputeInvoke<T, T, T>(input1, default(T), DelegateTypeEnum.ACTION, actionArray);
        }
        public async Task InvokeAsync<T>(T input1, params Action<T>[] actionArray)
        {
            if (input1 == null)
                throw new ArgumentException("Input cannot be null.");
            if (actionArray == null || actionArray.Length < 1)
                throw new ArgumentException("Action cannot be null or empty.");

            await ComputeInvokeAsync<T, T, T>(input1, default(T), DelegateTypeEnum.ACTION, actionArray);
        }
        public void Invoke<T1, T>(T1 input1, T input2, params Action<T1, T>[] actionArray)
        {
            if (input1 == null || input2 == null)
                throw new ArgumentException("Input cannot be null.");
            if (actionArray == null || actionArray.Length < 1)
                throw new ArgumentException("Action cannot be null or empty.");

            ComputeInvoke<T1, T, T>(input1, input2, DelegateTypeEnum.ACTION, actionArray);
        }
        public async Task InvokeAsync<T1, T>(T1 input1, T input2, params Action<T1, T>[] actionArray)
        {
            if (input1 == null || input2 == null)
                throw new ArgumentException("Input cannot be null.");
            if (actionArray == null || actionArray.Length < 1)
                throw new ArgumentException("Action cannot be null or empty.");

            await ComputeInvokeAsync<T1, T, T>(input1, input2, DelegateTypeEnum.ACTION, actionArray);
        }
        #endregion
    }
}
