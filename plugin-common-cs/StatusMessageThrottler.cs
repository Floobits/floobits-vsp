using System.Collections.Generic;
using System.Text.RegularExpressions;
using Floobits.Common.Interfaces;

namespace Floobits.Common
{
    /**
     * This class prevents status message spam by throttling messages.
     */
    public class StatusMessageThrottler
    {
        private string throttleMessage;
        private int maxMessages = 10;
        private int throttleWait = 2000;
        private List<string> messages = new List<string>();
        private ScheduledFuture schedule = null;
        private IContext context;

        /**
         * Create a message throttler that doesn't spam the user.
         * @param context
         * @param throttleMessage If this message has a {.} it will show a message count.
         */
        public StatusMessageThrottler(IContext context, string throttleMessage)
        {
            this.throttleMessage = throttleMessage;
            this.context = context;
        }

        public void statusMessage(string message)
        {
            messages.Add(message);
            queueUpMessages();
        }

        private void queueUpMessages()
        {
            if (schedule != null)
            {
                return;
            }
            schedule = context.setTimeout(throttleWait, delegate
            {
                clearMessages();
            });
        }

        private void clearMessages()
        {
            int numMessages = messages.Count;

            if (numMessages <= maxMessages)
            {
                foreach (string message in messages)
                {
                    context.statusMessage(message);
                }
                messages.Clear();
                return;
            }

            if (Regex.IsMatch(throttleMessage, "{.}"))
            {
                context.statusMessage(string.Format(throttleMessage, numMessages));
            }
            else
            {
                context.statusMessage(throttleMessage);
            }
            foreach (string message in messages)
            {
                context.chatStatusMessage(message);
            }
            messages.Clear();
            schedule = null;
        }
    }
}
