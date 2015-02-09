using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Floobits.Common.Protocol.Handlers;
using Floobits.Common.Interfaces;

namespace Floobits.floobits_vsp
{
    /// <summary>
    /// Interaction logic for MyControl.xaml
    /// </summary>
    public partial class ChatControl : UserControl
    {
        IContext context;

        public ChatControl()
        {
            InitializeComponent();
        }

        public void SetContext(IContext context)
        {
            // FIXME this is awkward
            this.context = context;
        }

        private WindowStatus currentState = null;
        /// <summary>
        /// This is the object that will keep track of the state of the IVsWindowFrame
        /// that is hosting this control. The pane should set this property once
        /// the frame is created to enable us to stay up to date.
        /// </summary>
        public WindowStatus CurrentState
        {
            get { return currentState; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                currentState = value;
                // Subscribe to the change notification so we can update our UI
                currentState.StatusChange += new EventHandler<EventArgs>(this.RefreshValues);
                // Update the display now
                this.RefreshValues(this, null);
            }
        }

        /// <summary>
        /// This method is the call back for state changes events
        /// </summary>
        /// <param name="sender">Event senders</param>
        /// <param name="arguments">Event arguments</param>
        private void RefreshValues(object sender, EventArgs arguments)
        {
            this.Width = currentState.Width;
            this.Height = currentState.Height;
            this.InvalidateVisual();
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            FlooHandler flooHandler = context.getFlooHandler();
            if (flooHandler == null)
            {
                return;
            }
            string chatContents = this.ChatMsg.Text.Trim();
            if (chatContents.Length < 1)
            {
                return;
            }
            flooHandler.editorEventHandler.msg(chatContents);
            ChatMsg.Text = "";
            context.chat(flooHandler.state.getUsername(flooHandler.state.getMyConnectionId()), chatContents, DateTime.Now);
        }

    }
}