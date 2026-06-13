using System.Collections.Generic;
using System.Diagnostics;

namespace FeatureGraph
{
	public class Logger<T>
	{
		public enum State { New, Running, Failed, Completed }
		public class LogEntry
		{
			public T context;
			public Stopwatch stopwatch;
			public string name;
			public string info;
			public State state;

			public string timestring
			{
				get
				{
					long time = stopwatch.ElapsedMilliseconds;
					int minutes = (int)(time / 60000);
					time -= 60000L * minutes;
					int seconds = (int)(time/1000);
					time -= 1000L * seconds;
					int millis = (int)(time);
					return $"<mspace=9>{minutes:00}:{seconds:00}:{millis:000}</mspace>";
				}
			}

			public LogEntry(T context1, string s)
			{
				context = context1;
				name = s;
				state = State.New;
				info = $"Scheduled {name}";
			}

			public void Begin()
			{
				stopwatch = Stopwatch.StartNew();
				state = State.Running;
				info = $"Started [{name}]";
			}

			public void Update()
			{
				if(stopwatch.IsRunning)
					info = $"Processing [{name}] {timestring}";
			}

			public void Finished()
			{
				stopwatch.Stop();
				state = State.Completed;
				info = $"Completed [{name}] in {timestring}";
			}

			public void Failed(string cause)
			{
				state = State.Failed;
				info = $"<color=red>Failed [{name}] after {timestring}\n({cause})</color>";
			}
		}

		private object _lock = new();
		private List<LogEntry> _privateEntries = new();

		public List<LogEntry> entries { get; } = new();

		public void Update()
		{
			lock (_lock)
			{
				for (int i = entries.Count; i < _privateEntries.Count; i++)
					entries.Add(_privateEntries[i]);

				if (_privateEntries.Count > 20)
				{
					_privateEntries.RemoveAt(0);
					entries.RemoveAt(0);
				}

				for (int i = 0; i < _privateEntries.Count; i++)
					_privateEntries[i].Update();
			}
		}

		public LogEntry NewEntry(T context, string name)
		{
			lock (_lock)
			{
				LogEntry entry = null; 
				if (_privateEntries.Count > 0)
				{
					LogEntry last = _privateEntries[_privateEntries.Count - 1];
					if ( (last.state==State.Completed || last.state==State.New) && Equals(last.context, context))
					{
						// Re-use repeated tasks
						entry = last;
						entry.state = State.New;
					}
				}

				if (entry == null)
				{
					entry = new LogEntry(context, name);
					_privateEntries.Add( entry );
				}
				return entry;
			}
		}

		public void Clear()
		{
			lock (_lock)
			{
				_privateEntries.Clear();
				entries.Clear();
			}
		}

		public LogEntry GetLongestRunning()
		{
			long time = 0;
			LogEntry longest = null;
			for (int i = 0; i < entries.Count; i++)
			{
				LogEntry entry = entries[i];
				long ms = entry.stopwatch.ElapsedMilliseconds;
				if (entry.state == State.Running && ms > time)
				{
					longest = entry;
					time = ms;
				}
			}
			return longest;
		}
	}
}