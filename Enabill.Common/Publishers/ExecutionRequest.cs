using System;

namespace Enabill.Publishers
{
	internal abstract class ExecutionRequest
	{
		public abstract void Dispatch();
	}

	internal class ExecutionRequest<T> : ExecutionRequest where T : ISink
	{
		public override void Dispatch() => this.DispatchAction(this.Sink);

		public Action<T> DispatchAction { get; set; }
		public T Sink { get; set; }
	}
}