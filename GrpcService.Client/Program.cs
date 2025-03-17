using Grpc.Net.Client;

namespace GrpcService.Client;

class Program
{
    static async Task Main(string[] args)
    {
        // using var channel = GrpcChannel.ForAddress("https://localhost:7116");
        // var client = new Greeter.GreeterClient(channel);
        // var reply = await client.SayHelloAsync(new HelloRequest { Name = "Wicked Awesome Client" });
        // Console.WriteLine($"Greeting: {reply.Message}");

        // var wClient = new WeatherService.WeatherServiceClient(channel);
        // var wReply = await wClient.GetCurrentWeatherAsync(
        //     new GetCurrentWeatherForCityRequest
        //     {
        //         City = "London",
        //         Units = GetCurrentWeatherForCityRequest.Types.Units.Metric,
        //     }
        // );
        // Console.WriteLine($"Temp: {wReply.Temperature}\r\nFL: {wReply.FeelsLike}");
        // Console.WriteLine(wReply);

        using var channel = GrpcChannel.ForAddress("https://localhost:7116");
        var sClient = new WeatherService.WeatherServiceClient(channel);
        using var call = sClient.GetCurrentWeatherStream(
            new GetCurrentWeatherForCityRequest
            {
                City = "London",
                Units = GetCurrentWeatherForCityRequest.Types.Units.Imperial,
            }
        );

        while (await call.ResponseStream.MoveNext(default))
        {
            Console.WriteLine($"From Server: {call.ResponseStream.Current}");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
