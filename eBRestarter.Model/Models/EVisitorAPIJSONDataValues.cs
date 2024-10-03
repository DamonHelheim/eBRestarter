using Newtonsoft.Json;

namespace eBRestarter.Model.Models
{
    [JsonObject]
    public class EVisitorAPIJSONDataValues
    {
        public class MonthlyEarningsRoot
        {
            public List<PointsTotalThisMonth> Points_Total_This_Month { get; set; } = [];
        }

        public class PointsTotalThisMonth
        {
            [JsonProperty("from")]
            public string From { get; set; } = string.Empty;

            [JsonProperty("from_w3c")]
            public DateTime From_w3c { get; set; }

            [JsonProperty("to")]
            public string To { get; set; } = string.Empty;

            [JsonProperty("to_w3c")]
            public DateTime To_w3c { get; set; }

            [JsonProperty("value")]
            public string Value { get; set; } = string.Empty;
        }


        public class IPData
        {
            [JsonProperty("ip")]
            public string IP { get; set; } = string.Empty;

            [JsonProperty("host")]
            public string Host { get; set; } = string.Empty;

            [JsonProperty("countryCode")]
            public string CountryCode { get; set; } = string.Empty;

            [JsonProperty("countryName")]
            public string CountryName { get; set; } = string.Empty;
        }

        public class HourlyEarning
        {

            private readonly double[] earningsHourly = new double[24];
            public double[] EarningsHourly { get => earningsHourly; }

            [JsonProperty("1")]
            public double _1 { get => earningsHourly[0]; set => earningsHourly[0] = value; }

            [JsonProperty("2")]
            public double _2 { get => earningsHourly[1]; set => earningsHourly[1] = value; }

            [JsonProperty("3")]
            public double _3 { get => earningsHourly[2]; set => earningsHourly[2] = value; }

            [JsonProperty("4")]
            public double _4 { get => earningsHourly[3]; set => earningsHourly[3] = value; }

            [JsonProperty("5")]
            public double _5 { get => earningsHourly[4]; set => earningsHourly[4] = value; }

            [JsonProperty("6")]
            public double _6 { get => earningsHourly[5]; set => earningsHourly[5] = value; }

            [JsonProperty("7")]
            public double _7 { get => earningsHourly[6]; set => earningsHourly[6] = value; }

            [JsonProperty("8")]
            public double _8 { get => earningsHourly[7]; set => earningsHourly[7] = value; }

            [JsonProperty("9")]
            public double _9 { get => earningsHourly[8]; set => earningsHourly[8] = value; }

            [JsonProperty("10")]
            public double _10 { get => earningsHourly[9]; set => earningsHourly[9] = value; }

            [JsonProperty("11")]
            public double _11 { get => earningsHourly[10]; set => earningsHourly[10] = value; }

            [JsonProperty("12")]
            public double _12 { get => earningsHourly[11]; set => earningsHourly[11] = value; }

            [JsonProperty("13")]
            public double _13 { get => earningsHourly[12]; set => earningsHourly[12] = value; }

            [JsonProperty("14")]
            public double _14 { get => earningsHourly[13]; set => earningsHourly[13] = value; }

            [JsonProperty("15")]
            public double _15 { get => earningsHourly[14]; set => earningsHourly[14] = value; }

            [JsonProperty("16")]
            public double _16 { get => earningsHourly[15]; set => earningsHourly[15] = value; }

            [JsonProperty("17")]
            public double _17 { get => earningsHourly[16]; set => earningsHourly[16] = value; }

            [JsonProperty("18")]
            public double _18 { get => earningsHourly[17]; set => earningsHourly[17] = value; }

            [JsonProperty("19")]
            public double _19 { get => earningsHourly[18]; set => earningsHourly[18] = value; }

            [JsonProperty("20")]
            public double _20 { get => earningsHourly[19]; set => earningsHourly[19] = value; }

            [JsonProperty("21")]
            public double _21 { get => earningsHourly[20]; set => earningsHourly[20] = value; }

            [JsonProperty("22")]
            public double _22 { get => earningsHourly[21]; set => earningsHourly[21] = value; }

            [JsonProperty("23")]
            public double _23 { get => earningsHourly[22]; set => earningsHourly[22] = value; }

            [JsonProperty("24")]
            public double _24 { get => earningsHourly[23]; set => earningsHourly[23] = value; }

        }
    }
}
