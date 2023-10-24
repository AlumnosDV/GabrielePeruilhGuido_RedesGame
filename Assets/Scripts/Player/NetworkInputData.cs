using Fusion;

namespace RedesGame.Player
{
    public struct NetworkInputData : INetworkInput
    {
        public float xMovement;
        public NetworkBool isJumpPressed;
        public NetworkBool isFirePressed;
    }
}