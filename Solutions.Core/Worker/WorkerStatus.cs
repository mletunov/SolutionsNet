namespace Solutions.Core.Worker
{
    public enum WorkerStatus
    {
        Idle,
        Running,

        StartPending,
        StopPending,
        CancelPending,
    }
}