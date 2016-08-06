using UnityEngine;
using System.Collections;
using System;
using System.IO;

/// <summary>
/// 游戏棋盘数据
/// </summary>
public class GameData
{
    private ItemData[,] data;//全局格子二维数组
    private int rowNumber;//行数
    private int colNumber;//列数
	
	public int NumRow//get,set
	{
		get { return rowNumber; }
		set { rowNumber = value; }
	}
	
    public int NumCol//get,set
	{
		get { return colNumber; }
		set { colNumber = value; }
	}
	
    public ItemData[,] Data//get,set
	{
		get { return data; }
		set { data = value; }
	}
	
	public GameData (int row, int col)
	{
		data = new ItemData[row, col];
		colNumber = col;
		rowNumber = row;
		generate();
	}
	
    /// <summary>
    /// 对每个格子定义其位置和类型
    /// </summary>
	private void generate()
	{
		for (int row = 0; row < NumRow; row++)
		{
			for (int col = 0; col < NumCol; col++)
			{
				data[row,col] = new ItemData();
                data[row,col].type = GetRandomType();
                data[row,col].rowIndex = row;
                data[row,col].colIndex = col;
			}
		}
	}
	
    /// <summary>
    /// 重新对每个格子定义类型
    /// </summary>
	public void ReGenerate()
	{
		for (int row = 0; row < NumRow; row++)
		{
			for (int col = 0; col < NumCol; col++)
			{
                data[row,col].type = GetRandomType();
			}
		}
	}
	
    /// <summary>
    /// 获取格子的类型
    /// </summary>
	public DataType GetTypeOf(int row, int col)
	{
        return data[row,col].type;
	}
	
    /// <summary>
    /// 随机获取格子的类型
    /// </summary>
	public DataType GetRandomType()
	{
        return (DataType) UnityEngine.Random.Range(0, Enum.GetNames(typeof(DataType)).Length);
	}
	
	/// <summary>
    //重载ToString，把实例转换成文本信息.
    /// </summary>
    public override string ToString()
	{
		string result = "";
		string lineData = "";
		
		for (int row = 0; row < NumRow; row++)
		{
			lineData = "";
			for (int col = 0; col < NumCol; col++)
			{
                lineData += (int)data[row,col].type + " ";
			}
			result += lineData + "\n";
		}
		
		return result;
	}
	
    /// <summary>
    /// 从text文件加载全部格子的类型
    /// </summary>
    /// <param name="fileName">File name.</param>
	public void LoadFromTxt(string fileName)
	{
		TextAsset ta = Resources.Load(fileName) as TextAsset;//加载text文件
		StringReader sr = new StringReader(ta.text);
		
		string line = sr.ReadLine();
		string[] lineData;
		int row = 0;
		while ( line!= null)
		{
			lineData = line.Split(' ');
			for (int col = 0; col < Game.NumCol; col++)
			{
                data[row,col].type = (DataType)(int.Parse(lineData[col]));
			}
			row++;
			line = sr.ReadLine();	
		}
	}
}


