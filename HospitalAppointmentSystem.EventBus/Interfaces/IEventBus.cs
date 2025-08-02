using HospitalAppointmentSystem.EventBus.Events;

namespace HospitalAppointmentSystem.EventBus.Interfaces;

public interface IEventBus
{
    Task PublishAsync<T>(T @event) where T : class, IEvent;
    void Subscribe<T, TH>()
        where T : class, IEvent
        where TH : class, IEventHandler<T>;
    void Unsubscribe<T, TH>()
        where T : class, IEvent
        where TH : class, IEventHandler<T>;
}
