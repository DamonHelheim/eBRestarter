using Microsoft.UI.Xaml.Controls;
using Microsoft.Xaml.Interactivity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace eBRestarter.Desktop.WinUI3.Behaviors
{
    public class TextBoxNumericOnlyBehavior : Behavior<TextBox>
    {
        // Regex für "Nur Zahlen"
        private static readonly Regex _regex = new("[^0-9]+");

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

        private void OnTextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            // Wenn der neue Text keine Zahl ist, verwerfen wir die Änderung
            if (_regex.IsMatch(sender.Text))
            {
                // Setze den Text auf den alten Wert zurück oder entferne ungültige Zeichen
                // Einfache Variante: Cursor-Position merken und ungültige Zeichen entfernen
                int pos = sender.SelectionStart;
                sender.Text = _regex.Replace(sender.Text, "");
                sender.SelectionStart = System.Math.Min(pos, sender.Text.Length);
            }
        }
    }
}
