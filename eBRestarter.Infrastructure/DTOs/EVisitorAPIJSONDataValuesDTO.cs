using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization; // WICHTIG: Dieser Namespace wird benötigt

namespace eBRestarter.Infrastructure.DTOs
{
    public class EVisitorAPIJSONDataValuesDTO
    {
        public class MonthlyEarningsRoot
        {
            // [JsonPropertyName] ist hier optional, wenn der Name im JSON exakt gleich ist, 
            // aber zur Sicherheit oft besser.
            [JsonPropertyName("points_total_this_month")]
            public List<PointsTotalThisMonth> Points_Total_This_Month { get; set; } = [];
        }

        public class PointsTotalThisMonth
        {
            [JsonPropertyName("from")]
            public string From { get; set; } = string.Empty;

            [JsonPropertyName("from_w3c")]
            public DateTime From_w3c { get; set; }

            [JsonPropertyName("to")]
            public string To { get; set; } = string.Empty;

            [JsonPropertyName("to_w3c")]
            public DateTime To_w3c { get; set; }

            [JsonPropertyName("value")]
            public string Value { get; set; } = string.Empty;
        }

        public class IPData
        {
            [JsonPropertyName("ip")]
            public string IP { get; set; } = string.Empty;

            [JsonPropertyName("host")]
            public string Host { get; set; } = string.Empty;

            [JsonPropertyName("countryCode")]
            public string CountryCode { get; set; } = string.Empty;

            [JsonPropertyName("countryName")]
            public string CountryName { get; set; } = string.Empty;
        }

        public class HourlyEarning
        {
            // Das Array wird nicht serialisiert (da kein Attribut und private Logik),
            // sondern dient als Speicher für die Properties unten. Das funktioniert weiterhin super.
            private readonly double[] earningsHourly = new double[24];

            [JsonIgnore] // Sicherstellen, dass dieses Feld nicht versehentlich serialisiert wird
            public double[] EarningsHourly => earningsHourly;

            [JsonPropertyName("1")]
            public double _1 { get => earningsHourly[0]; set => earningsHourly[0] = value; }

            [JsonPropertyName("2")]
            public double _2 { get => earningsHourly[1]; set => earningsHourly[1] = value; }

            [JsonPropertyName("3")]
            public double _3 { get => earningsHourly[2]; set => earningsHourly[2] = value; }

            [JsonPropertyName("4")]
            public double _4 { get => earningsHourly[3]; set => earningsHourly[3] = value; }

            [JsonPropertyName("5")]
            public double _5 { get => earningsHourly[4]; set => earningsHourly[4] = value; }

            [JsonPropertyName("6")]
            public double _6 { get => earningsHourly[5]; set => earningsHourly[5] = value; }

            [JsonPropertyName("7")]
            public double _7 { get => earningsHourly[6]; set => earningsHourly[6] = value; }

            [JsonPropertyName("8")]
            public double _8 { get => earningsHourly[7]; set => earningsHourly[7] = value; }

            [JsonPropertyName("9")]
            public double _9 { get => earningsHourly[8]; set => earningsHourly[8] = value; }

            [JsonPropertyName("10")]
            public double _10 { get => earningsHourly[9]; set => earningsHourly[9] = value; }

            [JsonPropertyName("11")]
            public double _11 { get => earningsHourly[10]; set => earningsHourly[10] = value; }

            [JsonPropertyName("12")]
            public double _12 { get => earningsHourly[11]; set => earningsHourly[11] = value; }

            [JsonPropertyName("13")]
            public double _13 { get => earningsHourly[12]; set => earningsHourly[12] = value; }

            [JsonPropertyName("14")]
            public double _14 { get => earningsHourly[13]; set => earningsHourly[13] = value; }

            [JsonPropertyName("15")]
            public double _15 { get => earningsHourly[14]; set => earningsHourly[14] = value; }

            [JsonPropertyName("16")]
            public double _16 { get => earningsHourly[15]; set => earningsHourly[15] = value; }

            [JsonPropertyName("17")]
            public double _17 { get => earningsHourly[16]; set => earningsHourly[16] = value; }

            [JsonPropertyName("18")]
            public double _18 { get => earningsHourly[17]; set => earningsHourly[17] = value; }

            [JsonPropertyName("19")]
            public double _19 { get => earningsHourly[18]; set => earningsHourly[18] = value; }

            [JsonPropertyName("20")]
            public double _20 { get => earningsHourly[19]; set => earningsHourly[19] = value; }

            [JsonPropertyName("21")]
            public double _21 { get => earningsHourly[20]; set => earningsHourly[20] = value; }

            [JsonPropertyName("22")]
            public double _22 { get => earningsHourly[21]; set => earningsHourly[21] = value; }

            [JsonPropertyName("23")]
            public double _23 { get => earningsHourly[22]; set => earningsHourly[22] = value; }

            [JsonPropertyName("24")]
            public double _24 { get => earningsHourly[23]; set => earningsHourly[23] = value; }
        }
    }
}
