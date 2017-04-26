namespace Solutions.Core.IoC
{
    public interface IBuild
    {
        void Add(Registration registration);

        IScope Build();
    }
}