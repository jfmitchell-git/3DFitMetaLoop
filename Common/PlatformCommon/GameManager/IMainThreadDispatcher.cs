#if !BACKOFFICE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace MetaLoop.Common.PlatformCommon.GameManager
{
    public interface IMainThreadDispatcher
    {
        void Enqueue(Action action);

    }
}
#endif
