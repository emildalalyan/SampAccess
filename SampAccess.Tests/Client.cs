using System;
using Xunit;
using System.Runtime.Versioning;

namespace SampAccess.Tests
{
    /// <summary>
    /// Class intended to testing <see cref="SampAccess.Client"/>
    /// </summary>
    [SupportedOSPlatform("windows")]
    public class Client
    {
        [Theory]
        [InlineData("Nick_Name")]
        public void TestClient(string Name)
        {
            SampAccess.Client.PlayerName = Name;

            if (SampAccess.Client.PlayerName.CompareTo(Name) != 0) throw new Exception();
        }
    }
}
