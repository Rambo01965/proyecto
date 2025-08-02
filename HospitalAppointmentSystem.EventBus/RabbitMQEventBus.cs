using HospitalAppointmentSystem.EventBus.Events;
using HospitalAppointmentSystem.EventBus.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace HospitalAppointmentSystem.EventBus;

public class RabbitMQEventBus : IEventBus, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMQEventBus> _logger;
    private readonly Dictionary<string, List<Type>> _handlers;
    private readonly List<Type> _eventTypes;

    public RabbitMQEventBus(
        IConnection connection,
        IServiceProvider serviceProvider,
        ILogger<RabbitMQEventBus> logger)
    {
        _connection = connection;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _handlers = new Dictionary<string, List<Type>>();
        _eventTypes = new List<Type>();
        _channel = _connection.CreateModel();
    }

    public async Task PublishAsync<T>(T @event) where T : class, IEvent
    {
        var eventName = @event.GetType().Name;
        var message = JsonConvert.SerializeObject(@event);
        var body = Encoding.UTF8.GetBytes(message);

        _channel.ExchangeDeclare(exchange: "hospital_events", type: ExchangeType.Direct);

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;

        _channel.BasicPublish(
            exchange: "hospital_events",
            routingKey: eventName,
            basicProperties: properties,
            body: body);

        _logger.LogInformation("Published event {EventName} with ID {EventId}", eventName, @event.Id);
        await Task.CompletedTask;
    }

    public void Subscribe<T, TH>()
        where T : class, IEvent
        where TH : class, IEventHandler<T>
    {
        var eventName = typeof(T).Name;
        var handlerType = typeof(TH);

        if (!_handlers.ContainsKey(eventName))
        {
            _handlers[eventName] = new List<Type>();
        }

        if (_handlers[eventName].Any(s => s == handlerType))
        {
            _logger.LogWarning("Handler {HandlerType} already registered for event {EventName}", handlerType.Name, eventName);
            return;
        }

        _handlers[eventName].Add(handlerType);

        if (!_eventTypes.Contains(typeof(T)))
        {
            _eventTypes.Add(typeof(T));
        }

        StartBasicConsume<T>();
        _logger.LogInformation("Subscribed to event {EventName} with handler {HandlerType}", eventName, handlerType.Name);
    }

    public void Unsubscribe<T, TH>()
        where T : class, IEvent
        where TH : class, IEventHandler<T>
    {
        var eventName = typeof(T).Name;
        var handlerType = typeof(TH);

        if (_handlers.ContainsKey(eventName))
        {
            _handlers[eventName].Remove(handlerType);
            if (_handlers[eventName].Count == 0)
            {
                _handlers.Remove(eventName);
                var eventType = _eventTypes.SingleOrDefault(e => e.Name == eventName);
                if (eventType != null)
                {
                    _eventTypes.Remove(eventType);
                }
            }
        }

        _logger.LogInformation("Unsubscribed from event {EventName} with handler {HandlerType}", eventName, handlerType.Name);
    }

    private void StartBasicConsume<T>() where T : class, IEvent
    {
        var eventName = typeof(T).Name;

        _channel.ExchangeDeclare(exchange: "hospital_events", type: ExchangeType.Direct);

        var queueName = $"hospital_queue_{eventName}";
        _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        _channel.QueueBind(queue: queueName, exchange: "hospital_events", routingKey: eventName);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            var eventName = ea.RoutingKey;
            var message = Encoding.UTF8.GetString(ea.Body.Span);

            try
            {
                await ProcessEvent(eventName, message);
                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing event {EventName}: {Message}", eventName, message);
                _channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        _channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
    }

    private async Task ProcessEvent(string eventName, string message)
    {
        if (_handlers.ContainsKey(eventName))
        {
            using var scope = _serviceProvider.CreateScope();
            var subscriptions = _handlers[eventName];

            foreach (var subscription in subscriptions)
            {
                var handler = scope.ServiceProvider.GetService(subscription);
                if (handler == null) continue;

                var eventType = _eventTypes.SingleOrDefault(t => t.Name == eventName);
                if (eventType == null) continue;

                var @event = JsonConvert.DeserializeObject(message, eventType);
                var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);
                var method = concreteType.GetMethod("HandleAsync");

                if (method != null)
                {
                    await (Task)method.Invoke(handler, new object[] { @event })!;
                }
            }
        }
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
