using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Undo
{
	public class UndoManager
	{
		public interface IUndoable
		{
			public void Redo();
			public void Undo();
		}

		public class Undoable<T> : IUndoable
		{
			public Undoable(Func<T> redo, Action<T> undo)
			{
				_redo = redo;
				_undo = undo;
			}

			private T _context;
			private Func<T> _redo;
			private Action<T> _undo;

			public void Redo()
			{
				_context = _redo();
			}

			public void Undo()
			{
				_undo(_context);
			}

			public override string ToString()
			{
				return $"Action: {_redo}, Undo: {_undo}, Context: {_context}";
			}
		}

		private static List<IUndoable> _undoStack = new();
		private static int _undoIndex = 0;
		private static bool _saved;
		private static int _savedIndex;
		public static void Append<T>(Func<T> redo, Action<T> undo)
		{
			Append(new Undoable<T>(redo, undo));
		}

		public static void Append(Action redo, Action undo)
		{
			Append( new Undoable<object>(() => { redo(); return null; }, o => { undo(); }) );
		}
		
		public static void Append(IUndoable undoable)
		{
			_undoStack.Insert(_undoIndex, undoable );
			_undoIndex++;
			while (_undoIndex < _undoStack.Count)
				_undoStack.RemoveAt(_undoIndex);
			undoable.Redo();
			_saved = false;
			DumpStack();
		}

		public static void Undo()
		{
			if (_undoIndex > 0)
			{
				_undoStack[--_undoIndex].Undo();
			}
			DumpStack();
		}

		public static void Redo()
		{
			if (_undoIndex < _undoStack.Count)
			{
				_undoStack[_undoIndex++].Redo();
			}
			DumpStack();
		}

		public static void Clear()
		{
			_undoStack.Clear();
			_undoIndex = 0;
			DumpStack();
		}

		public static bool CanUndo()
		{
			return _undoIndex > 0;
		}

		public static void MarkSaved()
		{
			_saved = true;
			_savedIndex = _undoIndex;
			DumpStack();
		}
		
		public static bool HasChanges()
		{
			return !_saved || _savedIndex != _undoIndex;
		}

		private static void DumpStack()
		{
			StringBuilder sb = new StringBuilder($"Saved: {_saved}... Currently at Index {_undoIndex} / {_undoStack.Count}");
			foreach (IUndoable undoable in _undoStack)
			{
				sb.AppendLine(undoable.ToString());
			}
			Debug.Log(sb.ToString());
		}
	}
}