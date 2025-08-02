using HospitalAppointmentSystem.EventBus.Consumers;
using HospitalAppointmentSystem.EventBus.Events;
using HospitalAppointmentSystem.EventBus.Interfaces;
using HospitalAppointmentSystem.EventBus.Publishers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace HospitalAppointmentSystem.EventBus.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventBus(this IServiceCollection services, string connectionString)
    {
        // Register RabbitMQ connection
        services.AddSingleton<IConnection>(sp =>
        {
            var factory = new ConnectionFactory()
            {
                Uri = new Uri(connectionString),
                DispatchConsumersAsync = true
            };
            return factory.CreateConnection();
        });

        // Register EventBus
        services.AddSingleton<IEventBus, RabbitMQEventBus>();

        // Register Publishers
        services.AddScoped<AppointmentBookedPublisher>();
        services.AddScoped<AppointmentCancelledPublisher>();

        // Register Event Handlers
        services.AddScoped<IEventHandler<AppointmentBookedEvent>, AppointmentBookedConsumer>();
        services.AddScoped<IEventHandler<AppointmentCancelledEvent>, AppointmentCancelledConsumer>();

        return services;
    }

    public static void ConfigureEventBus(this IServiceProvider serviceProvider)
    {
        var eventBus = serviceProvider.GetRequiredService<IEventBus>();

        // Subscribe to events
        eventBus.Subscribe<AppointmentBookedEvent, AppointmentBookedConsumer>();
        eventBus.Subscribe<AppointmentCancelledEvent, AppointmentCancelledConsumer>();
    }
}
