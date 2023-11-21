using UnityEngine.Timeline;

namespace Gameplay.Level
{
    [TrackClipType(typeof(BurstSignalReceiver ))]
    [TrackBindingType(typeof(BurstSignalReceiver ))]
    public class BurstSignalReceiverTrack : TrackAsset {}
}