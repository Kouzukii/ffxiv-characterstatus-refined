using Lumina;
using Lumina.Excel.GeneratedSheets;
using Serilog;
using ILogger = Lumina.ILogger;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

var cats = new (string, Predicate<int>, string, Dictionary<uint, int>)[] {
    ("Two-Handed", c => c is 13, "13", new Dictionary<uint, int>()),
    ("Main-Hand", c => c is 1, "1", new Dictionary<uint, int>()),
    ("Off-Hand", c => c is 2, "2", new Dictionary<uint, int>()),
    ("Head, Hands, Feet", c => c is 3 or 5 or 8, "3 or 5 or 8", new Dictionary<uint, int>()),
    ("Body, Legs", c => c is 4 or 7, "4 or 7", new Dictionary<uint, int>()),
    ("Accessory", c => c is >= 9 and <= 12, ">= 9 and <= 12", new Dictionary<uint, int>()),
};
var gameData = new GameData(@"C:\Program Files\SquareEnix\FINAL FANTASY XIV - A Realm Reborn\game\sqpack", new SerilogLogger(),
    new LuminaOptions { LoadMultithreaded = true });
var items = gameData.Excel.GetSheet<Item>();
foreach (var item in items) {
    var cat = (int) item.EquipSlotCategory.Row;
    if (cat is >= 1 and <= 13) {
        var majorStat = 0;
        var store = cats.First(c => c.Item2(cat));
        foreach (var data in item.UnkData59) {
            if (data.BaseParam is 6 or 19 or 22 or 27 or 44 or 45 or 46) {
                var val = data.BaseParamValue + (item.UnkData73.FirstOrDefault(i => i.BaseParamSpecial == data.BaseParam)?.BaseParamValueSpecial ?? 0);
                if (val > majorStat)
                    majorStat = val;
            }
        }

        var itemLevel = item.LevelItem.Row;
        if (majorStat > store.Item4.GetValueOrDefault(itemLevel, 0)) {
            store.Item4[itemLevel] = majorStat;
        }
    }
}

{
    await using var file = File.CreateText("../../../../CharacterPanelRefined/SubstatBreakpoints.cs");
    await file.WriteLineAsync("// File created by CharacterPanelRefined.BreakpointGenerator");
    await file.WriteLineAsync("// Do not modify manually");
    await file.WriteLineAsync();
    await file.WriteLineAsync("namespace CharacterPanelRefined;");
    await file.WriteLineAsync();
    await file.WriteLineAsync("public static class SubstatBreakpoints {");
    await file.WriteLineAsync("    public static ushort? GetBreakpoint(byte equipSlotCategory, ushort itemLevel) {");
    await file.WriteLineAsync("        return equipSlotCategory switch {");

    foreach (var it in cats) {
        await file.WriteLineAsync($"            // {it.Item1}");
        await file.WriteLineAsync($"            {it.Item3} => itemLevel switch {{");
        var noLowerBound = true;
        var lastStat = 1;
        var minIlvl = 0u;
        var lastIlvl = 0u;
        foreach (var kv in it.Item4.OrderBy(kv => kv.Key)) {
            if (kv.Value > lastStat && lastIlvl > 0) {
                if (noLowerBound) {
                    await file.WriteLineAsync($"                <= {lastIlvl} => {lastStat},");
                } else if (lastIlvl == minIlvl) {
                    await file.WriteLineAsync($"                {lastIlvl} => {lastStat},");
                } else {
                    await file.WriteLineAsync($"                >= {minIlvl} and <= {lastIlvl} => {lastStat},");
                }

                minIlvl = kv.Key;
                noLowerBound = kv.Value - lastStat == 1;
                lastStat = kv.Value;
            }

            lastIlvl = kv.Key;
        }
        if (noLowerBound) {
            await file.WriteLineAsync($"                <= {lastIlvl} => {lastStat},");
        } else if (lastIlvl == minIlvl) {
            await file.WriteLineAsync($"                {lastIlvl} => {lastStat},");
        } else {
            await file.WriteLineAsync($"                >= {minIlvl} and <= {lastIlvl} => {lastStat},");
        }
        await file.WriteLineAsync("                _ => null");
        await file.WriteLineAsync("            },");
    }

    await file.WriteLineAsync("            _ => null");
    await file.WriteLineAsync("        };");
    await file.WriteLineAsync("    }");
    await file.WriteLineAsync();

    await file.WriteLineAsync("}");
}


class SerilogLogger : ILogger {
    public void Verbose(string template, params object[] values) {
        Log.Verbose(template, values);
    }

    public void Debug(string template, params object[] values) {
        Log.Debug(template, values);
    }

    public void Information(string template, params object[] values) {
        Log.Information(template, values);
    }

    public void Warning(string template, params object[] values) {
        Log.Warning(template, values);
    }

    public void Error(string template, params object[] values) {
        Log.Error(template, values);
    }

    public void Fatal(string template, params object[] values) {
        Log.Fatal(template, values);
    }
}
