using gRPCOnHttp3.CreateStudent;

namespace ES_gRPC.UnitTests.Fakers;

public class StudentCreatedFaker : PrivateFaker<StudentCreatedEvent>
{
    public StudentCreatedFaker()
    {
        UsePrivateConstructor();
        
        RuleFor(r => r.AggregateId, f => f.Random.Guid());
        RuleFor(r => r.Sequence, 1);
        RuleFor(r => r.Version, 1);
        RuleFor(r => r.DateTime, DateTime.Now);
    }
}

public class StudentCreatedDataFaker : PrivateFaker<StudentCreatedData>
{
    public StudentCreatedDataFaker()
    {
        UsePrivateConstructor();
        
        RuleFor(r => r.Name, f => f.Person.FullName);
        RuleFor(r => r.Email, f => f.Person.Email);
        RuleFor(r => r.PhoneNumber, f => f.Random.AlphaNumeric(9));
    } 
}