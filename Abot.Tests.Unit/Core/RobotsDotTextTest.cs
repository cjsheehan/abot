﻿using Abot.Core;
using Abot.Poco;
using Moq;
using NUnit.Framework;
using System;

namespace Abot.Tests.Unit.Core
{
    [TestFixture]
    public class RobotsDotTextTest
    {
        private RobotsDotText _unitUnderTest;
        private Uri _rootUri = new Uri("http://www.spidertestsite1.com/");
        CrawledPage _realPage;
        private string _userAgentString = "Some User Agent...";
        private string _robotsContent = @"
User-Agent: *
Disallow: /disallowedfile.txt
Disallow: /disallowedfolder
Disallow: /disallowedfolder/subfolder
Crawl-Delay: 20

User-Agent: badagent
Disallow: /

User-Agent: userAgentCrawlDelayIs1
Crawl-Delay: 1

User-Agent: userAgentCrawlDelayNotSpecified
Allow: /

User-Agent: userAgentCrawlDelayEmpty
Crawl-Delay: 

Sitemap: http://a.com/sitemap.xml
Sitemap: http://b.com/sitemap.xml
";

        [SetUp]
        public void SetUp()
        {
            _unitUnderTest = new RobotsDotText(_rootUri, _robotsContent);
            _realPage = new PageRequester(new CrawlConfiguration{ UserAgentString = "aaa" }).MakeRequest(new Uri("http://localhost:1111/"));
        }

        [Test]
        public void Constructor()
        {
            Assert.IsNotNull(_unitUnderTest);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullRootUri()
        {
            _unitUnderTest = new RobotsDotText(null, _robotsContent);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullContent()
        {
            string nullContent = null;
            _unitUnderTest = new RobotsDotText(_rootUri, nullContent);
        }

        [Test]
        public void IsUrlAllowed_AllowedPages_ReturnsTrue()
        {
            string userAgentString = _userAgentString;
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri, userAgentString));
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri + "allowedfolder/aa.html", userAgentString));
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri + "allowedfolder/bb.html", userAgentString));
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri + "allowedfile2", userAgentString));

            //User agent "userAgentCrawlDelayIs1" doesn't specify anything to disallow so should allow all ("*" is not inherited)
            userAgentString = "userAgentCrawlDelayIs1";
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri, userAgentString));
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri + "disallowedfile.txt", userAgentString));
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri + "disallowedfolder", userAgentString));
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri + "disallowedfolder/", userAgentString));
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri + "disallowedfolder/subfolder", userAgentString));
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri + "disallowedfolder/subfolder/", userAgentString));

            //Allows all since "userAgentCrawlDelayIs1" does not specify allow or disallow
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri, userAgentString));
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri + "allowedfolder/aa.html", userAgentString));
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri + "allowedfolder/bb.html", userAgentString));
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri + "allowedfile2", userAgentString));
        }

        [Test]
        public void IsUrlAllowed_DisAllowedPages_ReturnsFalse()
        {
            //Should use "*" user agent by default
            string userAgentString = _userAgentString;
            Assert.IsFalse(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri + "disallowedfile.txt", userAgentString));
            Assert.IsFalse(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri + "disallowedfolder", userAgentString));
            Assert.IsFalse(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri + "disallowedfolder/", userAgentString));
            Assert.IsFalse(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri + "disallowedfolder/subfolder", userAgentString));
            Assert.IsFalse(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri + "disallowedfolder/subfolder/", userAgentString));

            //Disallows all for "badagent"
            userAgentString = "badagent";
            Assert.IsFalse(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri, userAgentString));
            Assert.IsFalse(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri + "allowedfolder/aa.html", userAgentString));
            Assert.IsFalse(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri + "allowedfolder/bb.html", userAgentString));
            Assert.IsFalse(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri + "allowedfile2", userAgentString));
        }

        [Test]
        public void IsUrlAllowed_EmptyRobotsContent_ReturnsTrue()
        {
            _unitUnderTest = new RobotsDotText(_rootUri, "");

            //Should use "*" user agent by default
            string userAgent = _userAgentString;
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri, userAgent));
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri + "allowedfolder/aa.html", userAgent));
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri + "allowedfolder/bb.html", userAgent));
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri + "allowedfile2", userAgent));

            //User agent "userAgentCrawlDelayIs1" doesn't specify anything to disallow so should allow all ("*" is not inherited)
            userAgent = "userAgentCrawlDelayIs1";
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri, userAgent));
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri + "disallowedfile.txt", userAgent));
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri + "disallowedfolder", userAgent));
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri + "disallowedfolder/", userAgent));
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri + "disallowedfolder/subfolder", userAgent));
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri + "disallowedfolder/subfolder/", userAgent));

            //Allows all since "userAgentCrawlDelayIs1" does not specify allow or disallow
            userAgent = "userAgentCrawlDelayIs1";
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri, userAgent));
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri + "allowedfolder/aa.html", userAgent));
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri + "allowedfolder/bb.html", userAgent));
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri + "allowedfile2", userAgent));
        }

        [Test]
        public void IsUrlAllowed_ExternalPages_ReturnsTrue()
        {
            Uri externalUri = new Uri("http://yahoo.com/");
            string userAgentString = _userAgentString;
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(externalUri.AbsoluteUri, userAgentString));
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(externalUri.AbsoluteUri + "allowedfolder/aa.html", userAgentString));
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(externalUri.AbsoluteUri + "allowedfolder/bb.html", userAgentString));
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(externalUri.AbsoluteUri + "allowedfile2", userAgentString));

            //User agent "userAgentCrawlDelayIs1" doesn't specify anything to disallow so should allow all ("*" is not inherited)
            userAgentString = "userAgentCrawlDelayIs1";
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(externalUri.AbsoluteUri, userAgentString));
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(externalUri.AbsoluteUri + "disallowedfile.txt", userAgentString));
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(externalUri.AbsoluteUri + "disallowedfolder", userAgentString));
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(externalUri.AbsoluteUri + "disallowedfolder/", userAgentString));
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(externalUri.AbsoluteUri + "disallowedfolder/subfolder", userAgentString));
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(externalUri.AbsoluteUri + "disallowedfolder/subfolder/", userAgentString));

            //Allows all since "userAgentCrawlDelayIs1" does not specify allow or disallow
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(externalUri.AbsoluteUri, userAgentString));
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(externalUri.AbsoluteUri + "allowedfolder/aa.html", userAgentString));
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(externalUri.AbsoluteUri + "allowedfolder/bb.html", userAgentString));
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(externalUri.AbsoluteUri + "allowedfile2", userAgentString));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void IsUrlAllowed_NullRobotsContent()
        {
            new RobotsDotText(_rootUri, null);
        }

        [Test]
        public void IsUserAgentAllowed_NullUserAgent_ReturnsTrue()
        {
            Assert.IsTrue(_unitUnderTest.IsUserAgentAllowed(null));
        }

        [Test]
        public void IsUserAgentAllowed_EmptyUserAgent_ReturnsTrue()
        {
            Assert.IsTrue(_unitUnderTest.IsUserAgentAllowed(""));
        }

        [Test]
        public void IsUserAgentAllowed_WildCardUserAgent_ReturnsTrue()
        {
            string content = @"
User-Agent: *
Allow: /";
            _unitUnderTest = new RobotsDotText(_rootUri, content);

            Assert.IsTrue(_unitUnderTest.IsUserAgentAllowed("*"));
        }

        [Test]
        public void IsUserAgentAllowed_WildCardUserAgent_ReturnsFalse()
        {
            string content = @"
User-Agent: *
Disallow: /";
            _unitUnderTest = new RobotsDotText(_rootUri, content);

            Assert.IsFalse(_unitUnderTest.IsUserAgentAllowed("aaaaaaaaaaaa"));
        }

        [Test]
        public void IsUserAgentAllowed_UserAgentNotAllowed_ReturnsFalse()
        {
            Assert.IsFalse(_unitUnderTest.IsUserAgentAllowed("badagent"));
        }

        [Test]
        public void IsUrlAllowed_WildCardAgentWithEmptyDisallow_ReturnsTrue()
        {
            string userAgentString = _userAgentString;
            _unitUnderTest = new RobotsDotText(_rootUri, @"User-agent: *
Disallow:");
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri, userAgentString));
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri + "aa.html", userAgentString));
        }

        [Test]
        public void IsUrlAllowed_WildCardAgentWithWhiteSpaceDisallow_ReturnsTrue()
        {
            string userAgentString = _userAgentString;
            _unitUnderTest = new RobotsDotText(_rootUri, @"User-agent: *
Disallow: ");
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri, userAgentString));
            Assert.IsTrue(_unitUnderTest.IsUrlAllowed(_rootUri.AbsoluteUri + "aa.html", userAgentString));
        }

        [Test]
        public void GetCrawlDelay_ValueExists_ReturnsValue()
        {
            Assert.AreEqual(20, _unitUnderTest.GetCrawlDelay(_userAgentString));
            Assert.AreEqual(1, _unitUnderTest.GetCrawlDelay("userAgentCrawlDelayIs1"));
        }

        [Test]
        public void GetCrawlDelay_ValueDoesNotExists_ReturnsZero()
        {
            Assert.AreEqual(0, _unitUnderTest.GetCrawlDelay("badagent"));
            Assert.AreEqual(0, _unitUnderTest.GetCrawlDelay("userAgentCrawlDelayNotSpecified"));
            Assert.AreEqual(0, _unitUnderTest.GetCrawlDelay("userAgentCrawlDelayEmpty"));
        }
    }
}
