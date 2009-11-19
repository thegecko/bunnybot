using System;
using System.Threading;
using log4net;
using MSNPSharp;

namespace org.theGecko.BunnyBot
{
	public class MessengerWrapper : IDisposable
	{
        private static readonly ILog Log = LogManager.GetLogger(typeof(MessengerWrapper));
        protected readonly Messenger _messenger;

		protected string _name;
		protected string _message;

		#region Constructors

		public MessengerWrapper(string msnUsername, string msnPassword, string name, string message)
			: this(msnUsername, msnPassword)
		{
			_name = name;
			_message = message;
		}

		public MessengerWrapper(string msnUsername, string msnPassword)
		{
			_messenger = new Messenger
         		{
         			Nameserver = {AutoSynchronize = true},
         			Credentials = new Credentials(msnUsername, msnPassword, MsnProtocol.MSNP15)
         		};

			_messenger.NameserverProcessor.ConnectionEstablished += NameserverProcessorConnectionEstablished;
			_messenger.NameserverProcessor.ConnectionClosed += NameserverProcessorConnectionClosed;
			_messenger.Nameserver.Owner.ScreenNameChanged += OwnerScreenNameChanged;
			_messenger.Nameserver.Owner.PersonalMessageChanged += OwnerPersonalMessageChanged;
			// _messenger.Nameserver.Owner.DisplayImageChanged += Owner_DisplayImageChanged;
			_messenger.Nameserver.SignedIn += NameserverSignedIn;
			_messenger.Nameserver.SignedOff += NameserverSignedOff;
			_messenger.Nameserver.ContactService.ReverseAdded += ContactServiceReverseAdded;
			_messenger.Nameserver.ContactOnline += NameserverContactOnline;
			_messenger.Nameserver.ContactOffline += NameserverContactOffline;
			_messenger.Nameserver.ContactStatusChanged += NameserverContactStatusChanged;
			_messenger.ConversationCreated += MessengerConversationCreated;
			_messenger.OIMService.OIMReceived += OimServiceOimReceived;
			// _messenger.TransferInvitationReceived

			_messenger.NameserverProcessor.ConnectingException += MessengerException;
			_messenger.NameserverProcessor.ConnectionException += MessengerException;
			_messenger.NameserverProcessor.HandlerException += MessengerException;
			_messenger.Nameserver.ExceptionOccurred += MessengerException;
			_messenger.Nameserver.AuthenticationError += MessengerException;
			_messenger.Nameserver.ServerErrorReceived += MessengerError;
			_messenger.ContactService.ServiceOperationFailed += OperationFailed;
			_messenger.OIMService.ServiceOperationFailed += OperationFailed;
			_messenger.SpaceService.ServiceOperationFailed += OperationFailed;
			_messenger.StorageService.ServiceOperationFailed += OperationFailed;
		}

		#endregion

		public virtual void Start()
		{
			if (!_messenger.Connected)
			{
				_messenger.Connect();
			}
		}

		public virtual void Stop()
		{
			if (_messenger.Connected)
			{
				_messenger.Disconnect();
			}
		}

		#region Event Handlers

		protected virtual void MessengerException(object sender, ExceptionEventArgs e)
		{
		    Log.Error("Messenger exception", e.Exception);
		}

		protected virtual void MessengerError(object sender, MSNErrorEventArgs e)
		{
            Log.Error(string.Format("Messenger error: {0}", e.MSNError));
		}

		protected virtual void OperationFailed(object sender, ServiceOperationFailedEventArgs e)
		{
            Log.Warn("Operation failed", e.Exception);
		}

		protected virtual void NameserverProcessorConnectionEstablished(object sender, EventArgs e)
		{
            Log.Debug("Connection established");
		}

		protected virtual void NameserverProcessorConnectionClosed(object sender, EventArgs e)
		{
            Log.Debug("Connection closed");
		}

		protected virtual void OwnerScreenNameChanged(object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(_name))
			{
				_messenger.Owner.Name = _name;
                Log.Debug(string.Format("Name set to: {0}", _messenger.Owner.Name));
			}
		}

		protected virtual void OwnerPersonalMessageChanged(object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(_message))
			{
				_messenger.Owner.PersonalMessage.Message = _message;
                Log.Debug(string.Format("Message set to: {0}", _messenger.Owner.PersonalMessage.Message));
			}
		}

		protected virtual void NameserverSignedIn(object sender, EventArgs e)
		{
			_messenger.Owner.Status = PresenceStatus.Online;
            Log.Info(string.Format("Signed in as: {0}", _messenger.Owner.Name));
		}

		protected virtual void NameserverSignedOff(object sender, SignedOffEventArgs e)
		{
            Log.Info("Signed out");
		}

		protected virtual void ContactServiceReverseAdded(object sender, ContactEventArgs e)
		{
			_messenger.Nameserver.ContactService.AddNewContact(e.Contact.Mail);
            Log.Info(string.Format("{0} added to contacts", e.Contact.Mail));
		}

		protected virtual void NameserverContactOnline(object sender, ContactEventArgs e)
		{
            Log.Debug(string.Format("{0}: Online", e.Contact.Mail));
		}

		protected virtual void NameserverContactOffline(object sender, ContactEventArgs e)
		{
            Log.Debug(string.Format("{0}: Offline", e.Contact.Mail));
		}

		protected virtual void NameserverContactStatusChanged(object sender, ContactStatusChangeEventArgs e)
		{
            Log.Debug(string.Format("{0}: {1} -> {2}", e.Contact.Mail, e.OldStatus, e.Contact.Status));
		}

		protected virtual void MessengerConversationCreated(object sender, ConversationCreatedEventArgs e)
		{
			e.Conversation.Switchboard.TextMessageReceived += SwitchboardTextMessageReceived;
		}

		protected virtual void SwitchboardTextMessageReceived(object sender, TextMessageEventArgs e)
		{
            Log.Info(string.Format("{0}: {1}", e.Sender.Mail, e.Message.Text));
		}

		protected virtual void OimServiceOimReceived(object sender, OIMReceivedEventArgs e)
		{
            Log.Info(string.Format("{0}: {1}", e.Email, e.Message));
		}
	
		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			Stop();
            Log.Debug("Disposed");
			Thread.Sleep(500);
		}

		#endregion
	}
}
