using LinaSys.BusinessIncubator.Application.BusinessIncubator.Commands;
using MediatR;
using static LinaSys.Sandbox.Program;

namespace LinaSys.Sandbox.Modules.BusinessIncubator.Application.Commands;

[ScopedDependency]
public class CreateBusinessIncubatorCommandTest(IMediator mediatr)
{
    public async Task RunTestCase1Async()
    {
        var command = new CreateBusinessIncubatorCommand("my namej", "my descrip", "mynaekj");
        var response = await mediatr.Send(command);

        if (response.IsFailure)
        {
            Console.WriteLine($"Response received: Empty GUID");
        }
        else
        {
            Console.WriteLine("Response received: {0:B}", response.Value);
        }
    }
}
