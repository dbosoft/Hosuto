namespace Dbosoft.Hosuto.Modules
{
    /// <summary>
    /// This is a marker interface that can be used as replacement for old
    /// IModule interface with Name property. It will be automatically used in .NET Standard 2.1
    /// version to search the module content folder. 
    /// </summary>
    public interface INamedModule
    {
        string Name { get; }
        
    }
}