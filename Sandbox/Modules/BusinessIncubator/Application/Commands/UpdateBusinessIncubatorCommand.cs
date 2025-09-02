using LinaSys.BusinessIncubator.Application.BusinessIncubator.Commands;
using MediatR;
using static LinaSys.Sandbox.Program;

namespace LinaSys.Sandbox.Modules.BusinessIncubator.Application.Commands;

[ScopedDependency]
public class UpdateBusinessIncubatorCommandTest(IMediator mediatr)
{
    public async Task RunTestCase1Async()
    {
        var command = new UpdateBusinessIncubatorCommand(Guid.Parse("08133562-7495-45fb-bc4e-287fc5435975"), "my namej", "my descrip", "mynae");
        var response = await mediatr.Send(command);

        if (response.IsFailure)
        {
            Console.WriteLine("Response received: {0}", response.ErrorCode.ToString());
        }
        else
        {
            Console.WriteLine("Response received: {0}", response);
        }
    }
}
