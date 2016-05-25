using FluentNHibernate.Mapping;

namespace Solutions.Tests.Queue.NHibernate
{
    class QueueItemMap : ClassMap<QueueItemDto>
    {
        public QueueItemMap()
        {
            Table("Queue");

            Id(x => x.Id).GeneratedBy.Identity();

            Map(x => x.Text).Not.Nullable();
            Map(x => x.HoldOn).Not.Nullable();

            Version(x => x.Version)
                .UnsavedValue(null)
                .Generated.Always();
        }
    }
}
