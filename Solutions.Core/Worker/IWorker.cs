using System.Threading.Tasks;

namespace Solutions.Core.Worker
{
    public interface IWorker
    {
        WorkerStatus Status { get; }
        Task Start();
        Task Stop();
    }
}