namespace EditorEX.Managers
{
    // This is terrible, but oh well...
    internal class ColorManagerInstanceManager
    {
        public static ColorManager Instance { get; private set; }

        public ColorManagerInstanceManager(ColorManager colorManager)
        {
            Instance = colorManager;
        }
    }
}
