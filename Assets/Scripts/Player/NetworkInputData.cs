using Fusion;

namespace RedesGame.Player
{
    public struct NetworkInputData : INetworkInput
    {
        public float Horizontal;
        public float Vertical;

        public NetworkButtons Buttons; // Jump, Fire, FallThrough, etc.
    }

    public enum MyButtons
    {
        Jump = 0,          // W
        Fire = 1,          // Space
        FallThrough = 2    // S
    }
}