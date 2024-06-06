using System.Text;

if (args.Length != 2)
{
    Console.WriteLine("Invalid parameters!");
    Console.WriteLine("Usage: BackupTool [source] [target]");
    return;
}
string Source = args[0];
if (!Directory.Exists(Source))
{
    Console.WriteLine("Invalid source!");
    Console.WriteLine("Usage: BackupTool [source] [target]");
    return;
}
string Target = args[1];
if (!Directory.Exists(Target))
{
    Console.WriteLine("Invalid target!");
    Console.WriteLine("Usage: BackupTool [source] [target]");
    return;
}

StateTree State;
if (File.Exists(Target + "/BackupState.bin"))
{
    //target contains BackupState.bin, so the backup will be updated
    State = StateTree.Load(Target + "/BackupState.bin");
}
else
{
    if (Directory.GetFiles(Target).Length > 0 || Directory.GetDirectories(Target).Length > 0)
    {
        Console.WriteLine("The target doesn't contain BackupState.bin but isn't empty!");
        Console.WriteLine("Usage: BackupTool [source] [target]");
        return;
    }

    //target doesn't contain BackupState.bin, so a fresh backup will be created
    State = new();
}

//run backup
Backup(Source, Target, State);

//save new state
File.WriteAllText(Target + "/BackupState.bin", State.Encode());

//done
Console.WriteLine("Done!");



static void Backup(string source, string target, StateTree state)
{
    //remove deleted directories
    foreach (var kv in state.Directories)
        if (!Directory.Exists(source + '/' + kv.Key))
        {
            Directory.Delete(target + '/' + kv.Key, true);
            state.Directories.Remove(kv.Key);
        }

    //remove deleted files
    foreach (var kv in state.Files)
        if (!File.Exists(source + '/' + kv.Key))
        {
            File.Delete(target + '/' + kv.Key);
            state.Directories.Remove(kv.Key);
        }

    DirectoryInfo sourceInfo = new(source);

    //add/update directories
    foreach (var directory in sourceInfo.GetDirectories().Select(x => x.Name))
    {
        if (!state.Directories.TryGetValue(directory, out var subState))
        {
            subState = new();
            state.Directories[directory] = subState;
            Directory.CreateDirectory(target + '/' + directory);
        }

        Backup(source + '/' + directory, target + '/' + directory, subState);
    }

    //add/update files
    foreach (var file in sourceInfo.GetFiles().Select(x => x.Name))
    {
        string timestamp = File.GetLastWriteTimeUtc(source + '/' + file).Ticks.ToString();
        if ((!state.Files.TryGetValue(file, out var savedTimestamp)) || savedTimestamp != timestamp)
        {
            state.Files[file] = timestamp;
            File.Copy(source + '/' + file, target + '/' + file, true);
        }
    }
}

public class StateTree
{
    public Dictionary<string, StateTree> Directories = [];

    public Dictionary<string, string> Files = [];

    public string Encode()
        => string.Join(';',
            [
                .. Directories.Select(x => $"{x.Key}=({x.Value.Encode()})"),
                .. Files.Select(x => $"{x.Key}={x.Value}")
            ]);

    public static StateTree Load(string path)
    {
        using StreamReader reader = new(path);
        StateTree result = new();
        result.LoadRecursive(reader);
        return result;
    }

    private void LoadRecursive(StreamReader reader)
    {
        int read;

        while (true)
        {
            //key
            StringBuilder keyBuilder = new();
            read = reader.Read();
            if ((char)read == ')')
                return;
            while (read != -1 && (char)read != '=')
            {
                keyBuilder.Append((char)read);
                read = reader.Read();
            }
            if (read == -1)
                return;
            string key = keyBuilder.ToString();

            //value
            read = reader.Read();
            switch ((char)read)
            {
                case '(': //directory
                    if (!Directories.TryGetValue(key, out var subTree))
                    {
                        subTree = new();
                        Directories[key] = subTree;
                    }
                    subTree.LoadRecursive(reader);
                    read = reader.Read();
                    break;
                default: //file
                    StringBuilder valueBuilder = new();
                    while (read != -1 && !";)".Contains((char)read))
                    {
                        valueBuilder.Append((char)read);
                        read = reader.Read();
                    }
                    Files[key] = valueBuilder.ToString();
                    break;
            }
            switch (read)
            {
                case -1:
                case ')':
                    return;
                    //the only possible alternative is a ; but nothing needs to be done in that case
            }
        }
    }
}