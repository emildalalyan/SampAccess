using System;
using Xunit;

namespace SampAccess.Tests
{
    /// <summary>
    /// Class intended to testing <see cref="SampAccess.Query"/>
    /// </summary>
    public class Query
    {
        [Theory]
        [InlineData("193.70.94.12", 7777)]
        public void TestQuery(string IP, ushort Port)
        {
            using SampAccess.Query query = new(IP, Port);

            query.Initialize();
        }
    }
}