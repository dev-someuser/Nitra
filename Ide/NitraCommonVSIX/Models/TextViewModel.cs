﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Text.Editor;
using Nitra.ClientServer.Messages;
using static Nitra.ClientServer.Messages.AsyncServerMessage;
using Nitra.VisualStudio.BraceMatching;
using Nitra.VisualStudio.KeyBinding;
using Microsoft.VisualStudio.Text;

namespace Nitra.VisualStudio.Models
{
  class TextViewModel : IEquatable<TextViewModel>, IDisposable
  {
    public   FileModel                FileModel { get; }
    readonly IWpfTextView             _wpfTextView;
             InteractiveHighlightingTagger _braceMatchingTaggerOpt;
    public   MatchedBrackets          MatchedBrackets      { get; private set; }
    public   FindSymbolReferences     FindSymbolReferences { get; private set; }
    readonly KeyBindingCommandFilter  _keyBindingCommandFilter;

    public TextViewModel(IWpfTextView wpfTextView, FileModel file)
    {
      _wpfTextView = wpfTextView;
      FileModel    = file;

      _keyBindingCommandFilter = new KeyBindingCommandFilter(wpfTextView, file.Server.ServiceProvider, this);
    }

    public InteractiveHighlightingTagger BraceMatchingTaggerOpt
    {
      get
      {
        if (_braceMatchingTaggerOpt == null)
        {
          var props = _wpfTextView.TextBuffer.Properties;
          if (!props.ContainsProperty(Constants.BraceMatchingTaggerKey))
            return null;

          _braceMatchingTaggerOpt = props.GetProperty<InteractiveHighlightingTagger>(Constants.BraceMatchingTaggerKey);
        }

        return _braceMatchingTaggerOpt;
      }
    }

    public bool Equals(TextViewModel other)
    {
      return _wpfTextView.Equals(other._wpfTextView);
    }

    public override bool Equals(object obj)
    {
      var other = obj as TextViewModel;

      if (other != null)
        return Equals(other);

      return false;
    }

    public override int GetHashCode()
    {
      return _wpfTextView.GetHashCode();
    }

    public override string ToString()
    {
      return _wpfTextView.ToString();
    }

    public void Dispose()
    {
      _keyBindingCommandFilter.Dispose();
      _wpfTextView.Properties.RemoveProperty(Constants.TextViewModelKey);
    }

    internal void Reset()
    {
      Update(default(MatchedBrackets));
    }

    internal void Update(MatchedBrackets matchedBrackets)
    {
      MatchedBrackets = matchedBrackets;
      BraceMatchingTaggerOpt?.Update();
    }

    internal void Update(FindSymbolReferences findSymbolReferences)
    {
      FindSymbolReferences = findSymbolReferences;
      BraceMatchingTaggerOpt?.Update();
    }

    internal void NavigateTo(ITextSnapshot snapshot, int pos)
    {
      _wpfTextView.Caret.MoveTo(new SnapshotPoint(snapshot, pos));
      _wpfTextView.ViewScroller.EnsureSpanVisible(new SnapshotSpan(snapshot, pos, 0));
    }

    internal void NavigateTo(SnapshotPoint snapshotPoint)
    {
      _wpfTextView.Caret.MoveTo(snapshotPoint);
      _wpfTextView.ViewScroller.EnsureSpanVisible(new SnapshotSpan(snapshotPoint, snapshotPoint));
    }

    internal void Navigate(int line, int column)
    {
      Navigate(_wpfTextView.TextBuffer.CurrentSnapshot, line, column);
    }

    internal void Navigate(ITextSnapshot snapshot, int line, int column)
    {
      var snapshotLine  = snapshot.GetLineFromLineNumber(line);
      var snapshotPoint = snapshotLine.Start + column;
      NavigateTo(snapshotPoint);
      _wpfTextView.ToVsTextView().SendExplicitFocus();
    }

    internal void GotoRef(SnapshotPoint point)
    {
      var fileModel = FileModel;
      var client = fileModel.Server.Client;
      client.Send(new ClientMessage.FindSymbolReferences(fileModel.Id, point.Snapshot.Version.VersionNumber - 1, point.Position));
      var msg = client.Receive<ServerMessage.FindSymbolReferences>();
    }

    internal void GotoDefn(SnapshotPoint point)
    {
      var fileModel = FileModel;
      var client = fileModel.Server.Client;
      client.Send(new ClientMessage.FindSymbolDefinitions(fileModel.Id, point.Snapshot.Version.VersionNumber - 1, point.Position));
      var msg = client.Receive<ServerMessage.FindSymbolDefinitions>();
    }
  }
}