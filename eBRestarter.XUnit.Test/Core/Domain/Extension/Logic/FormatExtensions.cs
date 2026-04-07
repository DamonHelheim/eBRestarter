using eBRestarter.Core.Domain.Extensions;
using Shouldly;
using System.Globalization;
using Xunit;

namespace eBRestarter.XUnit.Test.Core.Domain.Extension.Logic
{
        /// <summary>
        /// Stellt sicher, dass die Dateigrößen-Konvertierung (Bytes zu KB, MB, GB etc.)
        /// unter allen Umständen mathematisch und optisch korrekt funktioniert.
        /// </summary>
        public class FormatExtensionsTests
        {
        // =========================================================
        // 1. GRENZWERTE & STANDARD-KONVERTIERUNGEN
        // =========================================================

        /// <summary>
        /// <para>
        /// WARUM WIRD DAS GETESTET?
        /// Das ist der "Happy Path" und testet die Kernmathematik (Logarithmus zur Basis 1024).
        /// Es wird geprüft, ob die Schwellenwerte für die Umrechnung in die nächsthöhere
        /// Einheit (KB, MB, GB) exakt getroffen werden.
        /// </para>
        /// <para>
        /// WAS WIRD GETESTET?
        /// - 0 Bytes als absoluter Nullpunkt.
        /// - Werte knapp unter der Schwelle (1023 B).
        /// - Werte genau auf der Schwelle (1024 B = 1.0 KB).
        /// - Werte, die glatte Brüche ergeben (1536 / 1024 = 1.5 KB).
        /// - Höhere Dimensionen wie Megabyte (1024^2) und Gigabyte (1024^3).
        /// </para>
        /// </summary>
        [Theory]
        [InlineData(0, "0 B")]
        [InlineData(512, "512 B")]
        [InlineData(1023, "1023 B")]
        [InlineData(1024, "1.0 KB")]
        [InlineData(1536, "1.5 KB")]
        [InlineData(1048576, "1.0 MB")]
        [InlineData(1073741824, "1.0 GB")]
        public void ToSizeSuffix_ShouldFormatCorrectly_WithDefaultSettings(long inputBytes, string expectedResult)
        {
            // Act
            // WARUM InvariantCulture?
            // Tests werden oft in Cloud-Pipelines (CI/CD) ausgeführt, deren Server z.B. auf Englisch laufen.
            // Ohne InvariantCulture würde der Test auf einem deutschen Entwickler-PC ("1,5 KB") erfolgreich
            // sein, aber auf dem Cloud-Server ("1.5 KB") fehlschlagen. Wir erzwingen hier den Punkt als Trenner.
            var result = inputBytes.ToSizeSuffix(1, CultureInfo.InvariantCulture);

            // Assert (Shouldly)
            // Prüft, ob der berechnete String exakt mit dem erwarteten [InlineData]-Wert übereinstimmt.
            result.ShouldBe(expectedResult);
        }

         // =========================================================
         // 2. NEGATIVE ZAHLEN
         // =========================================================

         /// <summary>
         /// WARUM WIRD DAS GETESTET?
         /// In der realen Welt gibt es keine "negativen Dateigrößen". Aber bei der Berechnung von
         /// Speicher-Differenzen (z. B. "Wie viel Platz wurde durch den Cleanup freigegeben?")
         /// können Deltas entstehen (z. B. -500 MB). Die Methode darf dabei nicht abstürzen (z. B.
         /// durch Logarithmus von negativen Zahlen, was einen Fehler wirft), sondern muss das Minuszeichen korrekt vorsetzen.
         /// </summary>
         [Theory]
         [InlineData(-512, "-512 B")]
         [InlineData(-1048576, "-1.0 MB")]
         public void ToSizeSuffix_ShouldHandleNegativeNumbers(long inputBytes, string expectedResult)
         {
             // Act
             var result = inputBytes.ToSizeSuffix(1, CultureInfo.InvariantCulture);

             // Assert
             result.ShouldBe(expectedResult);
         }

        // =========================================================
        // 3. NACHKOMMASTELLEN (DECIMAL PLACES)
        // =========================================================

        /// <summary>
        /// <para>
        /// WARUM WIRD DAS GETESTET?
        /// Stellt sicher, dass das dynamische String-Format (`$"n{decimalPlaces}"`) korrekt
        /// angewendet wird und mathematisches Runden funktioniert.
        /// </para>
        /// <para>
        /// WAS WIRD GETESTET?
        /// Ein Wert, der exakt auf 1,25 MB hinausläuft (1.310.720 Bytes).
        /// Es wird geprüft, ob bei 1 Nachkommastelle kaufmännisch korrekt aufgerundet wird (1.3 MB),
        /// bei 2 Stellen der exakte Wert (1.25 MB) steht und bei 0 Stellen abgerundet wird (1 MB).
        /// </para>
        /// </summary>
        [Fact]
        public void ToSizeSuffix_ShouldRespectDecimalPlaces()
        {
        // Arrange
        const long inputBytes = 1310720; // 1310720 / 1024 / 1024 = 1.25

        // Act
        var oneDecimal = inputBytes.ToSizeSuffix(1, CultureInfo.InvariantCulture);
            var twoDecimals = inputBytes.ToSizeSuffix(2, CultureInfo.InvariantCulture);
            var zeroDecimals = inputBytes.ToSizeSuffix(0, CultureInfo.InvariantCulture);

            // Assert (Shouldly)
            oneDecimal.ShouldBe("1.3 MB"); // Prüft korrekte Aufrundung
            twoDecimals.ShouldBe("1.25 MB"); // Prüft exakten Wert
            zeroDecimals.ShouldBe("1 MB"); // Prüft Abschneiden / Abrunden
        }

        // =========================================================
        // 4. LOKALISIERUNG (CULTURE INFO)
        // =========================================================

        /// <summary>
        /// <para>
        /// WARUM WIRD DAS GETESTET?
        /// Ein klassisches Fehlerpotenzial in global genutzter Software sind unterschiedliche
        /// Dezimaltrennzeichen. Die Methode muss in der Lage sein, sich der Sprache des PCs anzupassen.
        /// </para>
        /// <para>
        /// WAS WIRD GETESTET?
        /// Ein und dieselbe Byte-Zahl wird einmal der deutschen Kultur (erwartet ein Komma) und
        /// einmal der amerikanischen Kultur (erwartet einen Punkt) übergeben.
        /// </para>
        /// </summary>
        [Fact]
        public void ToSizeSuffix_ShouldRespectCultureInfo()
        {
            // Arrange
            const long inputBytes = 1536; // 1536 / 1024 = 1.5 (bzw. 1,5)

            var germanCulture = new CultureInfo("de-DE");
            var englishCulture = new CultureInfo("en-US");

            // Act
            var germanResult = inputBytes.ToSizeSuffix(1, germanCulture);
            var englishResult = inputBytes.ToSizeSuffix(1, englishCulture);

            // Assert
            germanResult.ShouldBe("1,5 KB"); // Prüft, ob ein Komma in Deutschland verwendet wird
            englishResult.ShouldBe("1.5 KB"); // Prüft, ob ein Punkt im Englischen verwendet wird
        }

        // =========================================================
        // 5. EXTREM GROSSE ZAHLEN (OVERFLOW SCHUTZ)
        // =========================================================

        /// <summary>
        /// <para>
        /// WARUM WIRD DAS GETESTET?
        /// Das ist ein reiner "Edge Case" (Extremfall) Test.
        /// Wenn jemand die Methode mit der allergrößten Zahl aufruft, die ein C# `long` überhaupt
        /// fassen kann (ca. 9.2 Exabytes), darf das Programm nicht abstürzen (z.B. durch eine
        /// IndexOutOfRangeException auf dem Suffix-Array).
        /// </para>
        /// <para>
        /// WAS WIRD GETESTET?
        /// long.MaxValue wird übergeben. Es wird sichergestellt, dass die Methode gracefully
        /// beim höchsten Suffix (EB = Exabytes) stoppt.
        /// </para>
        /// </summary>
        [Fact]
        public void ToSizeSuffix_ShouldNotThrowOnExtremelyLargeNumbers()
        {
        // Arrange
        const long maxLong = long.MaxValue; // 9.223.372.036.854.775.807 Bytes

        // Act
        // Führe die Konvertierung aus. Wenn hier eine Exception fliegt, schlägt der Test automatisch fehl.
        var result = maxLong.ToSizeSuffix(2, CultureInfo.InvariantCulture);

            // Assert
            // Wir prüfen, ob die Methode den Array-Überlauf verhindert hat und das höchste
            // verfügbare Suffix "EB" (Exabytes) herangezogen wurde.
            result.ShouldEndWith("EB");

            // Mathematische Prüfung (ca. 8.00 EB, weil long limit)
            result.ShouldBe("8.00 EB");
        }
        }
    }
