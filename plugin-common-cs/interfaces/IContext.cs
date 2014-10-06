using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Floobits.Common.Interfaces
{
    public interface IContext
    {
        public EditorScheduler editor;
        private ReaderWriterLockSlim rwlock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        public IFactory iFactory;

        void flashMessage(string message);
        void warnMessage(string message);
        void statusMessage(string message);
        void errorMessage(string message);

        void setTimeout(int time, Action runnable);

        void readThread(Action runnable);
        void writeThread(Action runnable);

        void shutdown();  //syncronized??
    }
}
