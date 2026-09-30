using DurableTask.Core;

public class GreetActivity : TaskActivity<string, string>
{
    protected override string Execute(TaskContext context, string name)
    {
        return $"Hello, {name}!";
    }
}