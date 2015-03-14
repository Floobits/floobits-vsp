using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using Floobits.Common.Interfaces;
using Floobits.Common;

namespace Floobits.floobits_vsp
{
    [ContentType("text")]
    [Export(typeof(IWpfTextViewCreationListener))]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal sealed class Listener : IWpfTextViewCreationListener 
    {
        [Import(typeof(VSPContextContainer))]
        internal VSPContextContainer ContextContainer = null;
        VSPContext context;

        public void TextViewCreated(IWpfTextView textView)
        {
            context = ContextContainer.GetVSPContext();

            // Create a Document
            context.GetVSPFactory().trackDocument(textView);

            // Set up change listeners
            textView.TextBuffer.Changed += TextBufferChanged;
            textView.Closed += TextViewClosed;
        }

        void TextViewClosed(object sender, EventArgs e)
        {
            context.GetVSPFactory().untrackDocument(sender as IWpfTextView);
        }

        void TextBufferChanged(object sender, TextContentChangedEventArgs e)
        {
            context.statusMessage(e.Changes.ToString());
        }
    }
}
