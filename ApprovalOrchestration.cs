using DurableTask.Core;

public class ApprovalOrchestration
    : TaskOrchestration<ApprovalData, string, ApprovalData, string>
{
    private TaskCompletionSource<ApprovalData> approvalHandle;

    public override async Task<ApprovalData> RunTask(
        OrchestrationContext context,
        string input)
    {
        // Create something that will wait for the approval
        this.approvalHandle = new TaskCompletionSource<ApprovalData>();

        // Get the task that represents "waiting for approval"
        var eventTask = this.approvalHandle.Task;

        // Create a 1-day timer
        using var cts = new CancellationTokenSource();

        var timerTask = context.CreateTimer(
            context.CurrentUtcDateTime.AddDays(1),
            true,
            cts.Token);

        // Wait for either:
        // 1. Approval event
        // 2. 1-day timer
        var winner = await Task.WhenAny(timerTask, eventTask);

        // Approval arrived first
        if (winner == eventTask)
        {
            // We don't need the timer anymore
            cts.Cancel();

            // Return the approval information
            return await eventTask;
        }

        // 1 day passed without approval
        Console.WriteLine("Approval timed out.");

        return default;
    }

    public override void OnEvent(
        OrchestrationContext context,
        string name,
        ApprovalData input)
    {
        if (name == "ApprovalReceived")
        {
            // Tell the waiting workflow:
            // "The approval has arrived!"
            this.approvalHandle?.TrySetResult(input);
        }
    }
}