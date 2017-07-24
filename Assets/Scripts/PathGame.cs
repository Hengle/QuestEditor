using System.Collections.Generic;

[System.Serializable]
public class PathGame
{
    public string name;
    public string description;
    public string autor;
    public List<ChainPack> chainPacks = new List<ChainPack>();
    public List<Param> parameters = new List<Param>();
}