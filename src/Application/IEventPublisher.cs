using RideShare.Events;

namespace RideShare.Application;

public interface IEventPublisher
{
    void Publish(ITripEvent tripEvent);
}
