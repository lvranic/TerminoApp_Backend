using HotChocolate.Types;
using TerminoApp_NewBackend.Models;

namespace TerminoApp_NewBackend.GraphQL.Types
{
    public class UserInputType : InputObjectType<User>
    {
        protected override void Configure(IInputObjectTypeDescriptor<User> descriptor)
        {
            descriptor.Field(f => f.Id).Ignore();
            descriptor.Field(f => f.Name);
            descriptor.Field(f => f.Email);
            descriptor.Field(f => f.Phone);
            descriptor.Field(f => f.Role);
            descriptor.Field(f => f.Password);
        }
    }
}