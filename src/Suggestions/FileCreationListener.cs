﻿using System;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows.Threading;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

namespace SolutionExtensions
{
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType("text")]
    [ContentType("plaintext")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    class SourceFileCreationListener : IVsTextViewCreationListener
    {
        [Import]
        public IVsEditorAdaptersFactoryService EditorAdaptersFactoryService { get; set; }

        [Import]
        public ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            IWpfTextView textView = EditorAdaptersFactoryService.GetWpfTextView(textViewAdapter);

            // Check that the content type isn't already registered for this file type.
            //if (textView.TextBuffer.ContentType.DisplayName != "plaintext")
            //    return;

            ITextDocument document;

            if (TextDocumentFactoryService.TryGetTextDocument(textView.TextDataModel.DocumentBuffer, out document))
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(new Action(async () =>
                {
                    await HandleSuggestions(document);

                }), DispatcherPriority.ApplicationIdle, null);
            }
        }

        private static async System.Threading.Tasks.Task HandleSuggestions(ITextDocument document)
        {
            string fileType = Path.GetFileName(document.FilePath);
            var extensions = await SuggestionHandler.Instance.GetSuggestions(fileType);
            int count = extensions.Count();

            if (count > 0)
            {
                InfoBarService.Instance.ShowInfoBar(count, fileType);
            }
        }
    }
}
