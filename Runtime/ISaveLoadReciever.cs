using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Recstazy.GameSaver
{
    public interface ISaveLoadReciever
    {
        void OnBeforeSerialize();
        void OnAfterDeserialize();
    }
}