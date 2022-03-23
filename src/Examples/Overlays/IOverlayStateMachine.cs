namespace Examples.Overlays
{
    public interface IOverlayStateMachine
    {
        void HandleEvent(OverlayEvent trigger);
    }
}
