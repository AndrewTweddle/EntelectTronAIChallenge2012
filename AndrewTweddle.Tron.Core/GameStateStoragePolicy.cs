using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AndrewTweddle.Tron.Core
{
    public enum GameStateStoragePolicy: byte
    {
        StrongReferenceOnRootAndLeafNodeOnly,
        WeakReference,
        StrongReference,
        NoReference
    }
}
