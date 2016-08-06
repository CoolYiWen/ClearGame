using System;

public class RectData
{
    public ItemData item_LeftTop;
    public ItemData item_RightBottom;
	
	public ItemData LeftTop
	{
		get { return item_LeftTop; }
		set { item_LeftTop = value; }
	}
	
	public ItemData RightBottom
	{
		get { return item_RightBottom; }
		set { item_RightBottom = value; }
	}
	
	public RectData ()
	{
	}
	
    public override string ToString()
	{
		string result = "";
        result += "LeftTop:["+item_LeftTop.rowIndex+","+item_LeftTop.colIndex+"]";
        result += "RightBottom:["+item_RightBottom.rowIndex+","+item_RightBottom.colIndex+"]";
		
		return result;
	}
}


