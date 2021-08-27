namespace Ocuda.Utility.Services.Interfaces
{
    public interface IPathResolverService
    {
        string GetPublicContentLink(params object[] pathElement);
        string GetPublicContentFilePath(string fileName = default, params object[] pathElement);
        string GetPrivateContentFilePath(string fileName = default, params object[] pathElement);
    }
}
