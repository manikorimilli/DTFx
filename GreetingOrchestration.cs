using DurableTask.Core;

public class GreetingOrchestration : TaskOrchestration<string, string>
{
    public override async Task<string> RunTask(OrchestrationContext context, string input)
    {
        // Call the GreetActivity
        string greeting = await context.ScheduleTask<string>(typeof(GreetActivity), input);
        return greeting;
    }
}