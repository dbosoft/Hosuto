namespace Dbosoft.Hosuto.Modules
{
    public abstract class WebModule : IModule
    {
        public abstract string Path { get; }

        public abstract string Name { get; }
    }
}