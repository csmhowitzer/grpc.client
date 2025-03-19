using Grpc.Net.Client;

namespace GrpcService.Client;

class Program
{
    static async Task Main(string[] args)
    {
        using var channel = GrpcChannel.ForAddress("https://localhost:7116");
        await StreamCurrentWeatherRequest(channel);
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
        var wClient = new WeatherService.WeatherServiceClient(channel);
        var wReply = await wClient.GetCurrentWeatherAsync(
            new GetCurrentWeatherForCityRequest
            {
                City = "London",
                Units = GetCurrentWeatherForCityRequest.Types.Units.Metric,
            }
        );
        Console.WriteLine($"Temp: {wReply.Temperature}\r\nFL: {wReply.FeelsLike}");
        Console.WriteLine(wReply);
    }

    public static async Task StreamCurrentWeatherRequest(GrpcChannel channel)
    {
        var sClient = new WeatherService.WeatherServiceClient(channel);
        using var call = sClient.GetCurrentWeatherStream(
            new GetCurrentWeatherForCityRequest
            {
                City = "London",
                Units = GetCurrentWeatherForCityRequest.Types.Units.Imperial,
            }
        );
        while (await call.ResponseStream.MoveNext(new CancellationToken()))
        {
            Console.WriteLine($"From Server: {call.ResponseStream.Current}");
        }
    }
}
