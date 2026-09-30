
using DurableTask.Core;
using DurableTask.Emulator;
using Microsoft.Extensions.Logging;

// Create logger factory for diagnostics
using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    // builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// Create the in-memory orchestration service
var service = new LocalOrchestrationService();

// Create and configure the worker
var worker = new TaskHubWorker(service, loggerFactory);

worker.AddTaskOrchestrations(
    typeof(GreetingOrchestration),
    typeof(ApprovalOrchestration)
);

worker.AddTaskActivities(typeof(GreetActivity));

// Start the worker
await worker.StartAsync();

Console.WriteLine("Worker started.");

// Create a client to start orchestrations
var client = new TaskHubClient(
    service,
    loggerFactory: loggerFactory
);


// -------------------------
// Greeting Orchestration
// -------------------------

var instance = await client.CreateOrchestrationInstanceAsync(
    typeof(GreetingOrchestration),
    "World");

Console.WriteLine(
    $"Started orchestration: {instance.InstanceId}");


// -------------------------
// Approval Orchestration
// -------------------------

var approvalInstance = await client.CreateOrchestrationInstanceAsync(
    typeof(ApprovalOrchestration),
    "Expense Approval");

Console.WriteLine(
    $"Started approval orchestration: {approvalInstance.InstanceId}");


// Send approval event
await client.RaiseEventAsync(
    approvalInstance,
    "ApprovalReceived",
    new ApprovalData
    {
        ApprovedBy = "John",
        Approved = true
    });


// -------------------------
// Wait for Greeting
// -------------------------

var result = await client.WaitForOrchestrationAsync(
    instance,
    TimeSpan.FromSeconds(30));

Console.WriteLine($"Result: {result.Output}");
Console.WriteLine($"Status: {result.OrchestrationStatus}");


// -------------------------
// Wait for Approval
// -------------------------

var approvalResult = await client.WaitForOrchestrationAsync(
    approvalInstance,
    TimeSpan.FromSeconds(30));

Console.WriteLine(
    $"Approval Result: {approvalResult.Output}");

Console.WriteLine(
    $"Approval Status: {approvalResult.OrchestrationStatus}");


// Stop the worker
await worker.StopAsync();

