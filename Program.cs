using DurableTask.Core;
using DurableTask.Emulator;
using Microsoft.Extensions.Logging;

// Create logger factory for diagnostics
using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    //builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// Create the in-memory orchestration service
var service = new LocalOrchestrationService();

// Create and configure the worker
var worker = new TaskHubWorker(service, loggerFactory);
worker.AddTaskOrchestrations(typeof(GreetingOrchestration));
worker.AddTaskActivities(typeof(GreetActivity));

// Start the worker
await worker.StartAsync();
Console.WriteLine("Worker started.");

// Create a client to start orchestrations
var client = new TaskHubClient(service, loggerFactory: loggerFactory);

// Start a new orchestration instance
var instance = await client.CreateOrchestrationInstanceAsync(
    typeof(GreetingOrchestration),
    "World");

Console.WriteLine($"Started orchestration: {instance.InstanceId}");

// Wait for completion
var result = await client.WaitForOrchestrationAsync(
    instance,
    TimeSpan.FromSeconds(30));

Console.WriteLine($"Result: {result.Output}");
Console.WriteLine($"Status: {result.OrchestrationStatus}");

// Stop the worker
await worker.StopAsync();