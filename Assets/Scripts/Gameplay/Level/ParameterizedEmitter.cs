using Configuration;
using UnityEngine.Timeline;

namespace Gameplay.Level
{
    public class ParameterizedEmitter<T> : SignalEmitter
    {
        public T parameter;
    }
}
