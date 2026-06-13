using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Utility {
	public class Log {
		public readonly string Name;

		private Log(string name) {
			this.Name = name;
		}

		private static Dictionary<string, Log> logs = new Dictionary<string, Log>();

		public static Log Get(string name) {
			if (!logs.TryGetValue(name, out Log log)) {
				log = new Log(name);
				mod(log);
				logs[name] = log;
			}

			return log;
		}

		public bool DebugEnabled = true;

		public void Debug(params object[] args) {
			if (DebugEnabled) {
				UnityEngine.Debug.Log(getMessage("[D] " + Name + ": ", args));
			}
		}

		public void DebugFormat(string format, params object[] args) {
			if (DebugEnabled) {
				Debug(string.Format(format, args));
			}
		}

		public bool InfoEnabled = true;

		public void Info(params object[] args) {
			if (InfoEnabled) {
				UnityEngine.Debug.Log(getMessage("[I] " + Name + ": ", args));
			}
		}

		public void InfoFormat(string format, params object[] args) {
			if (InfoEnabled) {
				Info(string.Format(format, args));
			}
		}

		public bool WarnEnabled = true;

		public void Warn(params object[] args) {
			if (WarnEnabled) {
				UnityEngine.Debug.LogWarning(getMessage("[W] " + Name + ": ", args));
			}
		}

		public void WarnFormat(string format, params object[] args) {
			if (WarnEnabled) {
				Warn(string.Format(format, args));
			}
		}

		public bool ErrorEnabled = true;

		public void Error(params object[] args) {
			if (ErrorEnabled) {
				UnityEngine.Debug.LogError(getMessage("[E] " + Name + ": ", args));
			}
		}

		public void ErrorFormat(string format, params object[] args) {
			if (ErrorEnabled) {
				Error(string.Format(format, args));
			}
		}

		public void Exception(Exception e)
		{
			if (ErrorEnabled)
			{
				UnityEngine.Debug.LogException(e);
			}
		}

		private string getMessage(string prefix, object[] msg) {
			return prefix + string.Join(", ", Array.ConvertAll(msg, m => m == null ? "[NULL]" : m.ToString()));
		}

		private static Action<Log> mod = delegate(Log log) { };

		internal static void ModLog(Action<Log> mod) {
			Log.mod = mod;
			foreach (var kv in logs) {
				mod(kv.Value);
			}
		}

		public static Action<Log> Focus(params string[] filter) {
			return delegate(Log log) {
				var allow = false;
				foreach (var s in filter) {
					if (log.Name.StartsWith(s)) {
						allow = true;
						break;
					}
				}

				log.DebugEnabled = allow;
				log.InfoEnabled = allow;
				log.WarnEnabled = allow;
				log.ErrorEnabled = true;
			};
		}

		public static long ElapsedTime(long t0=0, string expl=null)
		{
			long t = Stopwatch.GetTimestamp();
			if(expl!=null)
				UnityEngine.Debug.Log($"{expl} in {1000*(t - t0)/Stopwatch.Frequency}ms");
			return t;
		}
	}
}