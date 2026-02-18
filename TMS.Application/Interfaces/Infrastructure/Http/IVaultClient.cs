namespace TMS.Application.Interfaces.Infrastructure.Http
{
    public interface IVaultClient
    {
        Task<T> GetSecretAsync<T>(string path, CancellationToken cancellationToken = default);
    }
}