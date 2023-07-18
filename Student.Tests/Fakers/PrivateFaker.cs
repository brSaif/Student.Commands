using Bogus;

namespace ES_gRPC.UnitTests.Fakers;

public class PrivateFaker<T> : Faker<T> where T : class
{
    public PrivateFaker<T> UsePrivateConstructor()
    {
        return (PrivateFaker<T>)base.CustomInstantiator(f => Activator.CreateInstance(typeof(T), nonPublic: true) as T ?? throw new InvalidOperationException());
    }
}