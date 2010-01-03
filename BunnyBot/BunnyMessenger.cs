using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using log4net;
using MSNPSharp;
using org.theGecko.Utilities;
using Q42.Wheels.Api.Nabaztag;

namespace org.theGecko.BunnyBot
{
	public class BunnyMessenger : MessengerWrapper
    {
        #region Variables

        private static readonly ILog Log = LogManager.GetLogger(typeof(BunnyMessenger));
        private readonly string[] _searchTemplates = new[] { "#{0}#", "#{0}" };
        private readonly NabaztagApiExtension _bunny;
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
                string message = SettingsUtil.Instance.GetSetting("MSNMessage", "Hi, I'm #bunnyname#");
                ParseMessage(ref message);
                return message;
            }
        }

	    public string SleepMessage
	    {
            get
            {
                string message = SettingsUtil.Instance.GetSetting("SleepMessage", "#bunnyname# is asleep");
                ParseMessage(ref message);
                return message;
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
            _bunny = new NabaztagApiExtension(serialId, tokenId);			
			MsnName = _bunny.Name;
			
			Names = new string[0];

			foreach (string language in _bunny.GetSupportedLanguages())
			{
				_voices.AddRange(_bunny.GetSupportedVoices(language));
			}

            Log.Info(string.Format("Loaded {0} voices", _voices.Count));
		}

		protected override void SwitchboardTextMessageReceived(object sender, TextMessageEventArgs e)
		{
            base.SwitchboardTextMessageReceived(sender, e);

            if (sender is SBMessageHandler)
            {
                SBMessageHandler switchboard = sender as SBMessageHandler;

                if (_bunny.IsSleeping)
                {
                    SendMsnMessage(switchboard, SleepMessage);
                }
                else
                {
                    ParseAndSendMessage(switchboard, e.Message.Text);
                }
            }
		}

        /// <summary>
        /// Parses a message and plays it on bunny
        /// </summary>
        /// <param name="message">Message to parse and play</param>
        public void ParseAndSendMessage(SBMessageHandler switchboard, string message)
        {
            message = message.ToLower();
            ParseMessage(ref message);
            int delay = GetDelay(ref message);
            message = message.Trim();

            if (delay > 0)
            {
                SendMsnMessage(switchboard, string.Format("In {0} minutes: {1}", delay, message));
            }
            else
            {
                SendMsnMessage(switchboard, message);
            }

            // Check for a url
            if (message.StartsWith("http://"))
            {
                SendBunnyUrl(message, delay);
            }
            else
            {
                SendBunnyMessage(message, delay);
            }
        }

        /// <summary>
        /// Speaks on MSN
        /// </summary>
        /// <param name="switchboard">MSN conversation switchboard</param>
        /// <param name="message">Message to send</param>
        private void SendMsnMessage(SBMessageHandler switchboard, string message)
        {
            switchboard.SendTextMessage(new TextMessage(message));
            Log.Info(string.Format("msn: {0}", message));
        }

        /// <summary>
        /// Plays a message on bunny
        /// </summary>
        /// <param name="message">Message to play</param>
        /// <param name="delay">Delay in minutes</param>
        public void SendBunnyMessage(string message, int delay)
		{
            if (delay > 0)
            {
                DelegateUtil.SetTimeout(() => SendBunnyMessage(message), delay * 1000 * 60);
            }
            else
            {
                SendBunnyMessage(message);
            }
		}

        /// <summary>
        /// Plays a message on bunny
        /// </summary>
        /// <param name="message">Message to play</param>
        private void SendBunnyMessage(string message)
        {
            string voice = GetVoice(ref message);
            _bunny.SendAction(new TextToSpeechAction(message, voice));
            Log.Info(string.Format("{0}: {1}", MsnName, message));
        }

        /// <summary>
        /// Tells bunny to stream some music
        /// </summary>
        /// <param name="message">Url to play</param>
        /// <param name="delay">Delay in minutes</param>
        public void SendBunnyUrl(string url, int delay)
        {
            if (delay > 0)
            {
                DelegateUtil.SetTimeout(() => SendBunnyUrl(url), delay * 1000 * 60);
            }
            else
            {
                SendBunnyUrl(url);
            }
        }

        /// <summary>
        /// Tells bunny to stream some music
        /// </summary>
        /// <param name="message">Url to play</param>
        private void SendBunnyUrl(string url)
        {
            _bunny.SendStreamAction(new UrlListAction(url));
            Log.Info(string.Format("url: {0}", url));
        }

        #region Parse Checks

        private void ParseMessage(ref string message)
        {
            CheckTemplates(ref message);
            CheckNames(ref message);
            CheckJoke(ref message);
            CheckQuote(ref message);
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
        }

        #endregion

        #region Parse Gets
       
        private int GetDelay(ref string message)
        {
            string timerMessage;

            // This is a bit hacky, move it to a regex
            for (int i = 1; i <= 30; i++)
            {
                timerMessage = string.Format("delay({0})", i);
                if (MessageContainsTemplate(message, timerMessage))
                {
                    ReplaceMessageTemplate(ref message, timerMessage, string.Empty);
                    Log.Debug(string.Format("Delay detected: {0}", message));

                    return i;
                }
            }

            return 0;
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

        #endregion

        #region Helper Methods

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

        #endregion

        #region IService Members

        public override void Start()
        {
            _threadRunning = true;

            while (_threadRunning)
            {
                try
                {
                    if (_bunny.IsSleeping)
                    {
                        base.Stop();
                    }
                    else
                    {
                        base.Start();
                    }
                }
                catch (Exception e)
                {
                    Log.Error("Error polling bunny", e);
                }

                Thread.Sleep(30000);
            }
        }

        public override void Stop()
        {
            _threadRunning = false;
            base.Stop();
        }

        #endregion
    }
}