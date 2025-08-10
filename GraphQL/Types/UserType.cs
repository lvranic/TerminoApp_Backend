using HotChocolate.Types;
using TerminoApp_NewBackend.Models;

namespace TerminoApp_NewBackend.GraphQL.Types
{
    public class UserType : ObjectType<User>
    {
        protected override void Configure(IObjectTypeDescriptor<User> descriptor)
        {
            descriptor.Field(f => f.Id).Type<NonNullType<IdType>>();
            descriptor.Field(f => f.Name);
            descriptor.Field(f => f.Email);
            descriptor.Field(f => f.Phone);
            descriptor.Field(f => f.Role);
            descriptor.Field(f => f.Password);
        }
    }
}