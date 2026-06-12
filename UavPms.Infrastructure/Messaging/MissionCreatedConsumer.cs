using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using UavPms.Core.Contracts;
using UavPms.Core.Entities;
using UavPms.Core.Interfaces.Repositories;

namespace UavPms.Infrastructure.Messaging;

public class MissionCreatedConsumer(
    ILogger<MissionCreatedConsumer> logger,
    RabbitMqConnection rabbitMqConnection,
    IServiceScopeFactory scopeFactory) : BackgroundService
{
    private readonly ILogger<MissionCreatedConsumer> _logger = logger;
    private readonly RabbitMqConnection _rabbitMqConnection = rabbitMqConnection;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private IConnection? _connection;
    private IChannel? _channel;
    private const string QueueName = "MissionCreatedQueue";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MissionCreatedConsumer background service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Connecting to RabbitMQ for MissionCreated consumer...");
                _connection = await _rabbitMqConnection.CreateConnectionAsync(stoppingToken);
                _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

                await _channel.QueueDeclareAsync(
                    queue: QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    cancellationToken: stoppingToken);

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += async (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        _logger.LogInformation("MissionCreated message received: {Message}", message);

                        var missionEvent = JsonSerializer.Deserialize<MissionCreatedEvent>(message, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (missionEvent != null)
                        {
                            using (var scope = _scopeFactory.CreateScope())
                            {
                                var notificationRepo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

                                var notification = new Notification
                                {
                                    Id = Guid.NewGuid(),
                                    UserId = missionEvent.AssignedToUserId.ToString(), // Notifying the assigned engineer
                                    Title = "Nhiệm vụ bay mới được phân công",
                                    Content = $"Bạn đã được phân công nhiệm vụ bay '{missionEvent.Title}' (Drone: {missionEvent.DroneCode}) lập lịch lúc {missionEvent.CreatedAt}.",
                                    Type = "MissionCreated",
                                    CreatedAt = DateTime.UtcNow,
                                    IsRead = false
                                };

                                await notificationRepo.AddAsync(notification);
                                _logger.LogInformation("Notification saved for new mission {MissionId} assigned to user {UserId}.", missionEvent.MissionId, missionEvent.AssignedToUserId);
                            }
                        }

                        await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing MissionCreated event.");
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, true, stoppingToken);
                    }
                };

                await _channel.BasicConsumeAsync(
                    queue: QueueName,
                    autoAck: false,
                    consumer: consumer,
                    cancellationToken: stoppingToken);

                _logger.LogInformation("MissionCreatedConsumer is now listening on queue {QueueName}.", QueueName);
                break; // Connection successful, break retry loop
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to connect or establish channel to RabbitMQ. Retrying in 5 seconds...");
                try
                {
                    await Task.Delay(5000, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("MissionCreatedConsumer background service is stopping.");
        if (_channel != null)
        {
            await _channel.CloseAsync(cancellationToken);
        }
        if (_connection != null)
        {
            await _connection.CloseAsync(cancellationToken);
        }
        await base.StopAsync(cancellationToken);
    }
}
