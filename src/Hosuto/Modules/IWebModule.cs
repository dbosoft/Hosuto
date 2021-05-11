namespace Dbosoft.Hosuto.Modules
{
    /// <summary>
    /// Marker interface to declare this type as Hosuto WebModule. 
    /// </summary>
    public interface IWebModule
    {
        /// <summary>
        /// This is a helper property to set the path of modules in UseAspNetCore.
        /// It is up to you how to use it - it will not be used by Hosuto framework.
        /// </summary>
        string Path { get; }

    }
}