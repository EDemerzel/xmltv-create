using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json.Linq;

namespace TvTv2XmlTv
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Default values
            string timezone = "America/Chicago";
            string lineUpID = "USA-OTA35213";
            int days = 8;
            string fileName = "xmltv.xml";

            // Parse command line arguments
            foreach (string arg in args)
            {
                if (arg.StartsWith("--timezone="))
                {
                    timezone = arg.Substring("--timezone=".Length);
                }
                else if (arg.StartsWith("--lineUpID="))
                {
                    lineUpID = arg.Substring("--lineUpID=".Length);
                }
                else if (arg.StartsWith("--days="))
                {
                    if (int.TryParse(arg.Substring("--days=".Length), out int parsedDays))
                    {
                        days = parsedDays;
                    }
                }
                else if (arg.StartsWith("--fileName="))
                {
                    fileName = arg.Substring("--fileName=".Length);
                }
            }

            // Ensure days does not exceed the maximum limit
            if (days > 8) days = 8;

            // Create HttpClient for API requests
            using (HttpClient client = new HttpClient())
            using (XmlWriter xmlWriter = XmlWriter.Create(fileName, new XmlWriterSettings { Indent = true }))
            {
                // Construct URL for source-info-url attribute
                string url = "https://" + (Environment.GetEnvironmentVariable("HTTP_HOST") ?? "") + (Environment.GetEnvironmentVariable("REQUEST_URI") ?? "");
                DateTime now = DateTime.UtcNow;
                string startTime = now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

                // Write the start of the XML document
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteDocType("tv", null, "xmltv.dtd", null);
                xmlWriter.WriteStartElement("tv");
                xmlWriter.WriteAttributeString("date", startTime);
                xmlWriter.WriteAttributeString("source-info-url", url);
                xmlWriter.WriteAttributeString("source-info-name", "tvtv2xmltv");

                // Fetch lineup data from API
                string lineupUrl = $"https://www.tvtv.us/api/v1/lineup/{lineUpID}/channels";
                string lineupJson = await client.GetStringAsync(lineupUrl);
                JArray lineupData = JArray.Parse(lineupJson);

                // Build channels string and write channel elements
                string channels = "";
                foreach (var channel in lineupData)
                {
                    if (channel["stationId"] != null)
                        channels += channel["stationId"]?.ToString() + ",";

                    if (channel["channelNumber"] != null)
                    {
                        xmlWriter.WriteStartElement("channel");
                        xmlWriter.WriteAttributeString("id", channel["channelNumber"]?.ToString());
                        xmlWriter.WriteElementString("display-name", channel["channelNumber"]?.ToString());

                        if (channel["stationCallSign"] != null)
                            xmlWriter.WriteElementString("display-name", channel["stationCallSign"]?.ToString());

                        if (channel["logo"] != null)
                        {
                            xmlWriter.WriteStartElement("icon");
                            xmlWriter.WriteAttributeString("src", "https://www.tvtv.us" + channel["logo"]?.ToString());
                            xmlWriter.WriteEndElement(); // icon
                        }
                        xmlWriter.WriteEndElement(); // channel
                    }
                }

                // Fetch and write program data for each day
                for (int day = 0; day < days; day++)
                {
                    now = DateTime.UtcNow.AddDays(day);
                    DateTime end = DateTime.UtcNow.AddDays(day + 1);
                    startTime = now.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
                    string endTime = end.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

                    string listingUrl = $"https://www.tvtv.us/api/v1/lineup/{lineUpID}/grid/{startTime}/{endTime}/{channels}";
                    string listingJson = await client.GetStringAsync(listingUrl);
                    JArray listingData = JArray.Parse(listingJson);

                    int index = 0;
                    foreach (var channel in lineupData)
                    {
                        if (listingData[index] is JArray programs)
                        {
                            foreach (var program in programs)
                            {
                                var programData = GetProgramData(program, timezone);
                                if (programData != null)
                                {
                                    string programId = programData.Value.programId;
                                    string title = programData.Value.title;
                                    string subtitle = programData.Value.subtitle;
                                    string flags = programData.Value.flags;
                                    string type = programData.Value.type;
                                    string programStartTime = programData.Value.programStartTime;
                                    string programEndTime = programData.Value.programEndTime;
                                    string duration = programData.Value.duration;

                                    xmlWriter.WriteStartElement("programme");
                                    xmlWriter.WriteAttributeString("start", programStartTime);
                                    xmlWriter.WriteAttributeString("stop", programEndTime);
                                    xmlWriter.WriteAttributeString("duration", duration);

                                    if (channel["channelNumber"] != null)
                                        xmlWriter.WriteAttributeString("channel", channel["channelNumber"]?.ToString());

                                    xmlWriter.WriteElementString("title", title);
                                    xmlWriter.WriteElementString("sub-title", subtitle);

                                    // Write category based on type
                                    if (type == "M")
                                        xmlWriter.WriteElementString("category", "movie");
                                    if (type == "N")
                                        xmlWriter.WriteElementString("category", "news");
                                    if (type == "S")
                                        xmlWriter.WriteElementString("category", "sports");
                                    if (flags.Contains("EI"))
                                        xmlWriter.WriteElementString("category", "kids");
                                    if (flags.Contains("HD"))
                                    {
                                        xmlWriter.WriteStartElement("video");
                                        xmlWriter.WriteElementString("quality", "HDTV");
                                        xmlWriter.WriteEndElement(); // video
                                    }
                                    if (flags.Contains("Stereo"))
                                    {
                                        xmlWriter.WriteStartElement("audio");
                                        xmlWriter.WriteElementString("stereo", "stereo");
                                        xmlWriter.WriteEndElement(); // audio
                                    }
                                    if (flags.Contains("New"))
                                    {
                                        xmlWriter.WriteElementString("new", string.Empty);
                                    }

                                    xmlWriter.WriteEndElement(); // programme
                                }
                            }
                        }
                        index++;
                    }
                }
                xmlWriter.WriteEndElement(); // tv
                xmlWriter.WriteEndDocument(); // End the XML document
            }
        }

        /// <summary>
        /// Extracts program data and returns a tuple with the relevant information.
        /// </summary>
        /// <param name="program">The JToken representing the program data.</param>
        /// <param name="timezone">The timezone to convert the times to.</param>
        /// <returns>A tuple containing program information or null if extraction fails.</returns>
        public static (string programId, string title, string subtitle, string flags, string type, string programStartTime, string programEndTime, string duration)? GetProgramData(JToken program, string timezone)
        {
            try
            {
                string programId = XmlConvert.EncodeName(program["programId"]?.ToString() ?? string.Empty);
                string title = XmlConvert.EncodeName(program["title"]?.ToString() ?? string.Empty);
                string subtitle = XmlConvert.EncodeName(program["subtitle"]?.ToString() ?? string.Empty);
                string flags = string.Join(", ", program["flags"] ?? new JArray());
                string type = XmlConvert.EncodeName(program["type"]?.ToString() ?? string.Empty);
                string startTime = program["startTime"]?.ToString() ?? string.Empty;
                string duration = program["duration"]?.ToString() ?? string.Empty;
                string runTime = program["runTime"]?.ToString() ?? string.Empty;

                if (!string.IsNullOrEmpty(startTime))
                {
                    DateTime tStart = DateTime.Parse(startTime);
                    tStart = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(tStart, timezone);
                    string programStartTime = tStart.ToString("yyyyMMddHHmmss zzz");
                    tStart = tStart.AddMinutes(!string.IsNullOrEmpty(runTime) ? Convert.ToDouble(runTime) : 0);
                    string programEndTime = tStart.ToString("yyyyMMddHHmmss zzz");

                    return (programId, title, subtitle, flags, type, programStartTime, programEndTime, duration);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing program data: {ex.Message}");
            }

            return null;
        }
    }
}
