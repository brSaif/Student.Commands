using Calzolari.Grpc.Net.Client.Validation;
using Google.Protobuf.WellKnownTypes;
using gPPCOnHttp3Client;
using Grpc.Core;
// using gPPCOnHttp3Server;
using Grpc.Net.Client;
using gRPCOnHttp3Client;
using Microsoft.Extensions.DependencyInjection;


// IoC way to use gRPC clients.
var services = new ServiceCollection();
services.AddGrpcClient<Student.StudentClient>(o =>
{
    o.Address = new Uri("https://localhost:5001");
});

var sc = services.BuildServiceProvider();
 var studentClient = new StudentService(sc);


//  var studentRequest = new CreateRequest() 
//      { Name = "Fujitora Issho", Email = "Test@Test.com", PhoneNumber = "1451212678"};
// try
// {
//
//     var res = studentClient.Create(studentRequest);
//     Console.WriteLine($"[gRPC Client][Program.cs] logging create student response {res}");
// }
// catch (RpcException e)
// {
//     var errors = e.GetValidationErrors();
//     Console.WriteLine(e.Message);
// }

// The update student request starts here.
Console.WriteLine("Enter the student aggregate id");
Console.Write("_> ");
var studentId = "923d7b10-4dca-45da-90e2-ae3b3ea00397";//Console.ReadLine();
var updateStudentRequest = new UpdateStudentRequest() 
     { Id = studentId, Name = "Fujitora Issho", Email = "Test001@ecom.ltd", PhoneNumber = "9874563210"};
Console.WriteLine(updateStudentRequest.GetType().Name);
Console.WriteLine(updateStudentRequest.GetType().Name);

Console.WriteLine($"[gRPC Client][Program.cs] logging change email request {updateStudentRequest}");
 var result = studentClient.Update(updateStudentRequest);
 Console.WriteLine($"[gRPC Client][Program.cs] logging change email response {result}");


// The manual way of creating clients

// var httpHandler = new HttpClientHandler();
// httpHandler.ServerCertificateCustomValidationCallback
//     = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
//
// var httpClient = new HttpClient(httpHandler);
// using var channel = GrpcChannel.ForAddress(
//     "https://localhost:5001", 
//     new GrpcChannelOptions { HttpClient = httpClient }
//     );
//
//
// // var greeterClient = new Greeter.GreeterClient(channel);
// // var greetRequest = new HelloRequest(){Name = "Saibr"};
// // var greetResponse = await greeterClient.SayHelloAsync(greetRequest);
// //
// // Console.WriteLine(greetResponse.Message);
//
// // var messageClient = new Message.MessageClient(channel);
// //
// // var message = new RequestMessage() { Data = "lorem ...", SendTime = DateTime.UtcNow.ToTimestamp() };
// // var reply = await messageClient.SendAsync(message);
// // Console.WriteLine("[Console logging] Reply received ...");
// // Console.WriteLine("[Console logging] Reply data : " + reply.ReceivedTime);