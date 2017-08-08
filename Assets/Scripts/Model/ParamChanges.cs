using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ParamChanges
{
    public Param aimParam
    {
        get
        {   
            return  GUIDManager.GetItemByGuid(aimParamGuid);
        }
        set
        {
            aimParamGuid = value.paramGUID;
        } 
    }

    public int aimParamGuid;
	public List<Param> parameters
	{
		get
		{
			List<Param> par = new List<Param> ();
			foreach(int pg in ParametersGUID)
			{
				par.Add (GUIDManager.GetItemByGuid(pg));
			}
			return par;
		}
		set
		{
			ParametersGUID = new List<int> ();
			foreach(Param p in value)
			{
				ParametersGUID.Add (p.paramGUID);
			}
		}
	}

    public void setParam(Param p, int index)
    {
        ParametersGUID[index] = p.paramGUID;
    }

	public List<int> ParametersGUID = new List<int>();
    public string changeString = "";

	public ParamChanges(Param aimParam)
	{
		this.aimParam = aimParam;
	}

	public void RemoveParam(Param p)
	{
		ParametersGUID.Remove (p.paramGUID);
	}
	public void AddParam(Param p)
	{
		ParametersGUID.Add(p.paramGUID);
	}
}