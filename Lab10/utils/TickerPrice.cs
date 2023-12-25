using System;
using System.Net.Http;
using System.Threading.Tasks;
using Lab10.utils;

namespace Lab10.utils
{
    public class TickerPrice
    {
        public static async Task<double> GetTodayPrice(string ticker)
        {
            DateTime today = DateTime.Today;
            DateTime yesterday = today.AddDays(-1);

            long todayUnixTimestamp = ((DateTimeOffset)today).ToUnixTimeSeconds();
            long yesterdayUnixTimestamp = ((DateTimeOffset)yesterday).ToUnixTimeSeconds();

            string url = $"https://query1.finance.yahoo.com/v7/finance/download/" +
                            $"{ticker}?period1={yesterdayUnixTimestamp}&period2={todayUnixTimestamp}" +
                            "&interval=1d&events=history&includeAdjustedClose=true";

            string response = await HTTPRequest.Request(url);

            var parsedResponse = await Parser.Parse(response);
            // Console.WriteLine(parsedResponse[1]);
            return Convert.ToDouble(parsedResponse[1].Replace('.', ','));
        }

        public static async Task<double> GetYesterdayPrice(string ticker)
        {
            DateTime yesterday = DateTime.Today.AddDays(-1);
            DateTime twoDaysAgo = yesterday.AddDays(-1);

            long yesterdayUnixTimestamp = ((DateTimeOffset)yesterday).ToUnixTimeSeconds();
            long twoDaysAgoUnixTimestamp = ((DateTimeOffset)twoDaysAgo).ToUnixTimeSeconds();

            string url = $"https://query1.finance.yahoo.com/v7/finance/download/" +
                         $"{ticker}?period1={twoDaysAgoUnixTimestamp}&period2={yesterdayUnixTimestamp}" +
                         "&interval=1d&events=history&includeAdjustedClose=true";

            string response = await HTTPRequest.Request(url);

            var parsedResponse = await Parser.Parse(response);

            return Convert.ToDouble(parsedResponse[1].Replace('.', ','));
        }
    }
}
