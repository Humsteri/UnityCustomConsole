To get some logs in build you might have to change your settings in PLAYER SETTINGS -> OTHER -> STACK TRACE

This will show how to add commands that will get picked up by the script. CommandHolder.cs has two commands ready for use: Clear and DestroyCommand. Inside the console prefab there is a setting for "Use Mouse Pos". When activated you can right click and it will print the ID of gameobject you are pointing at.

Tooltip will show when hovering over an command in the list. If left empty the object wont appear.

## Example usage
```csharp
using CustomConsolePackage;

[CustomCommand("Add Tooltip here")]
public void Example(string txt)
{
    Debug.Log("Hello from code " + txt);
}
```