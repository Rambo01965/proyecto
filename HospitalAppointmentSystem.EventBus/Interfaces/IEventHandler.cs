using HospitalAppointmentSystem.EventBus.Events;

namespace HospitalAppointmentSystem.EventBus.Interfaces;

public interface IEventHandler<in T> where T : IEvent
{
    Task HandleAsync(T @event);
}
