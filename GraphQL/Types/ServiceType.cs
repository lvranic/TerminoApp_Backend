using HotChocolate.Types;
using TerminoApp_NewBackend.Models;

namespace TerminoApp_NewBackend.GraphQL.Types
{
    public class ServiceType : ObjectType<Service>
    {
        protected override void Configure(IObjectTypeDescriptor<Service> descriptor)
        {
            descriptor.Field(s => s.Id);
            descriptor.Field(s => s.Name);
            descriptor.Field(s => s.DurationMinutes);
            descriptor.Field(s => s.ProviderId);
        }
    }
}