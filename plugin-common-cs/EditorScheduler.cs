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
        public ConcurrentQueue<ThreadStart> queue_items = new ConcurrentQueue<ThreadStart>();
        private IContext context;
        // buffer ids are not removed from readOnlyBufferIds
        private class dequeueRunnableWork
        {
            private ConcurrentQueue<ThreadStart> queue;
            public dequeueRunnableWork(ConcurrentQueue<ThreadStart> queue)
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
                    ThreadStart action;
                    if (!queue.TryDequeue(out action))
                    {
                        return;
                    }
                    Thread thread = new Thread(action);
                    thread.Start();
                }
            }
        }

        private ThreadStart dequeueRunnable;

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
            this.dequeueRunnable = new ThreadStart(dequeuerunnablework.run);
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
            queue(new ThreadStart(queuedAction.run));
        }

        public void queue(ThreadStart runnable)
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
            ThreadStart action;
            while (queue_items.TryDequeue(out action)) ;
        }

    }
}

