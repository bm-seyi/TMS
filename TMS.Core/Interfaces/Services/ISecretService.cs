namespace TMS.Core.Interfaces.Services
{
    public interface ISecretService
    {
        Task<TSecret> GetSecretAsync<TVaultResponse, TSecret>(string path, CancellationToken cancellationToken);
    }
}