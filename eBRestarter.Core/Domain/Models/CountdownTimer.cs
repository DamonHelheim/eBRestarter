using System;
using System.Collections.Generic;
using System.Text;

namespace eBRestarter.Core.Domain.Models
{
    /// <summary>
    /// Verantwortlichkeit: Verwaltet nur den Zustand und die Rechenregeln der Zeit.
    /// Layer: Core (Domain Entity)
    /// </summary>
    public class CountdownTimer
    {
        public int SecondsRemaining { get; private set; }

        public CountdownTimer(int startSeconds)
        {
            // Validierung gehört auch in die Domain
            if (startSeconds < 0) throw new ArgumentException("Zeit darf nicht negativ sein.");
            SecondsRemaining = startSeconds;
        }

        public void Tick()
        {
            if (SecondsRemaining > 0)
            {
                SecondsRemaining--;
            }
        }

        // Optional: Helper für Geschäftslogik (nicht UI!)
        public bool IsFinished => SecondsRemaining == 0;
    }
}
