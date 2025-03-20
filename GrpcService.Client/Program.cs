using Grpc.Core;
using Grpc.Net.Client;

namespace GrpcService.Client;

class Program
{
    static async Task Main(string[] args)
    {
        using var channel = GrpcChannel.ForAddress("https://localhost:7116");
        await ChatBiDiStream(channel);
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    public static async Task UniaryCall(GrpcChannel channel)
    {
        var client = new Greeter.GreeterClient(channel);
        var reply = await client.SayHelloAsync(new HelloRequest { Name = "Wicked Awesome Client" });
        Console.WriteLine($"Greeting: {reply.Message}");
    }

    public static async Task RequestCurrentWeather(GrpcChannel channel)
    {
        var client = new WeatherService.WeatherServiceClient(channel);
        var reply = await client.GetCurrentWeatherAsync(
            new GetCurrentWeatherForCityRequest { City = "London", Units = Units.Metric }
        );
        Console.WriteLine($"Temp: {reply.Temperature}\r\nFL: {reply.FeelsLike}");
        Console.WriteLine(reply);
    }

    public static async Task StreamCurrentWeatherRequest(GrpcChannel channel)
    {
        var client = new WeatherService.WeatherServiceClient(channel);
        using var call = client.GetCurrentWeatherStream(
            new GetCurrentWeatherForCityRequest { City = "London", Units = Units.Imperial }
        );
        while (await call.ResponseStream.MoveNext(new CancellationToken()))
        {
            Console.WriteLine($"From Server: {call.ResponseStream.Current}");
        }
    }

    public static async Task ClientStreamRequest(GrpcChannel channel)
    {
        var client = new WeatherService.WeatherServiceClient(channel);
        using var call = client.GetMultiCurrentWeatherStream();

        for (var i = 0; i < 3; i++)
        {
            await call.RequestStream.WriteAsync(
                new GetCurrentWeatherForCityRequest { City = "London", Units = Units.Imperial }
            );
        }
        await call.RequestStream.CompleteAsync();

        var response = await call;
        Console.WriteLine(response);
    }

    public static async Task PrintStream(GrpcChannel channel)
    {
        var client = new WeatherService.WeatherServiceClient(channel);
        using var call = client.PrintStream();

        for (var i = 0; i < 3; i++)
        {
            await call.RequestStream.WriteAsync(
                new PrintRequest { Message = $"This is request #{i}" }
            );
        }
        await call.RequestStream.CompleteAsync();

        var response = await call;
        Console.WriteLine(response);
    }

    public static async Task ChatBiDiStream(GrpcChannel channel)
    {
        var client = new Chat.ChatClient(channel);
        using var call = client.SendMessage();

        Console.WriteLine("Starting task to receive messages");
        var readTask = Task.Run(async () =>
        {
            await foreach (var response in call.ResponseStream.ReadAllAsync())
            {
                Console.WriteLine(response);
            }
        });

        Console.WriteLine("Starting to send message");
        Console.WriteLine("Type a message to echo then press enter.");
        while (true)
        {
            var result = Console.ReadLine();
            if (string.IsNullOrEmpty(result))
            {
                break;
            }

            await call.RequestStream.WriteAsync(new ClientToServerMessage { Message = result });
        }
        Console.WriteLine("Disconnecting");
        await call.RequestStream.CompleteAsync();
        await readTask;
    }
}
