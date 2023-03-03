using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Enabill.Publishers
{
	public static class EventPublisher
	{
		private static IDictionary<Type, List<ISink>> _registeredDataSinks = new Dictionary<Type, List<ISink>>();
		private static Queue _dispatchQueue = Queue.Synchronized(new Queue());
		private static AutoResetEvent _queueEvent = new AutoResetEvent(false);

		public static void StartPublisher() => new Thread((object state) =>
		{
			Thread.CurrentThread.IsBackground = true; // allow application to kill thread easily

			ExecutionRequest req;

			while (true)
			{
				_queueEvent.WaitOne();
				while (_dispatchQueue.Count > 0 && (req = _dispatchQueue.Dequeue() as ExecutionRequest) != null)
					ProcessRequest(req);
			}
		}).Start();

		private static void ProcessRequest(ExecutionRequest request)
		{
			try
			{
				request.Dispatch();
			}
			catch (Exception)
			{
			}
		}

		public static void RegisterSink<TInfType>(TInfType sink) where TInfType : ISink
		{
			lock (_registeredDataSinks)
			{
				var type = typeof(TInfType);
				if (!_registeredDataSinks.ContainsKey(type))
					_registeredDataSinks.Add(type, new List<ISink>());
				else if (_registeredDataSinks[type].Contains(sink))
					return; // ignore if already registered

				_registeredDataSinks[type].Add(sink);
			}
		}

		public static void Dispatch<TInfType>(Action<TInfType> action) where TInfType : ISink
		{
			var type = typeof(TInfType);

			if (!_registeredDataSinks.ContainsKey(type))
				return;

			foreach (TInfType sink in _registeredDataSinks[type])
				_dispatchQueue.Enqueue(new ExecutionRequest<TInfType>() { DispatchAction = action, Sink = sink });

			// notify the processing thread to process the new queue items
			_queueEvent.Set();
		}
	}
}