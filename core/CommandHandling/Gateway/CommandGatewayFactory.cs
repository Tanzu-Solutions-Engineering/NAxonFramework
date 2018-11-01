using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Castle.Core.Interceptor;
using Castle.DynamicProxy;
using MoreLinq;
using NAxonFramework.CommandHandling.Callbacks;
using NAxonFramework.Common;
using NAxonFramework.Common.Attributes;
using NAxonFramework.Messaging;
using NAxonFramework.Messaging.Attributes;

namespace NAxonFramework.CommandHandling.Gateway
{
    public class CommandGatewayFactory
    {
        private ICommandBus _commandBus;
        private IRetryScheduler _retryScheduler;
        private List<IMessageDispatchInterceptor> _dispatchInterceptors;
        private List<ICommandCallback> _commandCallbacks;

        public CommandGatewayFactory(ICommandBus commandBus, params IMessageDispatchInterceptor[] dispatchInterceptors)
            : this(commandBus, null, dispatchInterceptors)
        {
        }

        public CommandGatewayFactory(ICommandBus commandBus, IRetryScheduler retryScheduler, params IMessageDispatchInterceptor[] dispatchInterceptors)
            : this(commandBus, retryScheduler, dispatchInterceptors.ToList())
        {
        }

        private CommandGatewayFactory(ICommandBus commandBus, IRetryScheduler retryScheduler, List<IMessageDispatchInterceptor> messageDispatchInterceptors)
        {
            Assert.NotNull(commandBus, () => "commandBus may not be null");
            _retryScheduler = retryScheduler;
            _commandBus = commandBus;
            if (messageDispatchInterceptors != null && !messageDispatchInterceptors.IsEmpty())
            {
                _dispatchInterceptors = new List<IMessageDispatchInterceptor>(messageDispatchInterceptors);
            }
            else
            {
                _dispatchInterceptors = new List<IMessageDispatchInterceptor>();
            }

            _commandCallbacks = new List<ICommandCallback>();
        }

        public T CreateGateway<T>()
        {
            var gatewayInterface = typeof(T);
            var dispatchers = new Dictionary<MethodInfo, IInvocationHandler>();
            foreach (var gatewayMethod in gatewayInterface.GetMethods())
            {
                var extractors = ExtractMetaData(gatewayMethod.GetParameters());

                var arguments = gatewayMethod.GetParameters().Select(x => x.ParameterType).ToArray();

                IInvocationHandler dispatcher =
                    new DispatchOnInvocationHandler(_commandBus, _retryScheduler, _dispatchInterceptors, extractors,
                        _commandCallbacks, true);

                if (typeof(Task).IsAssignableFrom(gatewayMethod.ReturnType))
                {

                    if (arguments.Length >= 3 && typeof(DateTime).IsAssignableFrom(arguments[arguments.Length - 1]) &&
                        (typeof(long).IsAssignableFrom(arguments[arguments.Length - 2]) ||
                         typeof(int).IsAssignableFrom(arguments[arguments.Length - 2])))
                    {
                        dispatcher = WrapToReturnWithTimeoutInArguments(dispatcher, arguments.Length - 2, arguments.Length - 1);
                    }
                    else
                    {
                        var timeout = AnnotationUtils.FindAnnotationAttributes(gatewayMethod, typeof(TimeoutAttribute))
                            .OrElse(AnnotationUtils.FindAnnotationAttributes(gatewayMethod.DeclaringType, typeof(TimeoutAttribute))
                                .OrElse(null));
                        if (timeout != null)
                        {
                            dispatcher = WrapToReturnWithFixedTimeout(dispatcher, (int) timeout.GetValueOrDefault("timeout"),
                                (TimeUnit) timeout.GetValueOrDefault("unit"));
                        }
                        else if (typeof(void) != gatewayMethod.ReturnType)
                        {
                            dispatcher = WrapToWaitForResult(dispatcher);
                        }
                        else if (_commandCallbacks.IsEmpty() && !HasCallbackParameters(gatewayMethod))
                        {
                            // switch to fire-and-forget mode
                            dispatcher = WrapToFireAndForget(
                                new DispatchOnInvocationHandler(_commandBus, _retryScheduler, _dispatchInterceptors,
                                    extractors, _commandCallbacks, false));
                        }
                    }

//                    var declaredExceptions = gatewayMethod.getExceptionTypes();
//                    if (!contains(declaredExceptions, TimeoutException.class)) {
//                        dispatcher = wrapToReturnNullOnTimeout(dispatcher);
//                    }
//                    if (!contains(declaredExceptions, InterruptedException.class)) {
//                        dispatcher = wrapToReturnNullOnInterrupted(dispatcher);
//                    }
//                    dispatcher = wrapUndeclaredExceptions(dispatcher, declaredExceptions);
                }

                dispatchers[gatewayMethod] = dispatcher;
            }

            var generator = new ProxyGenerator();
            var proxy = (T) generator.CreateInterfaceProxyWithoutTarget(gatewayInterface, new GatewayInvocationHandler(dispatchers, _commandBus, _retryScheduler,
                _dispatchInterceptors));

            return proxy;
        }

        private bool HasCallbackParameters(MethodInfo gatewayMethod)
        {
            foreach (var parameter in gatewayMethod.GetParameterTypes())
            {
                if (typeof(ICommandCallback).IsAssignableFrom(parameter))
                {
                    return true;
                }
            }

            return false;
        }

        protected IInvocationHandler WrapToReturnNullOnInterrupted(IInvocationHandler @delegate)
        {
            return new NullOnInterrupted(@delegate);
        }

        protected IInvocationHandler WrapToReturnNullOnTimeout(IInvocationHandler @delegate)
        {
            return new NullOnTimeout(@delegate);
        }

        protected IInvocationHandler WrapToFireAndForget(IInvocationHandler @delegate)
        {
            return new FireAndForget(@delegate);
        }

        protected IInvocationHandler WrapToWaitForResult(IInvocationHandler @delegate)
        {
            return new WaitForResult(@delegate);
        }

        protected IInvocationHandler WrapToReturnWithFixedTimeout(IInvocationHandler @delegate, long timeout, TimeUnit timeUnit)
        {
            return new WaitForResultWithFixedTimeout(@delegate, timeout, timeUnit);
        }

        protected IInvocationHandler WrapToReturnWithTimeoutInArguments(IInvocationHandler @delegate, int timeoutIndex, int timeUnitIndex)
        {
            return new WaitForResultWithTimeoutInArguments(@delegate, timeoutIndex, timeUnitIndex);
        }

        public CommandGatewayFactory RegisterCommandCallback(ICommandCallback callback)
        {
            _commandCallbacks.Add(new TypeSafeCallbackWrapper(callback));
            return this;
        }

        public CommandGatewayFactory RegisterDispatchInterceptor(
            IMessageDispatchInterceptor dispatchInterceptor)
        {
            _dispatchInterceptors.Add(dispatchInterceptor);
            return this;
        }

        private MetaDataExtractor[] ExtractMetaData(ParameterInfo[] parameters)
        {
            var extractors = new List<MetaDataExtractor>();
            for (int i = 0; i < parameters.Length; i++)
            {
                if (typeof(MetaData).IsAssignableFrom(parameters[i].ParameterType))
                {
                    extractors.Add(new MetaDataExtractor(i, null));
                }
                else
                {
                    var metaDataAnnotation =
                        AnnotationUtils.FindAnnotationAttributes(parameters[i], typeof(MetaDataValueAttribute));
                    if (metaDataAnnotation.IsPresent)
                    {
                        extractors.Add(new MetaDataExtractor(i, (string) metaDataAnnotation.Get().Get("metaDataValue")));
                    }
                }
            }

            return extractors.ToArray();
        }

        public interface IInvocationHandler<out R> : IInvocationHandler
        {
            R Invoke(object proxy, MethodInfo invokedMethod, object[] args);
        }

        public interface IInvocationHandler
        {
            object Invoke(object proxy, MethodInfo invokedMethod, object[] args);
        }

        public class GatewayInvocationHandler : AbstractCommandGateway, IInterceptor
        {
            private readonly IReadOnlyDictionary<MethodInfo, IInvocationHandler> _dispatchers;

            public GatewayInvocationHandler(IReadOnlyDictionary<MethodInfo, IInvocationHandler> dispatchers,
                ICommandBus commandBus,
                IRetryScheduler retryScheduler,
                List<IMessageDispatchInterceptor> dispatchInterceptors)
                : base(commandBus, retryScheduler, dispatchInterceptors)
            {
                _dispatchers = dispatchers;
            }

            public void Intercept(IInvocation invocation)
            {
                var invocationHandler = _dispatchers.GetValueOrDefault(invocation.Method);
                invocationHandler.Invoke(invocation.Proxy, invocation.Method, invocation.Arguments);
            }
        }

        private class DispatchOnInvocationHandler: AbstractCommandGateway, IInvocationHandler<Task<object>>
        {

            private MetaDataExtractor[] _metaDataExtractors;
            private List<ICommandCallback> _commandCallbacks;
            private bool _forceCallbacks;

            public DispatchOnInvocationHandler(ICommandBus commandBus, IRetryScheduler retryScheduler,
                List<IMessageDispatchInterceptor> messageDispatchInterceptors,
                MetaDataExtractor[] metaDataExtractors, // NOSONAR
                List<ICommandCallback> commandCallbacks,
                bool forceCallbacks)
                : base(commandBus, retryScheduler, messageDispatchInterceptors)
            {
                _metaDataExtractors = metaDataExtractors; // NOSONAR
                _commandCallbacks = commandCallbacks;
                _forceCallbacks = forceCallbacks;
            }

            public Task<object> Invoke(object proxy, MethodInfo invokedMethod, object[] args)
            {
                var command = args[0];
                if (_metaDataExtractors.Length != 0)
                {
                    var metaDataValues = new Dictionary<string, object>();
                    foreach (var extractor in _metaDataExtractors)
                    {
                        extractor.AddMetaData(args, metaDataValues);
                    }

                    if (!metaDataValues.IsEmpty())
                    {
                        command = GenericCommandMessage.AsCommandMessage(command).WithMetaData(metaDataValues);
                    }
                }

                if (_forceCallbacks || !_commandCallbacks.IsEmpty())
                {
                    var callbacks = new List<ICommandCallback>();
                    var future = new FutureCallback<object>();
                    callbacks.Add(future);
                    foreach (var arg in args)
                    {
                        if (arg is ICommandCallback)
                        {
                            var callback = (ICommandCallback) arg;
                            callbacks.Add(callback);
                        }
                    }

                    callbacks.AddRange(_commandCallbacks);
                    Send(command, new CompositeCallback(callbacks));
                    return future.Task;
                }
                else
                {
                    SendAndForget(command);
                    return null;
                }
            }


            object IInvocationHandler.Invoke(object proxy, MethodInfo invokedMethod, object[] args)
            {
                return Invoke(proxy, invokedMethod, args);
            }
        }

        private class CompositeCallback : ICommandCallback
        {
            private readonly List<ICommandCallback> _callbacks;

            public CompositeCallback(List<ICommandCallback> callbacks)
            {
                _callbacks = callbacks;
            }


            public void OnSuccess(ICommandMessage commandMessage, object result)
            {
                foreach (var callback in _callbacks)
                {
                    callback.OnSuccess(commandMessage, result);
                }
            }

            public void OnFailure(ICommandMessage commandMessage, Exception cause)
            {
                foreach (var callback in _callbacks)
                {
                    callback.OnFailure(commandMessage, cause);
                }
            }
        }

        private class NullOnTimeout : IInvocationHandler
        {
            private readonly IInvocationHandler _delegate;

            public NullOnTimeout(IInvocationHandler @delegate)
            {
                _delegate = @delegate;
            }

            public object Invoke(object proxy, MethodInfo invokedMethod, object[] args)
            {
                try
                {
                    return _delegate.Invoke(proxy, invokedMethod, args);
                }
                catch (TimeoutException timeout)
                {
                    return null;
                }
            }
        }

        private class NullOnInterrupted : IInvocationHandler
        {
            private readonly IInvocationHandler _delegate;

            public NullOnInterrupted(IInvocationHandler @delegate)
            {
                _delegate = @delegate;
            }

            public object Invoke(object proxy, MethodInfo invokedMethod, object[] args)
            {
                try
                {

                    return _delegate.Invoke(proxy, invokedMethod, args);
                }
                catch (ThreadInterruptedException timeout) //todo: Refactor for TPL 
                {
                    Thread.CurrentThread.Interrupt();
                    return null;
                }
            }
        }

        private class WaitForResultWithFixedTimeout : IInvocationHandler
        {

            private IInvocationHandler _delegate;
            private long _timeout;
            private TimeUnit _timeUnit;

            public WaitForResultWithFixedTimeout(IInvocationHandler @delegate, long timeout, TimeUnit timeUnit)
            {
                _delegate = @delegate;
                _timeout = timeout;
                _timeUnit = timeUnit;

            }

            public object Invoke(Object proxy, MethodInfo invokedMethod, Object[] args)
            {
                dynamic task = _delegate.Invoke(proxy, invokedMethod, args);
                task = task.TimeoutAfter(_timeUnit.ToTimeSpan(_timeout));
                return task.Result; //todo: refactor for tpl, potential deadlock
            }

        }

        private class WaitForResultWithTimeoutInArguments : IInvocationHandler
        {

            private IInvocationHandler _delegate;
            private int _timeoutIndex;
            private int _timeUnitIndex;

            public WaitForResultWithTimeoutInArguments(IInvocationHandler @delegate, int timeoutIndex, int timeUnitIndex)
            {
                _delegate = @delegate;
                _timeoutIndex = timeoutIndex;
                _timeUnitIndex = timeUnitIndex;
            }

            public object Invoke(object proxy, MethodInfo invokedMethod, object[] args)
            {
                dynamic task = _delegate.Invoke(proxy, invokedMethod, args);
                task = task.TimeoutAfter(((TimeUnit) args[_timeUnitIndex]).ToTimeSpan((long) args[_timeoutIndex]));
                return task.Result;

            }
        }

        private class WaitForResult : IInvocationHandler
        {

            private IInvocationHandler _delegate;

            public WaitForResult(IInvocationHandler @delegate)
            {
                _delegate = @delegate;
            }

            public object Invoke(object proxy, MethodInfo invokedMethod, object[] args)
            {
                dynamic taskResult = _delegate.Invoke(proxy, invokedMethod, args);
                return taskResult.Result;
            }

        }

        private class FireAndForget : IInvocationHandler
        {

            private IInvocationHandler _delegate;

            public FireAndForget(IInvocationHandler @delegate)
            {
                _delegate = @delegate;
            }

            public object Invoke(object proxy, MethodInfo invokedMethod, object[] args)
            {
                _delegate.Invoke(proxy, invokedMethod, args);
                return null;
            }

        }
        
        private class MetaDataExtractor 
        {

            private readonly int _argumentIndex;
            private readonly string _metaDataKey;
            
            public MetaDataExtractor(int argumentIndex, string metaDataKey) 
            {
                _argumentIndex = argumentIndex;
                _metaDataKey = metaDataKey;
            }

            public void AddMetaData(object[] args, IDictionary<string, object> metaData) 
            {
                var parameterValue = (IEnumerable)args[_argumentIndex];

                
                if (_metaDataKey == null) 
                {
                    if (parameterValue != null) 
                    {
                        var parameterDictionary = parameterValue.Cast<object>().ToDictionary((dynamic kv) => (string)kv.Key, (dynamic kv) => (object) kv.Value);
                        parameterDictionary.ForEach(metaData.Add);
                    }
                } 
                else 
                {
                    metaData.Add(_metaDataKey, parameterValue);
                }
            }
        }
        private class TypeSafeCallbackWrapper : ICommandCallback
        {

            private ICommandCallback _delegate;
            private Type _parameterType;

        public TypeSafeCallbackWrapper(ICommandCallback @delegate) 
        {
            _delegate = @delegate;
            Type discoveredParameterType = typeof(object);
            foreach (var m in @delegate.GetType().GetMethods()) 
            {
                if (m.IsGenericMethod && m.GetGenericArguments().Length == 2 && m.GetGenericArguments()[1] != typeof(object) && "OnSuccess".Equals(m.Name) && m.IsPublic) 
                {
                    discoveredParameterType = m.GetParameterTypes()[1];
                    if (discoveredParameterType != typeof(object)) 
                    {
                        break;
                    }
                }
            }
            _parameterType = discoveredParameterType;
        }

        public void OnSuccess(ICommandMessage commandMessage, object result) 
        {
            if (_parameterType.IsInstanceOfType(result) || result == null) 
            {
                _delegate.OnSuccess(commandMessage, result);
            }
        }

        public void OnFailure(ICommandMessage commandMessage, Exception cause) 
        {
            _delegate.OnFailure(commandMessage, cause);
        }
    }
    }
}