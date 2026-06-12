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

public class DefectDetectedConsumer(ILogger<DefectDetectedConsumer> logger,
        RabbitMqConnection rabbitMqConnection,
        IServiceScopeFactory scopeFactory) : BackgroundService
{
    private readonly ILogger<DefectDetectedConsumer> _logger = logger;
    private readonly RabbitMqConnection _rabbitMqConnection = rabbitMqConnection;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private IConnection? _connection;
    private IChannel? _channel;
    private const string QueueName = "DefectDetectedQueue";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DefectDetectedConsumer background service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Connecting to RabbitMQ for DefectDetected consumer...");
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
                        _logger.LogInformation("DefectDetected message received: {Message}", message);

                        var defectEvent = JsonSerializer.Deserialize<DefectDetectedEvent>(message, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (defectEvent != null)
                        {
                            using (var scope = _scopeFactory.CreateScope())
                            {
                                var notificationRepo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

                                var notification = new Notification
                                {
                                    Id = Guid.NewGuid(),
                                    UserId = "Admin", // Notifying technical managers (Admins)
                                    Title = "Cảnh báo lỗi thiết bị!",
                                    Content = $"Phát hiện lỗi dạng '{defectEvent.DefectType}' trên chuyến bay '{defectEvent.MissionId}' lúc {defectEvent.DetectedAt}.",
                                    Type = "DefectDetected",
                                    CreatedAt = DateTime.UtcNow,
                                    IsRead = false
                                };

                                await notificationRepo.AddAsync(notification);
                                _logger.LogInformation("Notification saved for defect detected on mission {MissionId}.", defectEvent.MissionId);
                            }
                        }

                        await _channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing DefectDetected event.");
                        // Nack message to requeue it or put in DLQ
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, true, stoppingToken);
                    }
                };

                await _channel.BasicConsumeAsync(
                    queue: QueueName,
                    autoAck: false,
                    consumer: consumer,
                    cancellationToken: stoppingToken);

                _logger.LogInformation("DefectDetectedConsumer is now listening on queue {QueueName}.", QueueName);
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
        _logger.LogInformation("DefectDetectedConsumer background service is stopping.");
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