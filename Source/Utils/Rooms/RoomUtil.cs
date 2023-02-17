#region

using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using PoliFemoBackend.Source.Enums;
using PoliFemoBackend.Source.Objects.Rooms;

#endregion

namespace PoliFemoBackend.Source.Utils.Rooms;

public static class RoomUtil
{
    private const string RoomInfoUrls = "https://www7.ceda.polimi.it/spazi/spazi/controller/";

    internal static List<object?>? GetFreeRooms(HtmlNode? table, DateTime start, DateTime stop)
    {
        if (table?.ChildNodes == null) return null;

        var shiftStart = GetShiftSlotFromTime(start);
        var shiftEnd = GetShiftSlotFromTime(stop);

        return table.ChildNodes.Where(child => child != null)
            .Select(child => CheckIfFree(child, shiftStart, shiftEnd))
            .Where(toAdd => toAdd != null).ToList();
    }

    private static object? CheckIfFree(HtmlNode? node, int shiftStart, int shiftEnd)
    {
        if (node == null) return null;

        if (!node.GetClasses().Contains("normalRow")) return null;

        if (node.ChildNodes == null) return null;

        if (!node.ChildNodes.Any(x =>
                x.HasClass("dove")
                && x.ChildNodes != null
                && x.ChildNodes.Any(x2 => x2.Name == "a" && !x2.InnerText.ToUpper().Contains("PROVA"))
            ))
            return null;

        var roomFree = IsRoomFree(node, shiftStart, shiftEnd);
        var searchInScopeResults = roomFree.Where(x => x.inScopeSearch).ToList();
        var roomFreeBool = searchInScopeResults.All(x => x is { RoomOccupancyEnum: RoomOccupancyEnum.FREE });

        return roomFreeBool == false ? null : GetAula(node, roomFree, shiftEnd);
    }

    private static List<RoomOccupancyResultObject> IsRoomFree(HtmlNode? node, int shiftStart, int shiftEnd)
    {
        if (node?.ChildNodes == null)
            return new List<RoomOccupancyResultObject>();

        var colsizetotal = 0;

        var occupied = new List<RoomOccupancyResultObject>
            { new(new TimeOnly(7, 45, 0), RoomOccupancyEnum.FREE, false) };

        // the first two children are not time slots
        for (var i = 2; i < node.ChildNodes.Count; i++)
        {
            var iTime = new TimeOnly(8, 0, 0);
            iTime = iTime.AddMinutes(colsizetotal * 15);

            var nodeChildNode = node.ChildNodes[i];

            var colsize =
                // for each column, take it's span as the colsize
                nodeChildNode.Attributes.Contains("colspan")
                    ? (int)Convert.ToInt64(nodeChildNode.Attributes["colspan"].Value)
                    : 1;

            // the time start in shifts for each column, is the previous total
            var vStart = colsizetotal;
            colsizetotal += colsize;
            var vEnd = colsizetotal; // the end is the new total (prev + colsize)


            // this is the trickery, if any column ends before the shift start or starts before
            // the shift end, then we skip
            var inScopeSearch = vEnd >= shiftStart && vStart <= shiftEnd;


            // if one of the not-skipped column represents an actual lesson, then return false,
            // the room is occupied
            var occupiedBool = !string.IsNullOrEmpty(nodeChildNode.InnerHtml.Trim());
            var roomOccupancyEnum = occupiedBool ? RoomOccupancyEnum.OCCUPIED : RoomOccupancyEnum.FREE;

            //now mark the occupancies of the room
            occupied.Add(new RoomOccupancyResultObject(iTime, roomOccupancyEnum, inScopeSearch));
        }

        // if no lesson takes place in the room in the time window, the room is free (duh)
        return occupied;
    }

    private static object GetAula(HtmlNode? node, IEnumerable<RoomOccupancyResultObject> roomOccupancyResultObjects,
        int shiftStop)
    {
        //Flag to indicate if the room has a power outlet (true/false)
        var pwr = RoomWithPower(node);
        var dove = node?.ChildNodes.First(x => x.HasClass("dove"));
        //Get Room name
        var nome = dove?.ChildNodes.First(x => x.Name == "a")?.InnerText.Trim();
        //Get Building name
        var edificio = dove?.ChildNodes.First(x => x.Name == "a")?.Attributes["title"]?.Value.Split('-')[2].Trim();
        //get room link
        var info = dove?.ChildNodes.First(x => x.Name == "a")?.Attributes["href"]?.Value;

        var occupancies = new JObject();

        foreach (var roomOccupancyResultObject in roomOccupancyResultObjects.Where(x =>
                     x._timeOnly > GetTimeFromShiftSlot(shiftStop)))
        {
            if (occupancies.Children().Any() && occupancies.Children().Last().Last().ToString() ==
                roomOccupancyResultObject.RoomOccupancyEnum.ToString()) continue;
            occupancies.Add(roomOccupancyResultObject._timeOnly.ToString(),
                roomOccupancyResultObject.RoomOccupancyEnum.ToString());
        }

        //Builds room object 
        return new
        {
            name = nome, building = edificio, power = pwr, link = RoomInfoUrls + info,
            occupancies
        };
    }

    public static async Task<JObject?> GetRoomById(int id)
    {
        var url = RoomInfoUrls + "Aula.do?" +
                  "idaula=" + id;

        var html = await HtmlUtil.DownloadHtmlAsync(url);
        if (html.IsValid() == false) return null;
        /*
        example of property tag
        <td colspan="1" rowspan="1" style="width: 33%" class="ElementInfoCard1 jaf-card-element">
			<i>Codice vano</i>
			<br>&nbsp;LCF040800S042
		</td>
        (parsing doesn't work very well, regex++)
        */
        var fetchedHtml = html.GetData() ?? "";
        string[] fields = { "Sigla", "Capienza", "Edificio", "Indirizzo" };
        string[] names = { "name", "capacity", "building", "address" };
        //other fields include "Tipologia", "Indirizzo", "Dipartimento", "Codice vano", "Postazione per studenti disabili", ...
        var properties = new JObject();
        var propLen = fields.Length;
        for (var i = 0; i < propLen; i++)
        {
            var iTag = $@"<em>{fields[i]}</em>";
            var filter = new Regex($@"{iTag}.*?<br>.*?</td>", RegexOptions.Singleline);
            var match = filter.Match(fetchedHtml);
            if (match.Success)
                properties.Add(names[i], match.Value
                    .Replace(iTag, "")
                    .Replace("<br>", "")
                    .Replace("</td>", "")
                    .Replace("&nbsp;", "")
                    .Replace("\n", "").Trim()
                );
            else
                properties.Add(names[i], null);

            if (properties[names[i]]?.ToString() == "-")
                properties[names[i]] = null;
        }

        properties["building"] = properties["building"]?.ToString().Split('-')[0].Trim();
        var json = await File.ReadAllTextAsync("Other/Examples/roomsWithPower.json");
        var data = JObject.Parse(json);
        //Retrieving the list of IDs for the room with power outlets
        var list = data["rwp"]?.Select(x => (int)x).ToArray();
        properties["power"] = list != null && list.Contains(id);
        properties["capacity"] = int.Parse(properties["capacity"]?.ToString() ?? "-1");
        return properties;
    }

    private static bool RoomWithPower(HtmlNode? node)
    {
        var dove = node?.ChildNodes.First(x => x.HasClass("dove"));
        var a = dove?.ChildNodes.First(x => x.Name == "a");

        var aulaUrl = a?.Attributes["href"].Value;

        //Get the room id, in order to see whether it has power or not
        var idAula = int.Parse(aulaUrl?.Split('=').Last() ?? string.Empty);

        var json = File.ReadAllText("Other/Examples/roomsWithPower.json");
        var data = JObject.Parse(json);

        //Retrieving the list of IDs for the room with power outlets
        var list = data["rwp"]?.Select(x => (int)x).ToArray();

        //Checking whether the room has a power outlet
        return list != null && list.Contains(idAula);
    }

    private static int GetShiftSlotFromTime(DateTime time)
    {
        var shiftSlot = (time.Hour - 8) * 4;
        shiftSlot += time.Minute / 15;
        return shiftSlot;
    }

    private static TimeOnly GetTimeFromShiftSlot(int shiftSlot)
    {
        var hour = shiftSlot / 4 + 8;
        var minute = shiftSlot % 4 * 15;
        return new TimeOnly(hour, minute, 0);
    }


    internal static async Task<List<HtmlNode>?> GetDailySituationOnDate(DateTime date, string sede)
    {
        var day = date.Day;
        var month = date.Month;
        var year = date.Year;

        if (string.IsNullOrEmpty(sede)) return null;

        var url = "https://www7.ceda.polimi.it/spazi/spazi/controller/OccupazioniGiornoEsatto.do?" +
                  "csic=" + sede +
                  "&categoria=tutte" +
                  "&tipologia=tutte" +
                  "&giorno_day=" + day +
                  "&giorno_month=" + month +
                  "&giorno_year=" + year +
                  "&jaf_giorno_date_format=dd%2FMM%2Fyyyy&evn_visualizza=";

        var html = await HtmlUtil.DownloadHtmlAsync(url);
        if (html.IsValid() == false) return null;

        var doc = new HtmlDocument();
        doc.LoadHtml(html.GetData());

        var t1 = HtmlUtil.GetElementsByTagAndClassName(doc.DocumentNode, "", "BoxInfoCard", 1);

        //Get html node tbody (table) containing the rooms' daily situation requested by the query 
        var t3 = HtmlUtil.GetElementsByTagAndClassName(t1?[0], "", "scrollContent");
        return t3;
    }
}