using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Acklann.WebFlow.Compilation
{
    internal class FileProcessorObserver : IActorObserver
    {
        public int Count => _count;

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(ICompilierResult value)
        {
            _count++;
            _results.Push(value);
            Commands.Log.Item(value);
        }

        public IEnumerable<ICompilierResult> GetResults()
        {
            while (_results.IsEmpty == false)
            {
                if (_results.TryPop(out ICompilierResult result)) yield return result;
            }
        }

        public void WaitForCompletion(int limit, int millisecondsTimeout)
        {
            _count = 0;
            bool notExpired = true;
            long startTime = DateTime.Now.Ticks;
            long timeout = TimeSpan.FromMilliseconds(millisecondsTimeout).Ticks;

            while ((_count < limit) && notExpired)
            {
                System.Threading.Thread.Sleep(1000);
                notExpired = ((DateTime.Now.Ticks - startTime) < timeout);
            }
        }

        #region Private Members

        private readonly ConcurrentStack<ICompilierResult> _results = new ConcurrentStack<ICompilierResult>();
        private volatile int _count;

        #endregion Private Members
    }
}