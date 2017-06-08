using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using MagicMirror.ThirdParty;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using System.Collections.Generic;

namespace TwitterAPITests
{
    [TestClass]
    public class TwitterTests
    {
        [TestMethod]
        public void TestGetBearerToken()
        {
            TwitterAPI api = new TwitterAPI("", "");

            string response = api.GetBearerToken().Result.access_token ;

            Assert.IsFalse(string.IsNullOrEmpty(response));
        }

        [TestMethod]
        public void TestGetFeedRaw()
        {
            TwitterAPI api = new TwitterAPI("", "");

            List<Tweet> response = api.GetTrumpsFeed().Result;

            Assert.IsNotNull(response);
        }
    }
}
