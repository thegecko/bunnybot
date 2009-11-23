using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using MSNPSharp;
using org.theGecko.Utilities;
using Q42.Wheels.Api.Nabaztag;
using log4net;

namespace org.theGecko.BunnyBot
{
    /// <summary>
    /// Ideas:
    /// ------
    /// pluggable messaging system
    /// more #random.. settings
    /// horroscopes
    /// #timer# for tea brewing
    /// wcf/web service versions
    /// set image
    /// send it mp3 files to play
    /// send it radio urls to play
    /// </summary>
	public class BunnyMessenger : MessengerWrapper
    {
        #region Variables

        private static readonly ILog Log = LogManager.GetLogger(typeof(BunnyMessenger));
        private readonly string[] _searchTemplates = new[] { "#{0}#", "#{0}" };
		private readonly NabaztagApi _bunny;
		private readonly List<String> _voices = new List<string>();
		private bool _threadRunning;

        #endregion

        #region Properties

        public String[] Names
        {
            get; set;
        }

        public override string MsnMessage
        {
            get
            {
                return SettingsUtil.Instance.GetSetting("MSNMessage", "Hi, I'm a bunny");
            }
        }

	    public string SleepMessage
	    {
            get
            {
                return SettingsUtil.Instance.GetSetting("SleepMessage", "#bunnyname# is asleep");
            }
	    }

        public String DefaultVoice
        {
            get
            {
                return SettingsUtil.Instance.GetSetting("DefaultVoice", "UK-Mistermuggles");
            }
        }

		public Dictionary<String, String> MessageTemplates
		{
			get
			{
                Dictionary<string, string> messageTemplates = new Dictionary<string, string>();

                string[] templates = SettingsUtil.Instance.GetSetting("MessageTemplates").Split(',');

                foreach (string template in templates)
                {
                    messageTemplates.Add(template, SettingsUtil.Instance.GetSetting(template + "Message"));
                }

			    return messageTemplates;
			}
		}

		#endregion

		public BunnyMessenger(string serialId, string tokenId, string msnUsername, string msnPassword) : base(msnUsername, msnPassword)
		{
			_bunny = new NabaztagApi(serialId, tokenId);			
			MsnName = _bunny.Name;
			
			Names = new string[0];

			foreach (string language in _bunny.GetSupportedLanguages())
			{
				_voices.AddRange(_bunny.GetSupportedVoices(language));
			}

            Log.Debug(string.Format("Loaded {0} voices", _voices.Count));
		}

		#region Overridden Event Handlers

		protected override void MessengerConversationCreated(object sender, ConversationCreatedEventArgs e)
		{
			if (_bunny.IsSleeping)
			{
				string message = ParseMessage(SleepMessage);
				e.Conversation.Switchboard.SendTextMessage(new TextMessage(message));
			}
			else
			{
				base.MessengerConversationCreated(sender, e);
			}
		}

		protected override void SwitchboardTextMessageReceived(object sender, TextMessageEventArgs e)
		{
			base.SwitchboardTextMessageReceived(sender, e);
			string message = ParseMessage(e.Message.Text.ToLower());
			SendMessage(message);
		}

		protected override void OimServiceOimReceived(object sender, OIMReceivedEventArgs e)
		{
			base.OimServiceOimReceived(sender, e);
			string message = ParseMessage(e.Message.ToLower());
			SendMessage(message);
		}

		#endregion

		#region Threading

		public void StartThread()
		{
			_threadRunning = true;

			while (_threadRunning)
			{
				try
				{
					if (_bunny.IsSleeping)
					{
						Stop();
					}
					else
					{
						Start();
					}
				}
				catch (Exception e)
				{
                    Log.Error("Error polling bunny", e);
				}

				Thread.Sleep(30000);
			}
		}

		public void StopThread()
		{
			_threadRunning = false;
			Stop();
		}

		#endregion

		public void SendMessage(string message)
		{
			string voice = GetVoice(ref message);
			_bunny.SendAction(new TextToSpeechAction(message, voice));
		}

		private string GetVoice(ref string message)
		{
			string result = DefaultVoice;

            foreach (string supportedVoice in _voices)
			{
                if (MessageContainsTemplate(message, supportedVoice))
                {
                    ReplaceMessageTemplate(ref message, supportedVoice, string.Empty);
					result = supportedVoice;
				}
			}

            Log.Debug(string.Format("Voice set to: {0}", result));

			return result;
		}

		private string ParseMessage(string message)
		{
			CheckTemplates(ref message);
			CheckNames(ref message);
			CheckJoke(ref message);
            CheckQuote(ref message);
            return message;
		}

		private void CheckTemplates(ref string message)
		{
			foreach (string messageTemplate in MessageTemplates.Keys)
			{
                if (MessageContainsTemplate(message, messageTemplate))
                {
                    ReplaceMessageTemplate(ref message, messageTemplate, MessageTemplates[messageTemplate]);
                    Log.Debug(string.Format("{0} detected: {1}", messageTemplate, message));
				}
			}
		}

		private void CheckNames(ref string message)
		{
            if (MessageContainsTemplate(message, "randomname"))
            {
                string[] randomNames = (Names.Length > 0) ? Names : GetAvailableContacts().ToArray();

                if (randomNames.Length > 0)
                {
                    ReplaceMessageTemplate(ref message, "randomname", randomNames[new Random().Next(randomNames.Length)]);
                    Log.Debug(string.Format("Random name detected: {0}", message));
                }                
            }

            if (MessageContainsTemplate(message, "bunnyname"))
            {
                ReplaceMessageTemplate(ref message, "bunnyname", MsnName);
                Log.Debug(string.Format("Bunny name detected: {0}", message));
            }
		}

		private List<string> GetAvailableContacts()
		{
			List<string> contacts = GetContacts(PresenceStatus.Online);
			contacts.AddRange(GetContacts(PresenceStatus.Away));
			contacts.AddRange(GetContacts(PresenceStatus.Busy));
			contacts.AddRange(GetContacts(PresenceStatus.Idle));

			return contacts;
		}

		private List<string> GetContacts(PresenceStatus status)
		{
			List<string> contacts = new List<string>();

			foreach (Contact contact in _messenger.ContactList.All)
			{
				if (contact.Status == status)
				{
					contacts.Add(Regex.Replace(contact.Name, @"[^A-Za-z0-9]", ""));
				}
			}

			return contacts;
 		}

        private void CheckJoke(ref string message)
        {
            // JOKES
            //Murphy's Laws - 7
            //Q&A - 3
            //Unnatural Laws - 18
            //Cool Jokes - 6
            //Blondes - 2
            //Random(contains Adult) - 1
            //Lawyers - 5
            //Headlines - 8
            //Military - 9
            //Accidents - 4
            //Excuses - 10
            //Answering machine - 11
            //All categories - 0

            if (MessageContainsTemplate(message, "joke"))
            {
                string jokeMessage = new Jokes.getJoke().CallgetJoke("6");
                ReplaceMessageTemplate(ref message, "joke", jokeMessage);
                Log.Debug(string.Format("Joke message detected: {0}", message));
            }
        }

        private void CheckQuote(ref string message)
        {
            if (MessageContainsTemplate(message, "quote"))
            {
                string quoteMessage = new Quote.QuoteofTheDay().GetQuote().QuoteOfTheDay;
                ReplaceMessageTemplate(ref message, "quote", quoteMessage);
                Log.Debug(string.Format("Quote message detected: {0}", message));
            }

            if (message.Contains("#quote#"))
            {
                string quoteMessage = new Jokes.getJoke().CallgetJoke("0");
                message = message.Replace("#quote#", quoteMessage);
                Log.Debug(string.Format("Quote message detected: {0}", message));
            }
        }

	    private bool MessageContainsTemplate(string message, string messageTemplate)
        {
            bool found = false;

            foreach (string template in _searchTemplates)
            {
                if (message.Contains(string.Format(template, messageTemplate.ToLower())))
                {
                    found = true;
                    break;
                }
            }

            return found;
        }

        private void ReplaceMessageTemplate(ref string message, string messageTemplate, string templateMessage)
        {
            foreach (string template in _searchTemplates)
            {
                message = message.Replace(string.Format(template, messageTemplate.ToLower()), templateMessage);
            }
        }
    }
}