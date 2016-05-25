using System;
using Rhino.Mocks;
using Solutions.Core.DAL;
using Solutions.Core.Time;

namespace Solutions.Tests
{
    static class StubFactory
    {
        public static ITimeService GetCurrentProvider()
        {
            var current = MockRepository.GenerateStrictMock<ITimeService>();
            current.Stub(c => c.Now).Return(DateTime.Now).WhenCalled(x =>
            {
                x.ReturnValue = DateTime.Now;
            });
            return current;
        }

        public static IDbConfig GetDbConfig()
        {
            var dbConfig = MockRepository.GenerateStrictMock<IDbConfig>();
            dbConfig.Stub(c => c.ConnectionString)
                .Return(@"server=.\SQLEXPRESS;DataBase=Proxy;Integrated Security=true");

            return dbConfig;
        }
    }
}