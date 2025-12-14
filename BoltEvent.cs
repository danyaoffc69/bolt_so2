using System;
using System.Collections.Generic;
using Axlebolt.RpcSupport;

namespace Axlebolt.Bolt
{
	public class BoltEvent<T>
	{
		private readonly List<Action<T>> _listeners;

		public BoltEvent()
		{
			_listeners = new List<Action<T>>();
		}

		internal void Invoke(T arg)
		{
			BoltPlayerApi.Instance.ExecuteInMainThread(delegate
			{
				Action<T>[] array = _listeners.ToArray();
				foreach (Action<T> action in array)
				{
					try
					{
						action(arg);
					}
					catch (Exception arg2)
					{
						Logger.Error($"EventListener ({action}) failed. {arg2}");
					}
				}
			});
		}

		public void AddListener(Action<T> listener)
		{
			if (_listeners.Contains(listener))
			{
				Logger.Error($"EventListener ({listener}) already added");
			}
			else
			{
				_listeners.Add(listener);
			}
		}

		public void RemoveListener(Action<T> listener)
		{
			_listeners.Remove(listener);
		}
	}
	public class BoltEvent
	{
		private readonly List<Action> _listeners;

		public BoltEvent()
		{
			_listeners = new List<Action>();
		}

		internal void Invoke()
		{
			BoltPlayerApi.Instance.ExecuteInMainThread(delegate
			{
				Action[] array = _listeners.ToArray();
				foreach (Action action in array)
				{
					try
					{
						action();
					}
					catch (Exception arg)
					{
						Logger.Error($"EventListener ({action}) failed. {arg}");
					}
				}
			});
		}

		public void AddListener(Action listener)
		{
			if (_listeners.Contains(listener))
			{
				Logger.Error($"EventListener ({listener}) already added");
			}
			else
			{
				_listeners.Add(listener);
			}
		}

		public void RemoveListener(Action listener)
		{
			_listeners.Remove(listener);
		}
	}
}
