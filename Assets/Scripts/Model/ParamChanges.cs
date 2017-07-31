using System.Collections.Generic;

public class ParamChanges
{
    public Param aimParam;
    public List<Param> parameters = new List<Param>();
    public string changeString = "";

	public ParamChanges(Param aimParam)
	{
		this.aimParam = aimParam;
	}
}