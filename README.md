You need to import TMP Essential Resources


This will show how to add commands that will get picked up by the script. CommandHolder.cs has two commands ready for use.

Tooltip will show when hovering over an command in the list. If left empty the object wont appear.

## Example usage
```csharp
[CustomCommand("Add Tooltip here")]
void Example(string txt)
{
    Debug.Log("Hello from code " + txt);
}
```