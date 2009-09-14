using System;
using System.ServiceModel;

namespace org.theGecko.Utilities
{
	public class WcfChannelFactoryWrapper<T> : IDisposable where T : class
	{
		private readonly ChannelFactory<T> _factory = null;
		public T Channel
		{
			get; internal set;
		}

		public WcfChannelFactoryWrapper(string endpointConfigurationName)
		{
			_factory = new ChannelFactory<T>(endpointConfigurationName);
			Channel = _factory.CreateChannel();
		}

		#region IDisposable Members

		public void Dispose()
		{
			if (Channel != null && Channel is IClientChannel)
			{
				IClientChannel clientChannel = (Channel as IClientChannel);
				try
				{
					if (clientChannel.State == CommunicationState.Faulted)
					{
						clientChannel.Abort();
					}
					else if (clientChannel.State != CommunicationState.Closed)
					{
						clientChannel.Close();
					}
				}
				catch (CommunicationException)
				{
					clientChannel.Abort();
				}
				catch (TimeoutException)
				{
					clientChannel.Abort();
				}
			}

			try
			{
				if (_factory.State == CommunicationState.Faulted)
				{
					_factory.Abort();
				}
				else if (_factory.State != CommunicationState.Closed)
				{
					_factory.Close();
				}
			}
			catch (CommunicationException)
			{
				_factory.Abort();
			}
			catch (TimeoutException)
			{
				_factory.Abort();
			}
		}

		#endregion
	}
}
