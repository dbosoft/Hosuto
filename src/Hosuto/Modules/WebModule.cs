namespace Dbosoft.Hosuto.Modules
{
    /// <summary>
    /// Web module is the abstract base class of a module that runs a aspnetcore host
    /// It replaces the Startup class and can be used almost like a startup class.
    /// See <see cref="IModule"/> for conventions.
    /// In addition to the ConfigureServices you should also add a Configure method to the
    /// web module.   
    /// </summary>
    public abstract class WebModule : IModule
    {
        public abstract string Path { get; }

        public abstract string Name { get; }
    }
}