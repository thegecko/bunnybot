using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using MSNPSharp;
using Q42.Wheels.Api.Nabaztag;

namespace org.theGecko.BunnyBot
{
	public class BunnyMessenger : MessengerWrapper
	{
		private readonly NabaztagApi _bunny;
		private readonly List<String> _voices = new List<string>();
		private bool _threadRunning;

		#region Properties

		public string Name
		{
			get
			{
				return _name;
			}
			set
			{
				_name = value;
			}
		}

		public string Message
		{
			get
			{
				return _message;
			}
			set
			{
				_message = value;
			}
		}

		public String[] Names
		{
			get; set;
		}

		public Dictionary<String, String> Templates
		{
			get; set;
		}

		public String DefaultVoice
		{
			get; set;
		}

		public String SleepMessage
		{
			get; set;
		}

		#endregion

		public BunnyMessenger(string serialId, string tokenId, string msnUsername, string msnPassword)
			: base(msnUsername, msnPassword)
		{
			_bunny = new NabaztagApi(serialId, tokenId);
			
			Name = _bunny.Name;
			DefaultVoice = "UK-Penelope";
			SleepMessage = "#bunnyname# is asleep";
			Names = new string[0];

			foreach (string language in _bunny.GetSupportedLanguages())
			{
				_voices.AddRange(_bunny.GetSupportedVoices(language));
			}

			Console.WriteLine("Loaded {0} voices", _voices.Count);
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
					Console.WriteLine("Error polling bunny: {0}", e.Message);
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
			string voice;

			foreach (string supportedVoice in _voices)
			{
				voice = "#" + supportedVoice.ToLower() + "#";
				if (message.Contains(voice))
				{
					message = message.Replace(voice, string.Empty);
					result = supportedVoice;
				}
			}

			Console.WriteLine("Voice set to: {0}", result);

			return result;
		}

		private string ParseMessage(string message)
		{
			CheckTemplates(ref message);
			CheckNames(ref message);
			CheckJoke(ref message);
			return message;
		}

		private void CheckTemplates(ref string message)
		{
			string template;
			foreach (string messageTemplate in Templates.Keys)
			{
				template = "#" + messageTemplate.ToLower() + "#";
				if (message.Contains(template))
				{
					string templateMessage = Templates[messageTemplate];
					message = message.Replace(template, templateMessage);
					Console.WriteLine("{0} detected: {1}", messageTemplate, message);
				}
			}
		}

		private void CheckNames(ref string message)
		{
			if (message.Contains("#randomname#"))
			{
				string[] randomNames = (Names.Length > 0) ? Names : GetAvailableContacts().ToArray();

				if (randomNames.Length > 0)
				{
					message = message.Replace("#randomname#", randomNames[new Random().Next(randomNames.Length)]);
				}

				Console.WriteLine("Random name detected: {0}", message);
			}

			if (message.Contains("#bunnyname#"))
			{
				message = message.Replace("#bunnyname#", Name);
				Console.WriteLine("Bunny name detected: {0}", message);
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
			if (message.Contains("#joke#"))
			{
				string jokeMessage = new Jokes.getJoke().CallgetJoke("6");
				message = message.Replace("#joke#", jokeMessage);
				Console.WriteLine("Joke message detected: {0}", message);
			}
		}

		//private void CheckQuote(ref string message)
		//{
		//    if (message.Contains("#quote#"))
		//    {
		//        string quoteMessage = new Jokes.getJoke().CallgetJoke("0");
		//        message = message.Replace("#quote#", quoteMessage);
		//        Console.WriteLine("Quote message detected: {0}", message);
		//    }
		//}
	}
}

// pluggable messaging system
// add logging
// more #random.. settings
// horroscopes/quotes
// #timer#
// wcf/web service versions

// set image?
// send it mp3 files to play
// send it radio urls to play?

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