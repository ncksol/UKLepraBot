using System;
using System.Collections.Generic;
using Microsoft.Bot.Connector;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UKLepraBot.MessageAdapters;

namespace UKLepraBot.Tests
{
    [TestClass]
    public class MessageAdapterTests
    {
        private Mock<IConnectorClient> _connector;

        [TestInitialize]
        public void Setup()
        {
            _connector = new Mock<IConnectorClient>();
        }

        [TestMethod]
        public void TestDelayMessageAdapterCreateFromActivity()
        {
            var messageAdapterFactory = new MessageAdapterFactory(_connector.Object);
            var activity = new Activity();
            activity.Text = "/delay";
            var adapter = messageAdapterFactory.CreateAdapter(activity);
            Assert.IsInstanceOfType(adapter, typeof(DelayAdapter));
        }

        [TestMethod]
        public void TestStatusMessageAdapterCreateFromActivityWithStatusText()
        {
            var messageAdapterFactory = new MessageAdapterFactory(_connector.Object);
            var activity = new Activity();
            activity.Text = "/status";
            var adapter = messageAdapterFactory.CreateAdapter(activity);
            Assert.IsInstanceOfType(adapter, typeof(StatusAdapter));
        }

        [TestMethod]
        public void TestStatusMessageAdapterCreateFromActivityWithHuifyText()
        {
            var messageAdapterFactory = new MessageAdapterFactory(_connector.Object);
            var activity = new Activity();
            activity.Text = "/huify";
            var adapter = messageAdapterFactory.CreateAdapter(activity);
            Assert.IsInstanceOfType(adapter, typeof(StatusAdapter));
        }

        [TestMethod]
        public void TestStatusMessageAdapterCreateFromActivityWithUnhuifyText()
        {
            var messageAdapterFactory = new MessageAdapterFactory(_connector.Object);
            var activity = new Activity();
            activity.Text = "/unhuify";
            var adapter = messageAdapterFactory.CreateAdapter(activity);
            Assert.IsInstanceOfType(adapter, typeof(StatusAdapter));
        }

        [TestMethod]
        public void TestMiscMessageAdapterCreateFromActivity()
        {
            var messageAdapterFactory = new MessageAdapterFactory(_connector.Object);
            var activity = new Activity();
            activity.Text = "слава роботам";
            var adapter = messageAdapterFactory.CreateAdapter(activity);
            Assert.IsInstanceOfType(adapter, typeof(MiscAdapter));
        }

        [TestMethod]
        public void TestHuifyMessageAdapterCreateFromActivity()
        {
            var messageAdapterFactory = new MessageAdapterFactory(_connector.Object);
            var activity = new Activity();
            activity.Text = "сорок тысяч обезьян";
            var adapter = messageAdapterFactory.CreateAdapter(activity);
            Assert.IsInstanceOfType(adapter, typeof(HuifyAdapter));
        }

        [TestMethod]
        public void TestDelayMessageAdapterActivators()
        {
            List<string> testList = new List<string> {"/delay"};

            CollectionAssert.AreEquivalent(testList, MessageAdapterFactory.DelayAdapterActivators);
        }

        [TestMethod]
        public void TestStatusMessageAdapterActivators()
        {
            List<string> testList = new List<string> { "/status", "/huify", "/unhuify", "/uptime" };

            CollectionAssert.AreEquivalent(testList, MessageAdapterFactory.StatusAdapterActivators);
        }

        [TestMethod]
        public void TestMiscMessageAdapterActivators()
        {
            List<string> testList = new List<string> { "слава роботам", "брексит", "брекзит", "brexit" };

            CollectionAssert.AreEquivalent(testList, MessageAdapterFactory.MiscAdapterActivators);
        }

        [TestMethod]
        public void TestSlavaRobotamMessageIsProcessed()
        {
            var messageAdapterFactory = new MessageAdapterFactory(_connector.Object);
            var activity = new Activity();
            activity.Text = "слава роботам";
            var adapter = messageAdapterFactory.CreateAdapter(activity);

            var conversationMock = new Mock<IConversations>();
            
            conversationMock.Setup(x => x.ReplyToActivity(It.IsAny<Activity>())).Callback(delegate(Activity act)
            {
                var i = 0;
            });
            _connector.SetupGet(client => client.Conversations).Returns(conversationMock.Object);
            adapter.Process(activity);

        }
    }
}
