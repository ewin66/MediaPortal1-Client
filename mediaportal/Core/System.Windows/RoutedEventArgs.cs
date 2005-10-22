using System;

namespace System.Windows
{
	public class RoutedEventArgs : EventArgs
	{
		#region Constructors

		public RoutedEventArgs()
		{
		}

		public RoutedEventArgs(RoutedEvent routedEvent)
		{
			_routedEvent = routedEvent;
		}

		public RoutedEventArgs(RoutedEvent routedEvent, object source)
		{
			_routedEvent = routedEvent;
			_source = source;
			_originalSource = source;
		}

		#endregion Constructors

		#region Methods

		protected virtual void InvokeEventHandler(Delegate handler, object target)
		{
			// TODO: Find Eric Gunnerson's blogs on safely invoking events
			throw new NotImplementedException();
		}

		protected virtual void OnSetSource(object source)
		{
			if(_originalSource == null)
				_originalSource = source;

			_source = source;
		}

		#endregion Methods

		#region Properties

		public bool Handled
		{
			get { return _isHandled; }
			set { _isHandled = true; }
		}

		public object OriginalSource
		{
			get { return _originalSource; }
		}

		public RoutedEvent RoutedEvent
		{
			get { return _routedEvent; }
			set { _routedEvent = value; }
		}

		public object Source
		{
			get { return _source; }
			set { _source = value; }
		}

		#endregion Properties

		#region Fields

		bool						_isHandled;
		object						_originalSource;
		RoutedEvent					_routedEvent;
		object						_source;

		#endregion Fields
	}
}
