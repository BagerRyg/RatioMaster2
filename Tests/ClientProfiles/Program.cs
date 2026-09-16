using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using RatioMaster;

internal static class Program
{
    private static readonly Type CatalogType = typeof(TorrentClient).Assembly.GetType("RatioMaster.TorrentClientsEnum");
    private static int assertions;

    [STAThread]
    private static void Main()
    {
        TorrentClient[] clients = ReadClients();
        Check(clients.Count(c => c.IsDivider) == 1, "One legacy divider");
        Check(clients.All(c => c != null), "No empty dropdown entries");
        Check(clients.Where(c => !c.IsDivider).Select(c => c.ProfileId).Distinct().Count() == clients.Length - 1, "Unique profile IDs");

        MainForm form = (MainForm)RuntimeHelpers.GetUninitializedObject(typeof(MainForm));
        typeof(MainForm).GetField("random", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(form, new Random(1));
        MethodInfo normalize = typeof(MainForm).GetMethod("NormalizeCommand", BindingFlags.Instance | BindingFlags.NonPublic);
        foreach (TorrentClient client in clients.Where(c => !c.IsDivider))
        {
            Check(DecodeBytes(client.PeerID).Length == 20, client.ProfileId + " peer ID length");
            form.currentClient = client;
            foreach (string eventType in new[] { "&event=started", "", "&event=completed", "&event=stopped" })
            {
                var torrent = new TorrentInfo(0, 0)
                {
                    tracker = "http://127.0.0.1:49200/announce?passkey=fixture",
                    hash = "000A0F207F80FEFF2D5F2E7E2829212A41425A6162",
                    key = client.Key,
                    peerID = client.PeerID,
                    port = "51413",
                    numberOfPeers = eventType == "&event=stopped" ? "0" : client.NumwantInitialValue.ToString()
                };
                string url = form.getUrlString(torrent, eventType);
                Check(!Regex.IsMatch(url, "\\{[^}]+\\}"), client.ProfileId + " unresolved query placeholder");
                string headers = client.Headers.Replace("{host}", "127.0.0.1:49200").Replace("{osver}", "Windows 11").Replace("{javaver}", "Java 1.8.0");
                string request = (string)normalize.Invoke(form, new object[] { "GET " + url.Substring(url.IndexOf("/announce", StringComparison.Ordinal)) + " " + client.HttpProtocol + "\r\n" + headers + "\r\n" });
                Check(request.EndsWith("\r\n\r\n"), client.ProfileId + " HTTP header terminator");
                Check(!Regex.IsMatch(request, "\\{[^}]+\\}"), client.ProfileId + " unresolved HTTP placeholder");
                var pairs = url.Substring(url.IndexOf('?') + 1).Split('&').Select(p => p.Split('=', 2)).ToArray();
                Check(DecodeBytes(pairs.Single(p => p[0] == "info_hash")[1]).SequenceEqual(Convert.FromHexString(torrent.hash)), client.ProfileId + " hash bytes");
                Check(DecodeBytes(pairs.Single(p => p[0] == "peer_id")[1]).Length == 20, client.ProfileId + " announce peer ID");
                if (client.ProfileId == "Transmission_4.1.3.client")
                {
                    Check(pairs.Single(p => p[0] == "numwant")[1] == (eventType == "&event=stopped" ? "0" : "80"), "Transmission numwant");
                    Check(!pairs.Any(p => p[0] == "corrupt" || p[0] == "requirecrypto"), "Transmission conditional parameters");
                }
                if (client.ProfileId == "rTorrent_ruTorrent_0.16.22_5.3.14.client")
                {
                    Check(!pairs.Any(p => p[0] == "numwant"), "rTorrent default numwant omitted");
                    Check(url.IndexOf("&compact=1&key=", StringComparison.Ordinal) > 0, "rTorrent parameter order");
                }
            }
        }

        Check(clients.Single(c => c.ProfileId == "Transmission_4.1.3.client").PeerID.StartsWith("-TR4130-"), "Transmission prefix");
        Check(clients.Single(c => c.ProfileId == "rTorrent_ruTorrent_0.16.22_5.3.14.client").PeerID.StartsWith("-lt1016-"), "rTorrent library prefix");
        Check(clients.Single(c => c.ProfileId == "Halite_0.4.0.4.client").Headers.Contains("Halite v 0.4.0.4"), "Halite user agent");
        foreach (string profile in new[] { "uTorrent_3.6.0_build_47254.client", "BitTorrent_7.11.0_build_47255.client" })
        {
            TorrentClient client = clients.Single(c => c.ProfileId == profile);
            byte[] peer = DecodeBytes(client.PeerID);
            int build = peer[8] | peer[9] << 8;
            Check(client.Headers.Contains("(" + build + ")"), profile + " build matches peer ID");
        }

        object catalog = Activator.CreateInstance(CatalogType, new object[] { null });
        MethodInfo generate = CatalogType.GetMethod("GenerateIdString", BindingFlags.Instance | BindingFlags.NonPublic, null,
            new[] { typeof(string), typeof(int), typeof(bool), typeof(bool), typeof(string), typeof(bool) }, null);
        string Generate(string type, int length, bool encode = false, bool upper = false) =>
            (string)generate.Invoke(catalog, new object[] { type, length, encode, upper, "-", false });
        bool leadingZero = false;
        bool variableLength = false;
        bool includes255 = false;
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            for (int i = 0; i < 1000; i++)
            {
                string suffix = Generate("transmission", 12);
                const string alphabet = "0123456789abcdefghijklmnopqrstuvwxyz";
                Check(suffix.Length == 12 && suffix.All(alphabet.Contains), "Transmission alphabet");
                Check(suffix.Sum(c => alphabet.IndexOf(c)) % 36 == 0, "Transmission checksum");
                string key = Generate("hex", 8, upper: true);
                Check(Regex.IsMatch(key, "^[0-9A-F]{8}$"), "Fixed-width libtorrent key");
                leadingZero |= key[0] == '0';
                string shortKey = Generate("hexVariable", 8, upper: true);
                Check(Regex.IsMatch(shortKey, "^(0|[1-9A-F][0-9A-F]{0,7})$"), "Halite variable-width key");
                variableLength |= shortKey.Length < 8;
                byte[] randomBytes = DecodeBytes(Generate("random", 12, encode: true, upper: true));
                Check(randomBytes.Length == 12, "Raw bytes survive uppercase URL encoding");
                includes255 |= randomBytes.Contains((byte)255);
            }
        }
        finally { CultureInfo.CurrentCulture = originalCulture; }
        Check(leadingZero && variableLength && includes255, "Generator coverage");

        MethodInfo resolve = CatalogType.GetMethod("ResolveClientIndex", BindingFlags.Static | BindingFlags.NonPublic);
        int Resolve(TorrentClient[] list, string id, int index) => (int)resolve.Invoke(null, new object[] { list, id, index });
        string[] oldIds = (string[])CatalogType.GetField("legacyClientIds", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
        for (int i = 0; i < oldIds.Length; i++)
        {
            if (oldIds[i].Length > 0)
                Check(clients[Resolve(clients, null, i)].ProfileId == oldIds[i], "Legacy selection " + i);
        }
        TorrentClient[] reversed = clients.Reverse().ToArray();
        const string savedId = "Transmission_4.1.3.client";
        Check(reversed[Resolve(reversed, savedId, 0)].ProfileId == savedId, "Named selection survives reorder");
        Check(clients[Resolve(clients, "missing.client", 0)].ProfileId == "qBittorrent_5.2.3.client", "Missing profile fallback");
        Check(Resolve(Array.Empty<TorrentClient>(), savedId, 0) == -1, "Empty list fallback");
        var serializer = new XmlSerializer(typeof(ApplicationSettings));
        using var writer = new StringWriter();
        serializer.Serialize(writer, new ApplicationSettings { TorrentClientId = savedId, TorrentClientsIndex = 6 });
        using var reader = new StringReader(writer.ToString());
        Check(((ApplicationSettings)serializer.Deserialize(reader)).TorrentClientId == savedId, "Saved profile ID round trip");

        // Only move generated test-output fixtures, restoring them even when an assertion fails.
        string fixturePath = Path.Combine(AppContext.BaseDirectory, "clients");
        string backupPath = Path.Combine(AppContext.BaseDirectory, "clients-" + Guid.NewGuid().ToString("N"));
        Directory.Move(fixturePath, backupPath);
        try
        {
            TorrentClient[] internalOnly = ReadClients();
            Check(internalOnly.Length == 4 && internalOnly.Count(c => c.IsDivider) == 1, "Missing directory uses internal clients without nulls");
            File.Copy(Path.Combine(backupPath, savedId), Path.Combine(fixturePath, savedId));
            TorrentClient[] partial = ReadClients();
            Check(partial.Length == 5 && partial[0].ProfileId == savedId && partial[1].IsDivider, "Divider follows available pinned profiles");
        }
        finally
        {
            File.Delete(Path.Combine(fixturePath, savedId));
            Directory.Delete(fixturePath);
            Directory.Move(backupPath, fixturePath);
        }

        Console.WriteLine($"Passed {assertions} checks across {clients.Length - 1} profiles. No tracker requests sent.");
    }

    private static TorrentClient[] ReadClients()
    {
        object catalog = Activator.CreateInstance(CatalogType, new object[] { null });
        return (TorrentClient[])CatalogType.GetProperty("TorrentClients").GetValue(catalog);
    }

    private static byte[] DecodeBytes(string value)
    {
        var bytes = new List<byte>();
        for (int i = 0; i < value.Length; i++)
        {
            if (value[i] == '%')
            {
                Check(i + 2 < value.Length && Uri.IsHexDigit(value[i + 1]) && Uri.IsHexDigit(value[i + 2]), "Valid percent escape");
                bytes.Add(Convert.ToByte(value.Substring(i + 1, 2), 16));
                i += 2;
            }
            else
            {
                Check(value[i] <= 127, "URL bytes must be ASCII");
                bytes.Add((byte)value[i]);
            }
        }
        return bytes.ToArray();
    }

    private static void Check(bool condition, string message)
    {
        assertions++;
        if (!condition) throw new Exception(message);
    }
}
