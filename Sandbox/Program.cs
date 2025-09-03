using LinaSys.Sandbox.Modules.BusinessIncubator.Application.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace LinaSys.Sandbox;

public partial class Program
{
    public static async Task Main(string[] args)
    {
        // Check if we want to run the Mailgun test
        if (args.Length > 0 && args[0].ToLower() == "mailgun")
        {
            await TestMailgun.RunAsync();
            return;
        }

        var services = Bootstrap(args);

        var x = services.GetRequiredService<CreateBusinessIncubatorCommandTest>();
        //// var x = services.GetRequiredService<UpdateBusinessIncubatorCommandTest>();

        await x.RunTestCase1Async();

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    private static partial IServiceProvider Bootstrap(string[] args);
}
