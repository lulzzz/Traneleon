﻿using System;
using System.Collections.Generic;

namespace Acklann.WebFlow.Compilation
{
    public interface IActorObserver : IObserver<ICompilierResult>
    {
        int Count { get; }

        IEnumerable<ICompilierResult> GetResults();

        void WaitForCompletion(int limit, int millisecondsTimeout);
    }
}