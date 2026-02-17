using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace eBRestarter.Desktop.WinUI3.Behaviors
{
    /// <summary>
    /// Attached behavior that restricts a <see cref="TextBox"/> to numeric input only.
    /// </summary>
    public class TextBoxNumericOnlyBehavior : Behavior<TextBox>
    {
        // =========================================================
        // 1. CONSTANTS & STATICS (Shared regex for non-digit stripping)
        // =========================================================
        #region ConstantsAndStatics

        private static readonly Regex _regex = new("[^0-9]+");

        #endregion

        // =========================================================
        // 2. PROTECTED METHODS (Behavior lifecycle overrides)
        // =========================================================
        #region ProtectedMethods

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.TextChanging += OnTextChanging;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.TextChanging -= OnTextChanging;
        }

        #endregion

        // =========================================================
        // 3. PRIVATE HELPER METHODS (Event handlers and helpers)
        // =========================================================
        #region PrivateHelperMethods

        private void OnTextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            if (_regex.IsMatch(sender.Text))
            {
                int pos = sender.SelectionStart;
                sender.Text = _regex.Replace(sender.Text, "");
                sender.SelectionStart = System.Math.Min(pos, sender.Text.Length);
            }
        }

        #endregion
    }
}
