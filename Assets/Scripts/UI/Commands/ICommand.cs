namespace RedesGame.UI.Commands
{
    internal interface ICommand
    {
        void Execute();
        void Undo();
    }
}