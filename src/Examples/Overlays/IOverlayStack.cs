namespace Examples.Overlays
{
    public interface IOverlayStack
    {
        void AddBusyOverlay();
        void AddPrivacyOverlay();
        void PopOverlay();
    }
}
