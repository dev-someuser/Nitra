﻿using Nitra.VisualStudio;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Adornments;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

namespace XXNamespaceXX
{
  /// <summary>
  /// Adds the error list support when the view is created
  /// </summary>
  [Export(typeof(IWpfTextViewCreationListener))]
  [ContentType("XXLanguageXX")]
  [TextViewRole(PredefinedTextViewRoles.Document)]
  class ErrorListPresenterFactory : IWpfTextViewCreationListener
  {
    [Import]
    private IErrorProviderFactory ErrorProviderFactory { get; set; }

    [Import(typeof(SVsServiceProvider))]
    private IServiceProvider ServiceProvider { get; set; }

    public void TextViewCreated(IWpfTextView textView)
    {
      var buffer = textView.TextBuffer;
      var errorListManager = new ErrorListManager(ErrorProviderFactory, buffer);
      buffer.Properties.AddProperty(TextBufferProperties.ErrorListManager, errorListManager);
      // Add the error list support to the just created view
      //textView.Properties.GetOrCreateSingletonProperty<ErrorListPresenter>(() =>
      //    new ErrorListPresenter(textView, ErrorProviderFactory, ServiceProvider)
      );
    }
  }
}
