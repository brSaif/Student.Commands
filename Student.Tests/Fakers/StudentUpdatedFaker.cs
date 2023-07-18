using gRPCOnHttp3.CreateStudent;
using gRPCOnHttp3.UpdateStudent;

namespace ES_gRPC.UnitTests.Fakers;

public class StudentUpdatedFaker : PrivateFaker<StudentUpdatedEvent>
{
    public StudentUpdatedFaker()
    {
        UsePrivateConstructor();
        RuleFor(r => r.Version, 1);
        RuleFor(r => r.DateTime, DateTime.Now);
    }
}

public class StudentUpdatedDataFaker : PrivateFaker<UpdateStudentData>
{
    public StudentUpdatedDataFaker()
    {
        UsePrivateConstructor();
        
        RuleFor(r => r.Name, f => f.Person.FullName);
        RuleFor(r => r.Email, f => f.Person.Email);
        RuleFor(r => r.PhoneNumber, f => f.Random.AlphaNumeric(9));
    }
}