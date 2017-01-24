using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Bot.Connector;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UKLepraBot.MessageAdapters;

namespace UKLepraBot.Tests
{
    [TestClass]
    public class MessageAdapterTests
    {
        private Mock<ConnectorClient> _connector;

        [TestInitialize]
        public void Setup()
        {
            _connector = new Mock<ConnectorClient>();
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
            Assert.IsInstanceOfType(adapter, typeof(CommandAdapter));
        }

        [TestMethod]
        public void TestStatusMessageAdapterCreateFromActivityWithHuifyText()
        {
            var messageAdapterFactory = new MessageAdapterFactory(_connector.Object);
            var activity = new Activity();
            activity.Text = "/huify";
            var adapter = messageAdapterFactory.CreateAdapter(activity);
            Assert.IsInstanceOfType(adapter, typeof(CommandAdapter));
        }

        [TestMethod]
        public void TestStatusMessageAdapterCreateFromActivityWithUnhuifyText()
        {
            var messageAdapterFactory = new MessageAdapterFactory(_connector.Object);
            var activity = new Activity();
            activity.Text = "/unhuify";
            var adapter = messageAdapterFactory.CreateAdapter(activity);
            Assert.IsInstanceOfType(adapter, typeof(CommandAdapter));
        }

        [TestMethod]
        public void TestMiscMessageAdapterCreateFromActivity()
        {
            var messageAdapterFactory = new MessageAdapterFactory(_connector.Object);
            var activity = new Activity();
            activity.Text = "слава роботам";
            var adapter = messageAdapterFactory.CreateAdapter(activity);
            Assert.IsInstanceOfType(adapter, typeof(MessageAdapter));
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
            var testList = new List<string> {"/delay"};

            CollectionAssert.AreEquivalent(testList, MessageAdapterFactory.DelayAdapterActivators);
        }

        [TestMethod]
        public void TestStatusMessageAdapterActivators()
        {
            var testList = new List<string> {"/status", "/huify", "/unhuify", "/uptime"};

            CollectionAssert.AreEquivalent(testList, MessageAdapterFactory.CommandAdapterActivators);
        }

        [TestMethod]
        public void TestMiscMessageAdapterActivators()
        {
            var testList = new List<string> {"слава роботам", "брексит", "брекзит", "brexit"};

            CollectionAssert.AreEquivalent(testList, MessageAdapterFactory.MiscAdapterActivators);
        }

        [TestMethod]
        public void TestSlavaRobotamMessageIsProcessed()
        {
            var messageAdapterFactory = new MessageAdapterFactory(_connector.Object);
            var activity = CreateActivity();
            activity.Text = "слава роботам";
            var adapter = messageAdapterFactory.CreateAdapter(activity);

            var conversationMock = new Mock<IConversations>();

            Activity replyActivity = null;

            conversationMock
                .Setup(x => x.ReplyToActivityWithHttpMessagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Activity>(), It.IsAny<Dictionary<string, List<string>>>(), It.IsAny<CancellationToken>()))
                .Callback(delegate(string conversationId, string activityId, Activity act, Dictionary<string, List<string>> arg4, CancellationToken token)
                {
                    replyActivity = act;
                    var i = 0;
                });

            _connector.SetupGet(client => client.Conversations).Returns(conversationMock.Object);
            adapter.Process(activity);

            if (replyActivity.ChannelData != null)
            {
                var channelData = (ChannelData) replyActivity.ChannelData;
                Assert.AreEqual(channelData.method, "sendSticker");
                Assert.AreEqual(channelData.parameters.sticker, "BQADBAADHQADmDVxAh2h6gc7L-sLAg");
            }
            else
            {
                Assert.AreEqual(replyActivity.Text, "Воистину слава!");
            }
        }

        private Activity CreateActivity()
        {
            var activity = new Activity();
            activity.Recipient = new ChannelAccount("111", "aaa");
            activity.From = new ChannelAccount("222", "bbb");

            activity.Type = "message";
            DateTime? nullable = DateTime.UtcNow;
            activity.Timestamp = nullable;
            var channelAccount1 = new ChannelAccount(activity.Recipient.Id, activity.Recipient.Name);
            activity.From = channelAccount1;
            var channelAccount2 = new ChannelAccount(activity.From.Id, activity.From.Name);
            activity.Recipient = channelAccount2;
            activity.Id = "444";
            activity.ReplyToId = "333";
            string serviceUrl = "https://telegram.botframework.com";
            activity.ServiceUrl = serviceUrl;
            string channelId = "777";
            activity.ChannelId = channelId;
            activity.Conversation = new ConversationAccount(false, "123123123", "zzz");
            var attachmentList = new List<Attachment>();
            activity.Attachments = attachmentList;
            var entityList = new List<Entity>();
            activity.Entities = entityList;
            return activity;
        }
    }
}