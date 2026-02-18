namespace TMS.Application.Interfaces.Services
{
    public interface ISecretService
    {
        Task<TSecret> GetSecretAsync<TVaultResponse, TSecret>(string path, CancellationToken cancellationToken);
    }
}