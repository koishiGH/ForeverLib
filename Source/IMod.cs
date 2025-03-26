namespace ForeverLib
{
    public interface IMod
    {
        string ModID { get; }
        string ModName { get; }
        string ModVersion { get; }
        
        void Initialize();
        void OnEnabled();
        void OnDisabled();
    }
} 