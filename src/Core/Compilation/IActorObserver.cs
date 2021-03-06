﻿using System;
using System.Collections.Generic;

namespace Acklann.Traneleon.Compilation
{
    public interface IActorObserver : IObserver<ICompilierResult>, IDisposable
    {
        int Count { get; }

        IEnumerable<ICompilierResult> GetResults();

        void WaitForCompletion(int limit, int millisecondsTimeout);
    }
}