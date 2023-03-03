using System;
using System.Data;

namespace Alacrity.DataAccess
{
	public interface IDbManager : IDisposable
	{
		IDbConnection Connection { get; }
	}
}