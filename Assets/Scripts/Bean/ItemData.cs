using System;

/// <summary>
/// 格子的属性
/// </summary>
public class ItemData
{
    public DataType type;//类型
    public int rowIndex;//行
    public int colIndex;//列
	
	public ItemData ()
	{
	}
}

public enum DataType//格子类型枚举
{
    Green,
    Red,
    Pink,
}


