using NanoidDotNet;
using System.Collections.Concurrent;
using System.Reflection;
using static BowlFrame.Tools.Logger;

namespace BowlFrame.Adapter.CocoaPlugin.Event
{
    internal class CocoaEvent
    {
        //绑定列表
        private readonly ConcurrentDictionary<string, (object, MethodInfo, bool)> _bindList = [];

        //委托
        private readonly Type _delegate;

        private readonly ParameterInfo[] _paramType;

        private readonly Type _returnType;

        public CocoaEvent(Type @delegate)
        {
            _delegate = @delegate.BaseType == typeof(MulticastDelegate) ? @delegate : throw new NotSupportedException();
            MethodInfo delegateMethod = _delegate.GetMethod("Invoke") ?? throw new NullReferenceException();

            _paramType = delegateMethod.GetParameters();

            _returnType = delegateMethod.ReturnType;
        }

        ~CocoaEvent()
        {
            _bindList.Clear();
        }

        /// <summary>
        /// 注册事件
        /// </summary>
        /// <param name="object">对象</param>
        /// <param name="methodInfo">方法信息</param>
        /// <returns>注册标识符，Null 为添加失败</returns>
        public string? RegisterEvent(object @object, MethodInfo methodInfo)
        {
            try
            {
                ParameterInfo[] targetParameters = methodInfo.GetParameters();

                if (targetParameters.Length == _paramType.Length)
                {
                    for (int i = 0; i < targetParameters.Length; i++)
                        if (targetParameters[i].ParameterType != _paramType[i].ParameterType)
                            return null;

                    bool? isAsync = null;

                    if (methodInfo.ReturnType == _returnType)
                        isAsync = false;
                    else if (_returnType == typeof(void) && methodInfo.ReturnType == typeof(Task))
                        isAsync = true;
                    else if (methodInfo.ReturnType is { IsGenericType: true } && methodInfo.ReturnType.GetGenericTypeDefinition() == typeof(Task<>) && methodInfo.ReturnType.GenericTypeArguments[0] == _returnType)
                        isAsync = true;

                    if (isAsync is not null)
                    {
                        string flag = Nanoid.Generate(size: 8);
                        if (_bindList.TryAdd(flag, (@object, methodInfo, (bool)isAsync)))
                            return flag;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Warn(e);
            }

            return null;
        }

        /// <summary>
        /// 取消注册事件
        /// </summary>
        /// <param name="flag">标识符</param>
        /// <returns>结果</returns>
        public bool UnregisterEvent(string flag)
        {
            return _bindList.TryRemove(flag, out _);
        }

        /// <summary>
        /// 清空注册列表
        /// </summary>
        /// <returns></returns>
        public void ClearEvent()
        {
            _bindList.Clear();
        }

        /// <summary>
        /// 触发事件并获取返回值
        /// </summary>
        /// <param name="args">参数</param>
        /// <param name="returns">返回内容</param>
        /// <returns>执行结果</returns>
        public bool Invoke(object?[]? args, out object?[]? returns)
        {
            returns = null;

            //同步执行列表
            (object, MethodInfo, bool)[] list = _bindList.Select(a => a.Value).Where(b => !b.Item3).ToArray();
            //异步执行列表
            (object, MethodInfo, bool)[] asyncList = _bindList.Select(a => a.Value).Where(b => b.Item3).ToArray();
            //返回内容列表
            List<object?> returnList = [];
            //返回内容列表
            List<Task> awaitList = [];

            try
            {
                foreach ((object, MethodInfo, bool) item in asyncList)
                {
                    object? returnContent = item.Item2.Invoke(item.Item1, args);

                    if (returnContent is null || returnContent is not Task)
                        continue;

                    Task task = (Task)returnContent;

                    awaitList.Add(task);
                }

                foreach ((object, MethodInfo, bool) item in list)
                {
                    object? returnContent = item.Item2.Invoke(item.Item1, args);

                    returnList.Add(returnContent);
                }

                Task.WaitAll([.. awaitList]);

                foreach (dynamic item in awaitList)
                {
                    returnList.Add(item.Result);
                }
            }
            catch (Exception e)
            {
                Log.Warn(e);
                return false;
            }

            returns = [.. returnList];
            return true;
        }
    }
}