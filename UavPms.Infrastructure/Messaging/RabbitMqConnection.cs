using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Threading;
using System.Threading.Tasks;

namespace UavPms.Infrastructure.Messaging;

public class RabbitMqConnection(IConfiguration configuration)
{
    private readonly ConnectionFactory _connectionFactory = new()
    {
        HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
        UserName = configuration["RabbitMQ:UserName"] ?? "guest",
        Password = configuration["RabbitMQ:Password"] ?? "guest",
        Port = string.IsNullOrEmpty(configuration["RabbitMQ:Port"]) 
            ? 5672 
            : int.Parse(configuration["RabbitMQ:Port"]!)
    };

    public Task<IConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        return _connectionFactory.CreateConnectionAsync(cancellationToken);
    }
}
