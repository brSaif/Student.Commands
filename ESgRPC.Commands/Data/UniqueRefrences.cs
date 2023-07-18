using gRPCOnHttp3.Domain;

namespace gRPCOnHttp3.Data;

public class UniqueReference
{
    private UniqueReference() { }
    public UniqueReference(Student student)
    {
        Id = student.Id;
        Name = student.Name;
    }

    public Guid Id { get; }
    public string Name { get; }
}