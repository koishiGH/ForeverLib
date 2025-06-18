using ForeverLib.Utils;

namespace ForeverLib.Core
{
    public abstract class ModBase
    {
        public ModMetadata Metadata { get; internal set; } = null!;
        protected internal Logger Logger { get; internal set; } = null!;

        public abstract void OnLoad();
        public abstract void OnUnload();

        public virtual void OnGameStart() { }
        public virtual void OnGameExit() { }
        public virtual void OnSceneLoaded(string sceneName) { }
        public virtual void OnSceneUnloaded(string sceneName) { }
    }
} 