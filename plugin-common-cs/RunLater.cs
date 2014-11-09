using System;

namespace Floobits.Common
{
    public abstract class RunLater<T>
    {
        public abstract void run(T arg);
    }

    public class RunLaterAction<T> : RunLater<T>
    {
        Action<T> action;
        public RunLaterAction(Action<T> action)
        {
            this.action = action;
        }
        public override void run(T arg)
        {
            action(arg);
        }
    }

}