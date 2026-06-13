using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Utility
{
  public class WorkerThread : MonoBehaviour
  {
    private bool _running;

    private Queue<Action> _backgroundJobs = new Queue<Action>();
    private Queue<Action> _foregroundJobs = new Queue<Action>();
    private Thread _mainThread;

    public int backgroundJobs
    {
	    get
	    {
		    lock (_backgroundJobs)
		    {
			    return _backgroundJobs.Count;
		    }
	    }
    }

    public int foregroundJobs
    {
	    get
	    {
		    lock (_foregroundJobs)
		    {
			    return _foregroundJobs.Count;
		    }
	    }
    }

    private void Start()
    {
      if (!_running)
      {
        _mainThread = Thread.CurrentThread;
        _running = true;
        for (int i = 0; i < 4; i++)
        {
          Thread thread = new Thread(() =>
          {
            while (_running)
            {
              Action job = null;
              lock (_backgroundJobs)
              {
                if(_backgroundJobs.Count==0)
                  Monitor.Wait(_backgroundJobs);
                else
                  job = _backgroundJobs.Dequeue();
              }
              if (job != null)
              {
                RunJob(job);
              }
            }
          });
          thread.Name = $"Worker_{i}";
          thread.Start();
        }
        
        StartCoroutine(MainCoroutine());
      }
    }

    private void OnDestroy()
    {
      _running = false;
    }

    IEnumerator MainCoroutine()
    {
      while (_running)
      {
        Action job = null;
        lock (_foregroundJobs)
        {
          if (_foregroundJobs.Count > 0)
            job = _foregroundJobs.Dequeue();
        }
        if (job != null)
        {
          RunJob(job);
        }
        else
          yield return null;
      }
    }

    public void RunInBackground(Action backgroundjob)
    {
      if (Thread.CurrentThread != _mainThread)
        RunJob(backgroundjob);
      else
      {
        lock (_backgroundJobs)
        {
          _backgroundJobs.Enqueue(backgroundjob);
          Monitor.Pulse(_backgroundJobs);
        }
      }
    }

    public void RunOnMain(Action mainjob)
    {
      if (Thread.CurrentThread == _mainThread)
        RunJob(mainjob);
      else
      {
        lock (_foregroundJobs)
        {
          _foregroundJobs.Enqueue(mainjob);
        }
      }
    }

    private void RunJob(Action job)
    {
      try
      {
        long t0 = Log.ElapsedTime();
        job();
        Log.ElapsedTime(t0, $"*** Finished Job {job.Target} on {(Thread.CurrentThread==_mainThread ? "Main" :Thread.CurrentThread.Name)}");
      }
      catch (Exception e)
      {
        Debug.Log(DateTime.Now+" Job failed with: "+e);
      }
    }
  }
}