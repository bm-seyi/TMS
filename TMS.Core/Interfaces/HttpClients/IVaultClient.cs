namespace TMS.Core.Interfaces.HttpClients
{
    public interface IVaultClient
    {
        Task<T> GetSecretAsync<T>(string path, CancellationToken cancellationToken = default);
    }
}