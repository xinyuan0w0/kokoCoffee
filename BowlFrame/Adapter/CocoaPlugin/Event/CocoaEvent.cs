using NanoidDotNet;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static BowlFrame.Tools.Logger;

namespace BowlFrame.Adapter.CocoaPlugin.Event
{
    internal class CocoaEvent(Type @delegate)
    {
        ~CocoaEvent()
        {
            _bindList.Clear();
        }

        //绑定列表
        private readonly ConcurrentDictionary<string, (object, MethodInfo)> _bindList = [];

        //委托
        private readonly Type @delegate = @delegate.BaseType == typeof(MulticastDelegate) ? @delegate : throw new NotSupportedException();

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

                ParameterInfo[] parameters = (@delegate.GetMethod("Invoke") ?? throw new NullReferenceException()).GetParameters();

                if (targetParameters.Length == parameters.Length)
                {
                    for (int i = 0; i < targetParameters.Length; i++)
                        if (targetParameters[i].ParameterType != parameters[i].ParameterType)
                            return null;

                    if (methodInfo.ReturnType == (@delegate.GetMethod("Invoke") ?? throw new NullReferenceException()).ReturnType)
                    {
                        string flag = Nanoid.Generate(size: 8);
                        if (_bindList.TryAdd(flag, (@object, methodInfo)))
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

            (object, MethodInfo)[] list = [.. _bindList.Values];
            List<object?> returnList = [];

            try
            {
                foreach ((object, MethodInfo) item in list)
                {
                    returnList.Add(item.Item2.Invoke(item.Item1, args));
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