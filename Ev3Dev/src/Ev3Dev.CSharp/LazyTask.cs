using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Ev3Dev.CSharp
{
	/// <summary>
	/// Lazy task will start only if someone wants to wait it execution.
	/// Originally it is designed for waiting completion of motor asynchronous commands, which requires motor polling.
	/// In this case, polling won't be executed if waiting is not necessary.
	/// </summary>
	/// <remarks>
	/// You can use all ContinueWith methods of this class, 
	/// but in this case you must call <see cref="Task.Start( )"/> manually.
	/// </remarks>
	public class LazyTask : Task
	{
		public LazyTask( Action action ) : base( action )
		{
			
		}

		/// <summary>
		/// Waits for this <see cref="LazyTask"/> to complete execution. 
		/// Starts the execution if it hasn't been started already.
		/// </summary>
		public new void Wait( )
		{
			if ( Status == TaskStatus.Created )
			{ Start( ); }
			base.Wait( );
		}

		public new bool IsCanceled
		{
			get
			{
				if ( Status == TaskStatus.Created )
				{ Start( ); }
				return base.IsCanceled;
			}
		}

		public new bool IsCompleted
		{
			get
			{
				if ( Status == TaskStatus.Created )
				{ Start( ); }
				return base.IsCompleted;
			}
		}

		public new bool IsFaulted
		{
			get
			{
				if ( Status == TaskStatus.Created )
				{ Start( ); }
				return base.IsFaulted;
			}
		}

		/// <summary>
		/// Gets an awaiter used to await this <see cref="LazyTask"/>. 
		/// Starts the execution if it hasn't been started already.
		/// </summary>
		public new TaskAwaiter GetAwaiter( )
		{
			if ( Status == TaskStatus.Created )
			{ Start( ); }
			return base.GetAwaiter( );
		}
	}
}
