This simple script allows to easily configure game saving.

* No need to call initialization anywhere. No need to create save file manually. First call to Save { get; } will load or create new save and provide valid data. 
* Works even before scene load.
* Has XOR encryption. You can setup ecryption key or enable/disable encryption in release or dev builds through settings asset.
* If you want to modify saves path you should create GameSaverSettings asset in any Resources folder (Create -> Game Saver -> Game Saver Settings). Keep default name of this asset.

How to use:

* Create a class that represents your saved data, make shure it is [System.Serializable].
* Create a new class for your custom game saver and call it whatever you want, derive it from GameSaverGeneric and pass your save game class as T parameter;

```csharp
using Recstazy.GameSaver;

// Your save struct, represents what you whant to save
[System.Serializable]
public class SaveGame
{
    public int LastSavedLevel = 0;
    public string PlayerName = "";
    public double Currency = 0;
}

// Your game saver class
public class GameSaver : GameSaverGeneric<SaveGame>
{
    //Custom code if needed
}
```

* To save something set it to the Save property of your game saver class and then call SaveChanged();
* To get some saved parameters just use Save property;

```csharp
public string GetPlayerName()
{
    return GameSaver.Save.PlayerName;
}

public void SavePlayerName(string name)
{
    GameSaver.Save.PlayerName = name;
    GameSaver.SaveChanged();
}
```

* You can implement **ISaveLoadReciever** interface in your save class to get callbacks before serialize and after deserialize like unity IserializationCallbackReciever.
* You also can subscribe to Save-Load events in your game saver calss to get serialization callbacks.
___
From **v1.4.0** you can create scriptable saves and edit them in editor. Game Saver will save-load using scriptable save provided in settings.

* Create new script with a class inherited from ScriptableSaveGeneric<T>. 
* **Avoid declaring save type in the same file with anything else.** Due to unity internal things it works bad with scriptable objects.
* Provide your save type to T parameter and add [CreateAssetMenu] attrubute. 
* Create new scriptable save in editor and put it in "Save Override" of Game Saver Settings.

```csharp
using UnityEngine;
using Recstazy.GameSaver;

[CreateAssetMenu(menuName = "Game Saver/My Scriptable Save")]
public class MyScriptableSave : ScriptableSaveGeneric<SaveGame> { }
```
