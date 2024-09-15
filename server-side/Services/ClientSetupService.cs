using TaskMonitor.DTOs;

namespace TaskMonitor.Services
{
    public interface IClientSetupService
    {
        ClientSetupDTO Read();
    }

    public class ClientSetupService(IConfiguration configuration) : IClientSetupService
    {
        private readonly IConfiguration _configuration = configuration;

        public ClientSetupDTO Read()
        {
            ClientSetupDTO clientSetupDTO = new();
            IConfiguration configuration = _configuration.GetRequiredSection("ClientSetup");
            configuration.Bind(clientSetupDTO);
            return clientSetupDTO;
        }
    }
}
