using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Diagnostics;
using Floobits.Common.Interfaces;
using Floobits.Common.Protocol.Buf;
using Floobits.Utilities;

namespace Floobits.Common
{
    public class EditorScheduler
    {
        public ConcurrentQueue<Action> queue_items = new ConcurrentQueue<Action>();
        private IContext context;
        // buffer ids are not removed from readOnlyBufferIds
        private class dequeueRunnableWork
        {
            private ConcurrentQueue<Action> queue;
            public dequeueRunnableWork(ConcurrentQueue<Action> queue)
            {
                this.queue = queue;
            }
            public void run()
            {
                if (queue.Count > 5)
                {
                    Flog.log("Doing %d work", queue.Count);
                }
                while (true)
                {
                    // TODO: set a limit here and continue later
                    Action action;
                    if (!queue.TryDequeue(out action))
                    {
                        return;
                    }
                    Thread thread = new Thread(action.Invoke);
                    thread.Start();
                }
            }
        }

        private Action dequeueRunnable;

        private class QueuedAction
        {
            public Buf buf;
            public RunLater<Buf> runnable;

            public QueuedAction(Buf buf, RunLater<Buf> runnable)
            {
                this.runnable = runnable;
                this.buf = buf;
            }
            public void run()
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                lock (buf)
                {
                    runnable.run(buf);
                }
                sw.Stop();
                if (sw.ElapsedMilliseconds > 200)
                {
                    Flog.log("Spent %s in ui thread", sw.ElapsedMilliseconds);
                }
            }
        }

        public EditorScheduler(IContext context)
        {
            this.context = context;
            dequeueRunnableWork dequeuerunnablework = new dequeueRunnableWork(this.queue_items);
            this.dequeueRunnable = new Action(dequeuerunnablework.run);
        }

        public void shutdown()
        {
            reset();
        }

        public void queue(Buf buf, RunLater<Buf> runnable)
        {
            if (buf == null)
            {
                Flog.log("Buf is null abandoning adding new queue action.");
                return;
            }
            QueuedAction queuedAction = new QueuedAction(buf, runnable);
            queue(new Action(queuedAction.run));
        }

        public void queue(Action runnable)
        {
            queue_items.Enqueue(runnable);
            if (queue_items.Count > 1)
            {
                return;
            }
            context.writeThread(dequeueRunnable);
        }

        public void reset()
        {
            Action action;
            while (queue_items.TryDequeue(out action)) ;
        }

    }
}

