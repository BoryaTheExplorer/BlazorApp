namespace BlazorApp.Models
{
    public class WorkflowRunCheckResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public WorkflowRun? Run { get; set; }

        public static WorkflowRunCheckResult Success(WorkflowRun? run)
        {
            return new WorkflowRunCheckResult
            {
                IsSuccess = true,
                Run = run
            };
        }
        public static WorkflowRunCheckResult Fail(string message)
        {
            return new WorkflowRunCheckResult
            {
                IsSuccess = false,
                ErrorMessage = message
            };
        }
    }
}
