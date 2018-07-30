using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NAxonFramework.Messaging;
using NAxonFramework.Monitoring;

namespace NAxonFramework.CommandHandling
{
    public class AsynchronousCommandBus : SimpleCommandBus, IDisposable
    {
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private bool _shuttingDown = false;
        private HashSet<Task> _tasks = new HashSet<Task>();
        private object _lock;
        
        public AsynchronousCommandBus(ILogger<SimpleCommandBus> logger, IMessageMonitor messageMonitor) : base(logger, messageMonitor)
        {
        }

        protected override void Handle<R>(ICommandMessage command, IMessageHandler<ICommandMessage> handler, ICommandCallback<R> callback)
        {
            lock(_lock)
            {
                if (!_shuttingDown)
                {
                    var task = Task.Run(() => base.Handle(command, handler, callback), _cancellationTokenSource.Token);
                    task.ContinueWith(x => _tasks.Remove(task));
                    _tasks.Add(task);
                }                
            }
        }

        public async Task Shutdown()
        {

            
            bool allComplete;
            lock (_lock)
            {
                _shuttingDown = true;

                
                allComplete = Task.WaitAll(_tasks.ToArray(), TimeSpan.FromSeconds(2));
            }

            if (!allComplete)
            {
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
            
        }

        public void Dispose() => Shutdown().Wait();
    }
}